using Graduation.Data;
using Graduation.DTOs.Advertisement;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers.Advertisement
{
    [Route("[controller]")]
    [ApiController]
    public class AdvertisementController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public AdvertisementController(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpPost("AddServiceAdvertisement")]
        public async Task<IActionResult> AddServiceAdvertisement(AddServiceAdvertisementDTOs request)
        {
            if(ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider") || role.Contains("admin"))
                {
                    AdvertisementProject advertisementProject = new AdvertisementProject
                    {
                        StartAt = request.StartAt,
                        EndAt = request.EndAt,

                    };
                    dbContext.advertisements.Add(advertisementProject);
                    await dbContext.SaveChangesAsync();
                    ServiceProject result=await dbContext.services.FindAsync(request.serviceId);
                    result.AdvertisementID = advertisementProject.Id;
                    dbContext.services.UpdateRange(result);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { message = "add successful Advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpPost("AddPropertyAdvertisement")]
        public async Task<IActionResult> AddPropertyAdvertisement(AddPropertyAdvertisementDTOs request)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider") || role.Contains("admin"))
                {
                    AdvertisementProject advertisementProject = new AdvertisementProject
                    {
                        StartAt = request.StartAt,
                        EndAt = request.EndAt,

                    };
                    dbContext.advertisements.Add(advertisementProject);
                    await dbContext.SaveChangesAsync();
                    PropertyProject result = await dbContext.properties.FindAsync(request.PropertyId);
                    result.AdvertisementID = advertisementProject.Id;
                    dbContext.properties.UpdateRange(result);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { message = "add successful Advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }
    }
}
