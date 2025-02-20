using Graduation.Data;
using Graduation.DTOs.Images;
using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.Reviews;
using Graduation.DTOs.Saves;
using Graduation.DTOs.ServiceToProject;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Graduation.Controllers.Save
{
    [Route("[controller]")]
    [ApiController]
    public class SaveToProjectController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public SaveToProjectController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpPost("SaveService")]
        public async Task<IActionResult> SaveService(int serviceId)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("consumer") || role.Contains("provider"))
                {
                    SaveProject save = new SaveProject
                    {
                        UserId = requestUser.Id,
                        ServiceId = serviceId,
                    };
                    dbContext.saveProjects.Add(save);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "save successfully" });
                }
                return Unauthorized();

            }
            return NotFound();

        }






        [HttpDelete("UnSave")]
        public async Task<IActionResult> UnSave(int saveId)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                
                    SaveProject result=await dbContext.saveProjects.FindAsync(saveId);
                    if (result.UserId== requestUser.Id||role.Contains("admin"))
                    {
                        dbContext.saveProjects.RemoveRange(result);
                        await dbContext.SaveChangesAsync();
                        return Ok(new { status = 200, message = "delete successfully" });
                    }
                    return Unauthorized() ;
              

            }
            return NotFound();

        }




        [HttpPost("SaveProperty")]
        public async Task<IActionResult> SaveProperty(int propertyId)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("consumer") || role.Contains("provider"))
                {
                    SaveProject save = new SaveProject
                    {
                        UserId = requestUser.Id,
                        PropertyId = propertyId,
                    };
                    dbContext.saveProjects.Add(save);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "save successfully" });
                }
                return Unauthorized();

            }
            return NotFound();

        }






        //[HttpDelete("UnSaveProperty")]
        //public async Task<IActionResult> UnSaveProperty(int saveId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
        //        if (string.IsNullOrEmpty(token))
        //            return Unauthorized(new { message = "Token Is Missing" });
        //        int? userId = ExtractClaims.ExtractUserId(token);
        //        if (string.IsNullOrEmpty(userId.ToString()))
        //            return Unauthorized(new { message = "Token Is Missing" });
        //        ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
        //        var role = await userManager.GetRolesAsync(requestUser);
        //        if (role.Contains("admin"))
        //        {
        //            SaveProject result = await dbContext.saveProjects.FindAsync(saveId);
        //            if (result.UserId == requestUser.Id)
        //            {
        //                dbContext.saveProjects.RemoveRange(result);
        //                await dbContext.SaveChangesAsync();
        //                return Ok(new { status = 200, message = "delete successfully" });
        //            }
        //            return Unauthorized();

        //        }
        //        return Unauthorized();

        //    }
        //    return NotFound();

        //}


        [HttpGet("GetSave")]
        public async Task<IActionResult> GetSave()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
                var result=await dbContext.saveProjects.Where(s=>s.UserId== userId).ToListAsync();
            List<GetSavesDTOs> ListSave = new List<GetSavesDTOs>();
            foreach(var project in result)
            {
                GetSavesDTOs savesDTOs = new GetSavesDTOs
                {
                    Id = project.Id,
                    UserId = project.UserId,
                    allProperty = await dbContext.properties.Where(p => p.Id == project.PropertyId)
                        .Select(p => new GetAllPropertyDTOs
                          {
                              Id = p.Id,
                              Description = p.Description,
                              StartAt = p.StartAt,
                              EndAt = p.EndAt,
                              UserID = p.UsersID,
                              ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                              {
                                  Id = img.Id,
                                  Name = Path.Combine(Directory.GetCurrentDirectory(), img.Image)
                              }).ToList(),
                              Reviews= p.Reviews.Select(r => new GetAllReviewDTOs
                              {
                                  Id = r.Id,
                                  description = r.Description,
                                  date = r.CreateAt,
                                  rating = r.Rating,

                              }).ToList()

                          }).ToListAsync(),
                    allService=await dbContext.services.Where(s=>s.Id==project.ServiceId)
                                    .Select(s=>new GetAllServiceDTOs
                                    {
                                        Id = s.Id,
                                        userId = s.UsersID,
                                        Description = s.Description,
                                        PriceRange = s.PriceRange,
                                        TypeName = s.Type.Name,
                                        ImageDetails = s.ImageDetails.Select(img => new GetImageDTOs
                                        {
                                            Id = img.Id,
                                            Name = Path.Combine(Directory.GetCurrentDirectory(), img.Image)
                                        }).ToList(),
                                        Reviews = s.Reviews.Select(r => new GetAllReviewDTOs
                                        {
                                            Id = r.Id,
                                            description = r.Description,
                                            date = r.CreateAt,
                                            rating = r.Rating,

                                        }).ToList()

                                    }).ToListAsync()
                };
                ListSave.Add(savesDTOs);
            }
            return Ok(new { status = 200, ListSave });
        }


    }
}
