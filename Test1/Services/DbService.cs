using System.Data.Common;
using Microsoft.Data.SqlClient;
using Template_2.Exceptions;
using Template_2.Models.DTOs;

namespace Test1.Services;

public class DbService(IConfiguration configuration) : IDbService
{
    private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

    private async Task ValidateVisit(VisitPostDto dto)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var visitCommand = new SqlCommand("SELECT 1 FROM Visit WHERE visit_id = @id", connection);
            visitCommand.Parameters.AddWithValue("@id", dto.VisitId);

            if (await visitCommand.ExecuteScalarAsync() is not null)
            {
                throw new VisitAlreadyExistsException("Visit already exists");
            }

            var clientCommand = new SqlCommand("SELECT 1 FROM Client WHERE client_id = @clientId", connection);
            clientCommand.Parameters.AddWithValue("@clientId", dto.ClientId);
            
            if (await clientCommand.ExecuteScalarAsync() is null)
            {
                throw new ClientDoesNotExistException("Client does not exist");
            }
            
            var mechanicCommand = new SqlCommand("SELECT 1 FROM Mechanic WHERE licence_number = @licenseNumber", connection);
            mechanicCommand.Parameters.AddWithValue("@licenseNumber", dto.MechanicLicenceNumber);
            
            if (await mechanicCommand.ExecuteScalarAsync() is null)
            {
                throw new MechanicDoesNotExistException("Mechanic does not exist");
            }
            
            var serviceCommand = new SqlCommand("SELECT 1 FROM Service WHERE name = @name", connection);
            foreach (var service in dto.Services)
            {
                serviceCommand.Parameters.AddWithValue("@name", service.ServiceName);

                if (await serviceCommand.ExecuteScalarAsync() is null)
                {
                    throw new ServiceDoesNotExistException("Service with that name does not exist");
                }
                
                serviceCommand.Parameters.Clear();
            }
        }
    }
    
    private async Task<int> GetId(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT visit_id FROM Visit WHERE visit_id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var result = Convert.ToInt32(await command.ExecuteScalarAsync() ?? throw new VisitDoesNotExistException("Visit does not exist"));
            return result;
        }
    }
    
    public async Task<VisitDto> GetVisit(int id)
    {
        var result = new VisitDto();
        var visitId = await GetId(id);
    
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            var dateCommand = new SqlCommand("SELECT date FROM Visit WHERE visit_id = @visitId", connection);
            dateCommand.Parameters.AddWithValue("@visitId", id);

            var date = Convert.ToDateTime(await dateCommand.ExecuteScalarAsync());

            var clientCommand = new SqlCommand("SELECT first_name, last_name, date_of_birth FROM Client JOIN Visit ON Client.client_id = Visit.client_id WHERE visit_id = @visitId", connection);
            clientCommand.Parameters.AddWithValue("@visitId", id);

            ClientDto client = new ClientDto();
            
            using (var reader = await clientCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    client.FirstName = reader.GetString(0);
                    client.LastName = reader.GetString(1);
                    client.DateOfBirth = reader.GetDateTime(2);
                }
            }

            var mechanicCommand =
                new SqlCommand(
                    "SELECT Mechanic.mechanic_id, licence_number FROM Mechanic JOIN Visit ON Mechanic.mechanic_id = Visit.mechanic_id WHERE visit_id = @visitId",
                    connection);
            mechanicCommand.Parameters.AddWithValue("@visitId", id);
            
            var mechanic = new MechanicDto();
            
            using (var reader = await mechanicCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    mechanic.MechanicId = reader.GetInt32(0);
                    mechanic.LicenseNumber = reader.GetString(1);
                }
            }

            var visitServicesCommand = new SqlCommand("SELECT Service.name, Visit_Service.service_fee FROM Visit_Service JOIN Service ON Visit_Service.service_id = Service.service_id WHERE visit_id = @visitId", connection);
            visitServicesCommand.Parameters.AddWithValue("@visitId", id);
            
            List<VisitServiceDto> visitServices = new List<VisitServiceDto>();

            using (var reader = await visitServicesCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    visitServices.Add(new VisitServiceDto
                    {
                        ServiceName = reader.GetString(0),
                        ServiceFee = reader.GetDecimal(1)
                    });
                }
            }

            result.Date = date;
            result.Client = client;
            result.Mechanic = mechanic;
            result.VisitServices = visitServices;
            
            return result;
        }
    }
    
    public async Task<VisitPostDto> PostVisit(VisitPostDto dto)
    {
        await ValidateVisit(dto);
        
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
        
            await using var command = new SqlCommand();
            
            DbTransaction transaction = await connection.BeginTransactionAsync();
            command.Transaction = transaction as SqlTransaction;
        
            try
            {
                command.Connection = connection;
                command.CommandText = "INSERT INTO Visit VALUES (@visitId, @clientId, @mechanicId, @date)";
                command.Parameters.AddWithValue("@visitId", dto.VisitId);
                command.Parameters.AddWithValue("@clientId", dto.ClientId);
                command.Parameters.AddWithValue("@date", DateTime.Now);

                var getMechanicIdCommand =
                    new SqlCommand("SELECT mechanic_id FROM Mechanic WHERE licence_number = @licenseNumber", connection,
                        transaction as SqlTransaction);
                getMechanicIdCommand.Parameters.AddWithValue("@licenseNumber", dto.MechanicLicenceNumber);

                command.Parameters.AddWithValue("@mechanicId",
                    Convert.ToInt32(await getMechanicIdCommand.ExecuteScalarAsync()));

                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
        
                command.CommandText = "INSERT INTO Visit_Service VALUES (@visitId, @serviceId, @serviceFee)";

                foreach (var service in dto.Services)
                {
                    command.Parameters.AddWithValue("@visitId", dto.VisitId);
                    command.Parameters.AddWithValue("@serviceFee", service.ServiceFee);
                    
                    var serviceIdCommand =
                        new SqlCommand("SELECT service_id FROM Service WHERE name = @name",
                            connection, transaction as SqlTransaction);
                    serviceIdCommand.Parameters.AddWithValue("@name", service.ServiceName);

                    command.Parameters.AddWithValue("@serviceId", Convert.ToInt32(await serviceIdCommand.ExecuteScalarAsync()));
                    await command.ExecuteNonQueryAsync();

                    command.Parameters.Clear();
                    serviceIdCommand.Parameters.Clear();
                }
                
                await transaction.CommitAsync();
                return dto;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new ValidationFailedException("Validation of the data has failed");
            }
        }
    }
}