using Graduation.Data;
using Graduation.DTOs.Advertisement;
using Graduation.DTOs.Images;
using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.Reviews;
using Graduation.DTOs.ServiceToProject;
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
                    ServiceProject result = await dbContext.services.FindAsync(request.serviceId);
                    if(result.AdvertisementID is null)
                    {
                        AdvertisementProject advertisementProject = new AdvertisementProject
                        {
                            StartAt = request.StartAt,
                            EndAt = request.EndAt,

                        };
                        dbContext.advertisements.Add(advertisementProject);
                        await dbContext.SaveChangesAsync();

                        result.AdvertisementID = advertisementProject.Id;
                        dbContext.services.UpdateRange(result);
                        await dbContext.SaveChangesAsync();
                        return Ok(new { message = "add successful Advertisement" });
                    }
                    return BadRequest(new { message = "There is an advertisement" });
                    
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
                    PropertyProject result = await dbContext.properties.FindAsync(request.PropertyId);
                    if(result.AdvertisementID is null)
                    {

                    AdvertisementProject advertisementProject = new AdvertisementProject
                    {
                        StartAt = request.StartAt,
                        EndAt = request.EndAt,

                    };
                    dbContext.advertisements.Add(advertisementProject);
                    await dbContext.SaveChangesAsync();
                    result.AdvertisementID = advertisementProject.Id;
                    dbContext.properties.UpdateRange(result);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { message = "add successful Advertisement" });
                    }
                    return BadRequest(new {message= "There is an advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpPut("UpdateServiceAdvertisement")]
        public async Task<IActionResult> UpdateServiceAdvertisement(UpdateAdvertisementDTOs request,int AdvertisementId)
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
                if (role.Contains("provider") )
                {
                    AdvertisementProject result = await dbContext.advertisements.FindAsync(AdvertisementId);
                    
                    if(result is not null)
                    {
                     ServiceProject resultService=await dbContext.services.FirstOrDefaultAsync(x=>x.AdvertisementID==result.Id);
                        if(resultService is not null)
                        {
                           if(resultService.UsersID== requestUser.Id || role.Contains("admin"))
                            {
                                result.StartAt = request.StartAt;
                                result.EndAt = request.EndAt;
                                dbContext.advertisements.UpdateRange(result);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { message = "update successful" });
                            }
                           return Unauthorized();
                        }
                        return BadRequest(new { message = "not found service" });
                    }
                    return BadRequest(new { message = "not found Advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpPut("UpdatePropertyAdvertisement")]
        public async Task<IActionResult> UpdatePropertyAdvertisement(UpdateAdvertisementDTOs request, int AdvertisementId)
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
                if (role.Contains("provider") )
                {
                    AdvertisementProject result = await dbContext.advertisements.FindAsync(AdvertisementId);

                    if (result is not null)
                    {
                        PropertyProject resultService = await dbContext.properties.FirstOrDefaultAsync(x => x.AdvertisementID == result.Id);
                        if (resultService is not null)
                        {
                            if (resultService.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                                result.StartAt = request.StartAt;
                                result.EndAt = request.EndAt;
                                dbContext.advertisements.UpdateRange(result);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { message = "update successful" });
                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found property" });
                    }
                    return BadRequest(new { message = "not found Advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpDelete("DeleteServiceAdvertisement")]
        public async Task<IActionResult> DeleteServiceAdvertisement( int AdvertisementId)
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
                if (role.Contains("provider"))
                {
                    AdvertisementProject result = await dbContext.advertisements.FindAsync(AdvertisementId);

                    if (result is not null)
                    {
                        ServiceProject resultService = await dbContext.services.FirstOrDefaultAsync(x => x.AdvertisementID == result.Id);
                        if (resultService is not null)
                        {
                            if (resultService.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                                
                                dbContext.advertisements.RemoveRange(result);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { message = "remove successful" });
                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found service" });
                    }
                    return BadRequest(new { message = "not found Advertisement" });

                 
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpDelete("DeletePropertyAdvertisement")]
        public async Task<IActionResult> DeletePropertyAdvertisement( int AdvertisementId)
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
                if (role.Contains("provider") )
                {
                    AdvertisementProject result = await dbContext.advertisements.FindAsync(AdvertisementId);

                    if (result is not null)
                    {
                        PropertyProject resultService = await dbContext.properties.FirstOrDefaultAsync(x => x.AdvertisementID == result.Id);
                        if (resultService is not null)
                        {
                            if (resultService.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                               
                                dbContext.advertisements.RemoveRange(result);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { message = "delete successful" });
                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found property" });
                    }
                    return BadRequest(new { message = "not found Advertisement" });
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpGet("AllAdvertisement")]
        public async Task<IActionResult> AllAdvertisement()
        {

            var deleteEnd=await dbContext.advertisements.Where(adv=>adv.EndAt<DateTime.Now)
                .Include(adv=>adv.Services)
                .Include(adv=>adv.Properties)
                .ToListAsync();
            foreach (var adv in deleteEnd)
            {

                dbContext.advertisements.RemoveRange(adv);
                await dbContext.SaveChangesAsync();
            }
            var result = await dbContext.advertisements
            .Include(a => a.Properties)
                .ThenInclude(p => p.Type)
            .Include(a => a.Properties)
                .ThenInclude(p => p.ImageDetails)
            .Include(a => a.Properties)
                .ThenInclude(p => p.Reviews)
            .Include(a => a.Services)
                .ThenInclude(s => s.Type)
            .Include(a => a.Services)
                .ThenInclude(s => s.ImageDetails)
            .Include(a => a.Services)
                .ThenInclude(s => s.Reviews)
            .Select(adv => new GetAllAdvertisementDTOs
            {
                Id = adv.Id,
                StartAt = adv.StartAt,
                EndAt = adv.EndAt,
                Properties = adv.Properties.Select(p => new GetAllPropertyDTOs
                {
                    Id = p.Id,
                    Description = p.Description,
                    StartAt = p.StartAt,
                    EndAt = p.EndAt,
                    UserID = p.UsersID,
                    TypeName = p.Type != null ? p.Type.Name : null, 
                    ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = Path.Combine(Directory.GetCurrentDirectory(), img.Image),
                    }).ToList(),
                    Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        date = r.CreateAt,
                        description = r.Description,
                        rating = r.Rating,
                    }).ToList(),
                }).ToList(),
                Services = adv.Services.Select(ser => new GetAllServiceDTOs
                {
                    Id = ser.Id,
                    Description = ser.Description,
                    PriceRange = ser.PriceRange,
                    TypeName = ser.Type != null ? ser.Type.Name : null, 
                    userId = ser.UsersID,
                    ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = Path.Combine(Directory.GetCurrentDirectory(), img.Image),
                    }).ToList(),
                    Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        date = r.CreateAt,
                        description = r.Description,
                        rating = r.Rating,
                    }).ToList(),
                }).ToList()
            }).ToListAsync(); 
            return Ok(result);

        }





    }
}
