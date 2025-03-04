using Graduation.Data;
using Graduation.DTOs.Images;
using Graduation.DTOs.Reviews;
using Graduation.DTOs.ServiceToProject;
using Graduation.Helpers;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Graduation.Controllers.ServiceToProject
{
    [Route("[controller]")]
    [ApiController]
    public class ServiceToProjectController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public ServiceToProjectController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService(AddServiceDTOs request)
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
                if (role.Contains("provider")||role.Contains("admin"))
                {

                    ServiceProject serviceProject = new ServiceProject
                    {
                        Description = request.Description,
                        PriceRange = request.PriceRange,
                        UsersID = requestUser.Id,
                        TypeId = request.TypeId,
                        AddressId=request.AddressId,
                    };
                    await dbContext.services.AddAsync(serviceProject);
                    await dbContext.SaveChangesAsync();

                    ReturnServiceDTOs getServiceDTOs = new ReturnServiceDTOs
                    {
                        Id = serviceProject.Id,
                        Description = serviceProject.Description,
                        PriceRange = serviceProject.PriceRange,
                        UsersID = serviceProject.UsersID,
                        TypeId = serviceProject.TypeId ,
                        AddressId = serviceProject.AddressId,

                    };
                    return Ok(new { status = 200, getServiceDTOs });
                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpPut("UpdateService")]
        public async Task<IActionResult> UpdateService(int serviceId, UpdateServiceDTOs request)
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
                    ServiceProject result=await dbContext.services.AsSplitQuery().Include(A=>A.Address).FirstOrDefaultAsync(s=>s.Id== serviceId);
                    if(result is not null)
                    {
                        if (result.UsersID== requestUser.Id|| role.Contains("admin"))
                        {

                        if (result is not null)
                        {
                            result.Description = request.Description;
                            result.PriceRange = request.PriceRange;
                             dbContext.UpdateRange(result);
                            await dbContext.SaveChangesAsync();

                        ReturnServiceDTOs getServiceDTOs = new ReturnServiceDTOs
                        {
                            Id = result.Id,
                            Description = result.Description,
                            PriceRange = result.PriceRange,
                            UsersID = result.UsersID,
                            TypeId = result.TypeId  ,
                            AddressId = result.AddressId,
                        };
                        return Ok(new { status = 200, getServiceDTOs });
                        };
                        return BadRequest(new { message = "don't find service" });
                        }
                        return Unauthorized();

                    }
                    return BadRequest(new {message= "not found service" });
                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpDelete("DeleteService")]
        public async Task<IActionResult> DeleteService(int serviceId)
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
               
                    ServiceProject result=await dbContext.services
                    .Include(s=>s.ImageDetails)
                    .Include(s=>s.Reviews)
                    .FirstOrDefaultAsync(u => u.Id == serviceId);
                    if(result is not null)
                    {
                       if(result.UsersID == requestUser.Id||role.Contains("admin"))
                        {
                        if (result.ImageDetails.Any()) 
                            foreach (var image in result.ImageDetails)
                            {
                                await FileSettings.DeleteFileAsync(image.Image); 
                                dbContext.images.Remove(image);
                            }
                        if (result.Reviews.Any())
                            foreach (var review in result.Reviews)
                            {
                                dbContext.reviews.Remove(review); 
                            }

                        dbContext.services.RemoveRange(result);
                            await dbContext.SaveChangesAsync();
                            
                            return Ok();
                        }
                       return Unauthorized() ;
                    }
                    return BadRequest(new {message= "not found service" });
                
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpPost("AddImageService")]
        public async Task<IActionResult> AddImageService(AddImagesDTOs request, int serviceId)
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
                    ImageDetails details = new ImageDetails
                    {
                        ServiceId = serviceId,
                        Image = await FileSettings.UploadFileAsync(request.Image),
                    };
                    await dbContext.images.AddAsync(details);
                    await dbContext.SaveChangesAsync();
                    return Ok("done");
                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpPut("UpdateImageService")]
        public async Task<IActionResult> UpdateImageService(AddImagesDTOs request, int imageId)
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
                    ImageDetails resultImage = await dbContext.images.FindAsync(imageId);
                    if (resultImage is not null)
                    {
                        ServiceProject resultService = await dbContext.services.FindAsync(resultImage.ServiceId);
                        if (resultService is not null)
                        {
                            if (resultService.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                                await FileSettings.DeleteFileAsync(resultImage.Image);
                                resultImage.Image =await FileSettings.UploadFileAsync(request.Image);
                                dbContext.UpdateRange(resultImage);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { status = 200 });

                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found service" });

                    }
                    return BadRequest(new { message = "not found image" });



                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpDelete("DeleteImageService")]
        public async Task<IActionResult> DeleteImageService( int imageId)
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
                    ImageDetails resultImage = await dbContext.images.FindAsync(imageId);
                    if (resultImage is not null)
                    {
                        ServiceProject resultService = await dbContext.services.FindAsync(resultImage.ServiceId);
                        if (resultService is not null)
                        {
                            if (resultService.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                                await FileSettings.DeleteFileAsync(resultImage.Image);;
                                dbContext.RemoveRange(resultImage);
                                await dbContext.SaveChangesAsync();
                                return Created();

                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found service" });

                    }
                    return BadRequest(new { message = "not found image" });



                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpPost("AddReviewService")]
        public async Task<IActionResult> AddReviewService(AddReviewServiceDTOs request)
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
                
                    Review review = new Review
                    {
                        Description = request.Description,
                        Rating = request.Rating,
                        CreateAt = DateTime.Now,
                        UsersID = requestUser.Id,
                        ServiceId = request.ServiceId
                    };
                    await dbContext.reviews.AddAsync(review);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { message = true });
            }
            return NotFound();
        }
        [HttpPut("UpdateReviewService")]
        public async Task<IActionResult> UpdateReviewService(UpdateReviewDTOs request,int reviewId)
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
               
                    Review result=await dbContext.reviews.FindAsync(reviewId);
                    if (result is not null)
                    {
                        if(result.UsersID == requestUser.Id||role.Contains("admin"))
                        {
                           result.Description = request.Description;
                            result.Rating = request.Rating;
                            result.ServiceId=result.ServiceId;
                            dbContext.UpdateRange(result);
                            await dbContext.SaveChangesAsync();
                            return Ok(new {message="update successful"});
                        }
                        return Unauthorized();
                    }
                    return BadRequest(new { message = "not found service" });
            }
            return NotFound();
        }

        [HttpDelete("DeleteReviewService")]
        public async Task<IActionResult> DeleteReviewService( int reviewId)
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
               
                    Review result = await dbContext.reviews.FindAsync(reviewId);
                    if (result is not null)
                    {
                        if (result.UsersID == requestUser.Id || role.Contains("admin"))
                        {
                            
                            dbContext.RemoveRange(result);
                            await dbContext.SaveChangesAsync();
                            return Ok(new { message = "remove successful" });
                        }
                        return Unauthorized();
                    }
                    return BadRequest(new { message = "not found service" });
            }
            return NotFound();
        }
        [HttpGet("AllService")]
        public async Task<IActionResult> AllService(string? type, string? address)
        {
            var  query = dbContext.services.AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.Type.Name.Contains(type));
            }
            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(p => p.Address.Name.Contains(address));
            }
            var allServices = await query
                .AsSplitQuery()
                .Include(s => s.ImageDetails)
                .Include(s => s.Reviews)
                .Include(s => s.Address)
                .Include(s => s.Type)
                .Select(s => new GetAllServiceDTOs
                {
                    Id = s.Id,
                    userId = s.UsersID,
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
                        UserId = r.UsersID,
                    }).ToList(),
                    AvgRating=s.Reviews.Any()?s.Reviews.Average(r=>r.Rating):0
                })
                .ToListAsync();
            return Ok(new { message = true, AllService = allServices });
        }
        [HttpGet("Service")]
        public async Task<IActionResult> Service(int ServiceId)
        {
            var service = await dbContext.services
                .Include(s => s.ImageDetails)
                .Include(s => s.Reviews)
                .Include(s => s.Address)
                .Include (s => s.Type)
                .FirstOrDefaultAsync(s => s.Id == ServiceId);

            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }
            var serviceDto = new GetAllServiceDTOs
            {
                Id = service.Id,
                userId = service.UsersID,
                Description = service.Description,
                PriceRange = service.PriceRange,
                TypeName = service.Type?.Name ?? "Unknown", 
                AddressName = service.Address?.Name ?? "Unknown", 
                ImageDetails = service.ImageDetails?
                    .Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    })
                    .ToList() ?? new List<GetImageDTOs>(), 
                Reviews = service.Reviews?
                    .Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        description = r.Description,
                        date = r.CreateAt,
                        rating = r.Rating,
                        UserId = r.UsersID,
                    })
                    .ToList() ?? new List<GetAllReviewDTOs>()     ,
                AvgRating = service.Reviews.Any() ? service.Reviews.Average(r => r.Rating) : 0
            };

            return Ok(new { message = true, Service = serviceDto });
        }

    }
}
