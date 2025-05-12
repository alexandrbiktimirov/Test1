namespace Template_2.Models.DTOs;

public class VisitPostDto
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; }
    public List<VisitServiceDto> Services { get; set; }
}