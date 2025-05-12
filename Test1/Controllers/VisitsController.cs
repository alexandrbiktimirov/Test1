using Microsoft.AspNetCore.Mvc;
using Template_2.Exceptions;
using Template_2.Models.DTOs;
using Test1.Services;

namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController(IDbService dbService) : ControllerBase
    {
        private IDbService _dbService = dbService;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(int id)
        {
            VisitDto result;

            try
            {
                result = await _dbService.GetVisit(id);
            }
            catch (VisitDoesNotExistException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post(VisitPostDto dto)
        {
            try
            {
                await _dbService.PostVisit(dto);
            }
            catch (ClientDoesNotExistException e)
            {
                return BadRequest(e.Message);
            }
            catch (MechanicDoesNotExistException e)
            {
                return BadRequest(e.Message);
            }
            catch (ServiceDoesNotExistException e)
            {
                return BadRequest(e.Message);
            }
            catch (VisitAlreadyExistsException e)
            {
                return BadRequest(e.Message);
            }
            catch (ValidationFailedException e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok(dto);
        }
    }
}

