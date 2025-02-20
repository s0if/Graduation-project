using Graduation.Data;
using Graduation.DTOs.TypeToProject;
using Graduation.Model;
using Graduation.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers.Task
{
    [Route("[controller]")]
    [ApiController]
    public class TaskOperationsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public TaskOperationsController(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager)
        {
            
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpPost("AddTypeService")]
        public async Task<IActionResult> AddType(string name)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
            var role = await userManager.GetRolesAsync(requestUser);
            if (role.Contains("admin"))
            {
                TypeService typeService = new TypeService
                {
                    Name = name
                };
                var type = await dbContext.typeServices.AddAsync(typeService);
                await dbContext.SaveChangesAsync();
                return Ok(new { status = 200, message = "add typeService successful" });
            }
            return Unauthorized();
        }

        [HttpPost("AddTypeProperty")]
        public async Task<IActionResult> AddTypeProperty(string name)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
            var role = await userManager.GetRolesAsync(requestUser);
            if (role.Contains("admin"))
            {
                TypeProperty typeProperty = new TypeProperty
                {
                    Name = name
                };
                var type = await dbContext.typeProperties.AddAsync(typeProperty);
                await dbContext.SaveChangesAsync();
                return Ok(new { status = 200, message = "add typeProperty successful" });
            }
            return Unauthorized();
        }
        [HttpGet("GetAllService")]
        public async Task<IActionResult> GetAllService()
        {
            IEnumerable<TypeService> result = await dbContext.typeServices.ToListAsync();
            IEnumerable<GetTypeDTOs> typeService = result.Adapt<IEnumerable<GetTypeDTOs>>();
            return Ok(new { status = 200, typeService });
        }
        [HttpGet("GetAllProperty")]
        public async Task<IActionResult> GetAllProperty()
        {
            IEnumerable<TypeProperty> result = await dbContext.typeProperties.ToListAsync();
            IEnumerable<GetTypeDTOs> typeProperty = result.Adapt<IEnumerable<GetTypeDTOs>>();
            return Ok(new { status = 200, typeProperty });
        }
    }
}
