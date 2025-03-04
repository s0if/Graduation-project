using Azure.Core;
using Graduation.Data;
using Graduation.DTOs.Images;
using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.Reviews;
using Graduation.DTOs.ServiceToProject;
using Graduation.Helpers;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;

namespace Graduation.Controllers.PropertyToProject
{
    [Route("[controller]")]
    [ApiController]
    public class PropertyToProjectController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public PropertyToProjectController(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpPost("AddProperty")]
        public async Task<IActionResult> AddProperty(AddPropertyDTOs request)
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
                    PropertyProject project = new PropertyProject
                    {
                      Description= request.Description,
                      StartAt= request.StartAt,
                      EndAt= request.EndAt,
                      TypeId= request.TypeId,
                      UsersID= requestUser.Id,
                      AddressId= request.AddressId,
                      Price= request.Price,
                    };
                    await dbContext.properties.AddAsync(project);
                    await dbContext.SaveChangesAsync();
                    ReturnPropertyDTOs returnProperty = new ReturnPropertyDTOs
                    {
                      Id= project.Id,
                      Description= project.Description,
                      StartAt= project.StartAt,
                      EndAt= project.EndAt,
                      TypeId= project.TypeId,
                      userId= project.UsersID ,
                      addressId=project.AddressId,
                      Price= project.Price,
                    };
                    return Ok(new { status = 200, returnProperty });
                }
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpPut("UpdateProperty")]
        public async Task<IActionResult> UpdateProperty(UpdatePropertyDTOs request,int propertyId)
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
                    PropertyProject result = await dbContext.properties.FindAsync(propertyId);
                    
                     if(result.UsersID== requestUser.Id || role.Contains("admin"))
                        {
                            result.Description = request.Description;
                            result.StartAt=request.StartAt;
                            result.EndAt=request.EndAt;
                            result.Price = request.Price;
                            dbContext.UpdateRange(result);
                            await dbContext.SaveChangesAsync();

                            ReturnPropertyDTOs returnProperty = new ReturnPropertyDTOs
                            {
                                Id = result.Id,
                                Description = result.Description,
                                StartAt = result.StartAt,
                                EndAt = result.EndAt,
                                Price = result.Price,
                                TypeId = result.TypeId,
                                userId = result.UsersID ,
                                addressId = result.AddressId ,
                            };
                            return Ok(new { status = 200, returnProperty });

                        }
                        return Unauthorized() ;
                    }
                    return BadRequest(new { message = "not found property" });
                
            }
            return NotFound();

        }
        [HttpDelete("DeleteProperty")]
        public async Task<IActionResult> DeleteProperty( int propertyId)
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
                    PropertyProject result = await dbContext.properties
                    .Include(p=>p.ImageDetails)
                    .Include(p=>p.Reviews)
                    .FirstOrDefaultAsync(p=>p.Id==propertyId);
                    if (result is not null)
                    {
                        if (result.UsersID == requestUser.Id || role.Contains("admin"))
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

                        dbContext.RemoveRange(result);
                            await dbContext.SaveChangesAsync();

                          
                            return Ok(new { status = 200 });

                        }
                        return Unauthorized();
                    }
                    return BadRequest(new { message = "not found property" });


            }
            return NotFound();

        }
        [HttpPost("AddImageProperty")]
        public async Task<IActionResult> AddImageProperty(AddImagesDTOs request, int propertyId)
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
                        PropertyId= propertyId,
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

        [HttpPut("UpdateImageProperty")]
        public async Task<IActionResult> UpdateImageProperty(AddImagesDTOs request, int imageId)
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
                        PropertyProject resultProperty = await dbContext.properties.FindAsync(resultImage.PropertyId);
                        if (resultProperty is not null)
                        {
                            if (resultProperty.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                                await FileSettings.DeleteFileAsync(resultImage.Image);
                                resultImage.Image = await FileSettings.UploadFileAsync(request.Image);
                                dbContext.UpdateRange(resultImage);
                                await dbContext.SaveChangesAsync();
                                return Ok(new { status = 200 });

                            }
                            return Unauthorized();
                        }
                        return BadRequest(new { message = "not found property" });

                    }
                    return BadRequest(new { message = "not found image" });



                }
                return Unauthorized();
            }
            return NotFound();
        }

        [HttpDelete("DeleteImageProperty")]
        public async Task<IActionResult> DeleteImageProperty(int imageId)
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
                        PropertyProject resultProperty= await dbContext.properties.FindAsync(resultImage.PropertyId);
                        if (resultProperty is not null)
                        {
                            if (resultProperty.UsersID == requestUser.Id || role.Contains("admin"))
                            {
                               await FileSettings.DeleteFileAsync(resultImage.Image); 
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
        [HttpPost("AddReviewProperty")]
        public async Task<IActionResult> AddReviewProperty(AddReviewPropertyDTOs request)
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
                        PropertyId = request.PropertyId
                    };
                    await dbContext.reviews.AddAsync(review);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { message = true });
            }
            return NotFound();
        }
        [HttpPut("UpdateReviewProperty")]
        public async Task<IActionResult> UpdateReviewProperty(UpdateReviewDTOs request, int reviewId)
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
                            result.Description = request.Description;
                            result.Rating = request.Rating;
                            dbContext.UpdateRange(result);
                            await dbContext.SaveChangesAsync();
                            return Ok(new { message = "update successful" });
                        }
                        return Unauthorized();
                    }
                    return BadRequest(new { message = "not found review" });
            }
            return NotFound();
        }
        [HttpDelete("DeleteReviewProperty")]
        public async Task<IActionResult> DeleteReviewProperty(int reviewId)
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
                    return BadRequest(new { message = "not found review" });
            }
            return NotFound();
        }
        [HttpGet("AllProperty")]
        public async Task<IActionResult> AllProperty(string? type , string? address)
        {
            var query = dbContext.properties.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type.Name.Contains(type));
            }
            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(p => p.Address.Name.Contains(address));
            }

            var allProperty = await query
                .AsSplitQuery()
                .Include(p => p.ImageDetails)
                .Include(p => p.Reviews)
                .Include (p => p.Address)
                .Include(p=>p.Type)
                .Select(s => new GetAllPropertyDTOs
                {
                    Id = s.Id,
                    UserID = s.UsersID,
                    Description = s.Description,
                    TypeName = s.Type.Name,
                    StartAt = s.StartAt,
                    EndAt = s.EndAt,
                    Price = s.Price,
                    AddressName = s.Address.Name,
                    ImageDetails = s.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    }).ToList(),
                    Reviews = s.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        UserId = r.UsersID,
                        description = r.Description,
                        date = r.CreateAt,
                        rating = r.Rating,
                    }).ToList(),
                       
                    AvgRating=s.Reviews.Any()?s.Reviews.Average(r=>r.Rating):0
                   
                }
                )
                .ToListAsync();

           
            return Ok(new { message = true, AllProperty = allProperty });
        }
        [HttpGet("property")]
        public async Task<IActionResult> property(int propertyId)
        {

            var Property = await dbContext.properties
             .AsSplitQuery() 
             .Include(p => p.ImageDetails)
             .Include(p => p.Reviews)
             .Include(p => p.Address)
             .Include(p=>p.Type)
             .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (Property == null)
            {
                return NotFound(new { message = "Property not found" });
            }  
            GetAllPropertyDTOs getProperty = new GetAllPropertyDTOs
            {
                Id = Property.Id,
                UserID = Property.UsersID,
                Description = Property.Description,
                Price = Property.Price,
                TypeName = Property.Type?.Name ?? "Unknown", 
                StartAt = Property.StartAt,
                EndAt = Property.EndAt,
                AddressName = Property.Address?.Name ?? "Unknown", 
                ImageDetails = Property.ImageDetails?
                    .Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    })
                    .ToList() ?? new List<GetImageDTOs>(), 
                Reviews = Property.Reviews?
                    .Select(r =>
                     new GetAllReviewDTOs
                        {
                            Id = r.Id,
                            UserId = r.UsersID,
                            description = r.Description,
                            date = r.CreateAt,
                            rating = r.Rating,

                        }
                    
                    )
                    .ToList() ?? new List<GetAllReviewDTOs>()  ,
                 
                AvgRating = Property.Reviews.Any()? Property.Reviews.Average(r => r.Rating):0
            
        };
           
            return Ok(new { message = true, AllProperty = getProperty });
        }



    }
}
