using AutoSect.App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSect.App.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,User,Member")]
    public class StudentsController : ControllerBase
    {
        private AutoSectDbContext _context;
        private IHttpContextAccessor httpContextAccessor;
        public StudentsController(IHttpContextAccessor httpCtxAccessor, AutoSectDbContext context)
        {
            _context = context;
            httpContextAccessor = httpCtxAccessor;

            
        }

        [HttpGet] 
        public IActionResult Get([FromQuery] string parameter, [FromQuery] string query)
        {
            var session = httpContextAccessor.HttpContext.Session;

            var projectIdFilter = session.GetInt32("ProjectId");
            var projectCurrentUserIdFilter = session.GetString("AppUserID");

            if (string.IsNullOrEmpty(parameter) || string.IsNullOrEmpty(query))
            {
                return BadRequest("Invalid search parameters.");
            }

            var queryable = _context.Students.AsQueryable();

            switch (parameter)
            {
                case "id":
                    queryable = queryable.Where(u => u.StudentId == (long)Convert.ToDouble(query))
                        .Where(u => u.AppUserID == projectCurrentUserIdFilter && u.ProjectId == projectIdFilter);
                    break;
                case "firstName":
                    queryable = queryable.Where(u => u.FirstName.Contains(query))
                        .Where(u => u.AppUserID == projectCurrentUserIdFilter && u.ProjectId == projectIdFilter); 
                    break;
                case "lastName":
                    queryable = queryable.Where(u => u.LastName.Contains(query))
                        .Where(u => u.AppUserID == projectCurrentUserIdFilter && u.ProjectId == projectIdFilter);
                    break;
                default:
                    return BadRequest("Invalid search parameter.");
            }

            var result = queryable.Select(u => new
            {
                u.StudentId, u.LastName, u.FirstName, u.MiddleName,
                u.Gender, u.Section, u.GWA
            }).ToList();

            return Ok(result);

        }

    }
}
