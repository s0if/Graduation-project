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
               
                    SaveProject save = new SaveProject
                    {
                        UserId = requestUser.Id,
                        ServiceId = serviceId,
                    };
                    dbContext.saveProjects.Add(save);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "save successfully", saveId = save.Id });

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
               
                    SaveProject save = new SaveProject
                    {
                        UserId = requestUser.Id,
                        PropertyId = propertyId,
                    };
                    dbContext.saveProjects.Add(save);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "save successfully" });
                

            }
            return NotFound();

        }
        [HttpDelete("UnSaveService")]
        public async Task<IActionResult> UnSaveService(int Id)
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
                var result = await dbContext.saveProjects.Where(s => s.UserId == userId &&  s.Services.Id == Id).ToListAsync(); 
                if (result.Any())
                {

                    dbContext.saveProjects.RemoveRange(result);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "delete successfully" });
                }
                return BadRequest(new { status = 400, message = "not found property" });


            }
            return NotFound();

        }
        [HttpDelete("UnSaveProperty")]
        public async Task<IActionResult> UnSaveProperty(int Id)
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
                var result = await dbContext.saveProjects.Where(s => s.UserId == userId && s.Properties.Id == Id).ToListAsync(); 
                if (result.Any())
                {

                dbContext.saveProjects.RemoveRange(result);
                await dbContext.SaveChangesAsync();
                return Ok(new { status = 200, message = "delete successfully" });
                }
                return BadRequest(new { status = 400, message = "not found property" });





            }
            return NotFound();

        }

        [HttpGet("GetSave")]
        public async Task<IActionResult> GetSave()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            var result = await dbContext.saveProjects.Where(s => s.UserId == userId).ToListAsync();
            List<GetSavesDTOs> ListSave = new List<GetSavesDTOs>();
            foreach (var project in result)
            {
                GetSavesDTOs savesDTOs = new GetSavesDTOs
                {
                    Id = project.Id,
                    UserId = project.UserId,
                    allProperty = await dbContext.properties.Where(p => p.Id == project.PropertyId)
                    .Include(p => p.User)
                    .Include(p=>p.Type)
                    .Include(p=>p.Address)
                        .Select(p => new GetAllPropertyDTOs
                        {
                            Id = p.Id,
                            Description = p.Description,
                            StartAt = p.StartAt,
                            EndAt = p.EndAt,
                            UserID = p.UsersID,
                            userName = p.User.UserName,
                            TypeName=p.Type.Name,
                            AddressName=p.Address.Name,
                            ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                description = r.Description,
                                date = r.CreateAt,
                                rating = r.Rating,

                            }).ToList()

                        }).ToListAsync(),
                    allService = await dbContext.services.Where(s => s.Id == project.ServiceId)
                                    .Include(p => p.User)
                                    .Include (p => p.Type)
                                    .Include(p=>p.Address)
                                    .Select(s => new GetAllServiceDTOs
                                    {
                                        Id = s.Id,
                                        userId = s.UsersID,
                                        UserName = s.User.UserName,
                                        Description = s.Description,
                                        PriceRange = s.PriceRange,
                                        TypeName = s.Type.Name,
                                        AddressName = s.Address.Name,
                                        ImageDetails = s.ImageDetails.Select(img => new GetImageDTOs
                                        {
                                            Id = img.Id,
                                            Name = img.Image
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



        [HttpGet("StatusService")]
        public async Task<IActionResult> StatusService(int Id)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            var result = await dbContext.saveProjects.Where(s => s.UserId == userId).ToListAsync();
            List<GetSavesDTOs> ListSave = new List<GetSavesDTOs>();
            bool status = false;
            
            foreach (var project in result)
            {
                GetSavesDTOs savesDTOs = new GetSavesDTOs
                {
                    Id = project.Id,
                    UserId = project.UserId,
                    allProperty = await dbContext.properties.Where(p => p.Id == project.PropertyId)
                                .Include(p => p.User)
                                    .Select(p => new GetAllPropertyDTOs
                                    {

                                        Id = p.Id,
                                        Description = p.Description,
                                        StartAt = p.StartAt,
                                        EndAt = p.EndAt,
                                        //UserID = p.UsersID,
                                        userName = p.User.UserName,
                                        ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                                        {
                                            Id = img.Id,
                                            Name = img.Image
                                        }).ToList(),
                                        Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                                        {
                                            Id = r.Id,
                                            description = r.Description,
                                            date = r.CreateAt,
                                            rating = r.Rating,

                                        }).ToList()

                                    }).ToListAsync(),
                    allService = await dbContext.services.Where(s => s.Id == project.ServiceId)
                                                .Include(p => p.User)
                                                .Select(s => new GetAllServiceDTOs
                                                {
                                                    Id = s.Id,
                                                    //userId = s.UsersID,
                                                    UserName = s.User.UserName,
                                                    Description = s.Description,
                                                    PriceRange = s.PriceRange,
                                                    TypeName = s.Type.Name,
                                                    ImageDetails = s.ImageDetails.Select(img => new GetImageDTOs
                                                    {
                                                        Id = img.Id,
                                                        Name = img.Image
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

            status = ListSave.Any(s =>  s.allService.Any(p => p.Id == Id));
              return Ok(status);
        }

        [HttpGet("StatusProperty")]
        public async Task<IActionResult> StatusProperty(int Id)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            var result = await dbContext.saveProjects.Where(s => s.UserId == userId).ToListAsync();
            List<GetSavesDTOs> ListSave = new List<GetSavesDTOs>();
            bool status = false;

            foreach (var project in result)
            {
                GetSavesDTOs savesDTOs = new GetSavesDTOs
                {
                    Id = project.Id,
                    UserId = project.UserId,
                    allProperty = await dbContext.properties.Where(p => p.Id == project.PropertyId)
                                .Include(p => p.User)
                                    .Select(p => new GetAllPropertyDTOs
                                    {

                                        Id = p.Id,
                                        Description = p.Description,
                                        StartAt = p.StartAt,
                                        EndAt = p.EndAt,
                                        //UserID = p.UsersID,
                                        userName = p.User.UserName,
                                        ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                                        {
                                            Id = img.Id,
                                            Name = img.Image
                                        }).ToList(),
                                        Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                                        {
                                            Id = r.Id,
                                            description = r.Description,
                                            date = r.CreateAt,
                                            rating = r.Rating,

                                        }).ToList()

                                    }).ToListAsync(),
                    allService = await dbContext.services.Where(s => s.Id == project.ServiceId)
                                                .Include(p => p.User)
                                                .Select(s => new GetAllServiceDTOs
                                                {
                                                    Id = s.Id,
                                                    //userId = s.UsersID,
                                                    UserName = s.User.UserName,
                                                    Description = s.Description,
                                                    PriceRange = s.PriceRange,
                                                    TypeName = s.Type.Name,
                                                    ImageDetails = s.ImageDetails.Select(img => new GetImageDTOs
                                                    {
                                                        Id = img.Id,
                                                        Name = img.Image
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

            status = ListSave.Any(s => s.allProperty.Any(p => p.Id == Id));
            return Ok(status);
        }


    }
}
