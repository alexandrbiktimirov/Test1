using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Test1.Services;

public class DbService(IConfiguration configuration) : IDbService
{
    private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

    // private async Task ValidateData(dto)
    // {
    //     
    // }
    //
    // private async Task<int> GetId(int id)
    // {
    //     
    // }
    //
    // public async Task<> Get(int id)
    // {
    //     var result = new Dto();
    //     var id = await GetId(id);
    //
    //     using (var connection = new SqlConnection(_connectionString))
    //     {
    //         await connection.OpenAsync();
    //         
    //         var command = new SqlCommand("", connection);
    //         command.Parameters.AddWithValue("", );
    //
    //         return result;
    //     }
    // }
    //
    // public async Task<> Post(dto)
    // {
    //     await ValidateVisit(dto);
    //
    //     using (var connection = new SqlConnection(_connectionString))
    //     {
    //         await connection.OpenAsync();
    //
    //         await using var command = new SqlCommand();
    //         
    //         DbTransaction transaction = await connection.BeginTransactionAsync();
    //         command.Transaction = transaction as SqlTransaction;
    //
    //         try
    //         {
    //             command.Connection = connection;
    //             command.CommandText = "";
    //             command.Parameters.AddWithValue("", );
    //             command.Parameters.AddWithValue("", );
    //             command.Parameters.AddWithValue("", );
    //             
    //
    //             await command.ExecuteNonQueryAsync();
    //
    //             command.Parameters.Clear();
    //
    //             await transaction.CommitAsync();
    //             return dto;
    //         }
    //         catch (Exception)
    //         {
    //             await transaction.RollbackAsync();
    //             throw new ValidationFailedException("Validation of the data has failed");
    //         }
    //     }
    // }
}