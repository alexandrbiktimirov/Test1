using Template_2.Models.DTOs;

namespace Test1.Services;

public interface IDbService
{
    public Task<VisitDto> GetVisit(int id);
    public Task<VisitPostDto> PostVisit(VisitPostDto dto);
}