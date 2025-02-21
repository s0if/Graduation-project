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
                if (role.Contains("provider"))
                {
                    PropertyProject project = new PropertyProject
                    {
                      Description= request.Description,
                      StartAt= request.StartAt,
                      EndAt= request.EndAt,
                      TypeId= request.TypeId,
                      UsersID= requestUser.Id,
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
                      userId= project.UsersID
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
                    PropertyProject result=await dbContext.properties.FindAsync(propertyId);
                    if(result is not null)
                    {
                     if(result.UsersID== requestUser.Id || role.Contains("admin"))
                        {
                            result.Description = request.Description;
                            result.StartAt=request.StartAt;
                            result.EndAt=request.EndAt;
                            result.TypeId = result.TypeId;
                            dbContext.UpdateRange(result);
                            await dbContext.SaveChangesAsync();

                            ReturnPropertyDTOs returnProperty = new ReturnPropertyDTOs
                            {
                                Id = result.Id,
                                Description = result.Description,
                                StartAt = result.StartAt,
                                EndAt = result.EndAt,
                                TypeId = result.TypeId,
                                userId = result.UsersID
                            };
                            return Ok(new { status = 200, returnProperty });

                        }
                        return Unauthorized() ;
                    }
                    return BadRequest(new { message = "not found property" });
                   
                    
                }
                return Unauthorized();
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
                if (role.Contains("provider"))
                {
                    PropertyProject result = await dbContext.properties.FindAsync(propertyId);
                    if (result is not null)
                    {
                        if (result.UsersID == requestUser.Id || role.Contains("admin"))
                        {
                            
                            dbContext.RemoveRange(result);
                            await dbContext.SaveChangesAsync();

                          
                            return Ok(new { status = 200 });

                        }
                        return Unauthorized();
                    }
                    return BadRequest(new { message = "not found property" });


                }
                return Unauthorized();
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
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider"))
                {
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
                return Unauthorized();
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
                if (role.Contains("provider"))
                {
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
                return Unauthorized();
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
                if (role.Contains("provider"))
                {
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
                return Unauthorized();
            }
            return NotFound();
        }
        [HttpGet("AllProperty")]
        public async Task<IActionResult> AllProperty(string? searchName)
        {
            var query = dbContext.properties.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(p => p.Type.Name.Contains(searchName));
            }

            var allProperty = await query
                .AsSplitQuery()
                .Include(p => p.ImageDetails)
                .Include(p => p.Reviews)
                .Select(s => new GetAllPropertyDTOs
                {
                    Id = s.Id,
                    UserID = s.UsersID,
                    Description = s.Description,
                    TypeName = s.Type.Name,
                    StartAt = s.StartAt,
                    EndAt = s.EndAt,
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
                })
                .ToListAsync();

            return Ok(new { message = true, AllProperty = allProperty });
        }



    }
}
