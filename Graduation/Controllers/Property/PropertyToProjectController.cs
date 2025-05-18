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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly ExtractClaims extractClaims;

        public PropertyToProjectController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ExtractClaims extractClaims)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.extractClaims = extractClaims;
        }

        [HttpPost("AddProperty")]
        public async Task<IActionResult> AddProperty(AddPropertyDTOs request)
        {
            
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string token = Request.Headers["Authorization"].ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token " });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);
                if (!roles.Contains("provider") && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only admin or provider can add this property" });

                var project = new PropertyProject
                {
                    Description = request.Description,
                    TypeId = request.TypeId,
                    UsersID = requestUser.Id,
                    AddressId = request.AddressId,
                    Price = request.Price,
                    lat = request.lat,
                    lng = request.lng,
                    StartAt = DateTime.Now,
                };
                await dbContext.properties.AddAsync(project);
                await dbContext.SaveChangesAsync();
            ReturnPropertyDTOs returnProperty = new ReturnPropertyDTOs
                {
                    Id = project.Id,
                    Description = project.Description,
                    TypeId = project.TypeId,
                    lat = request.lat,
                    lng = request.lng,
                    userId = project.UsersID,
                    addressId = project.AddressId,
                    Price = project.Price,
                    StartAt=project.StartAt,
                };

                return Ok(new { status = "success", data = returnProperty });
           
        }
        [HttpPut("UpdateProperty")]
        public async Task<IActionResult> UpdateProperty(UpdatePropertyDTOs request, int propertyId)
        {
            
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);

                var property = await dbContext.properties.FindAsync(propertyId);
                if (property == null)
                    return NotFound(new { message = "Property not found" });

                // السماح بالتحديث فقط للمالك أو الأدمن
                if (property.UsersID != requestUser.Id && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only admin or the provider can update this property" });

                // تحديث الحقول المطلوبة
                property.Description = request.Description;
                property.Price = request.Price;
                property.updateAt = DateTime.UtcNow;

                dbContext.properties.Update(property);
                await dbContext.SaveChangesAsync();

                var returnProperty = new ReturnPropertyDTOs
                {
                    Id = property.Id,
                    Description = property.Description,
                    updateAt = property.updateAt,
                    Price = property.Price,
                    TypeId = property.TypeId,
                    userId = property.UsersID,
                    addressId = property.AddressId,
                };

                return Ok(new { status = "success", data = returnProperty });
            
            
        }

        [HttpDelete("DeleteProperty")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
           
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);

                var property = await dbContext.properties
                    .Include(p => p.ImageDetails)
                    .Include(p => p.Reviews)
                    .Include(p => p.Saves)
                    .FirstOrDefaultAsync(p => p.Id == propertyId);

                if (property == null)
                    return NotFound(new { message = "Property not found" });

                // السماح بالحذف فقط للمالك أو الأدمن
                if (property.UsersID != requestUser.Id && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only admin or the provider can delete this property" });

                // حذف الصور المرتبطة
                if (property.ImageDetails.Any())
                {
                    foreach (var image in property.ImageDetails)
                    {
                        await FileSettings.DeleteFileAsync(image.Image);
                        dbContext.images.Remove(image);
                    }
                }

                // حذف التقييمات المرتبطة
                if (property.Reviews.Any())
                {
                    foreach (var review in property.Reviews)
                    {
                        dbContext.reviews.Remove(review);
                    }
                }

                // حذف السيفات المرتبطة
                if (property.Saves.Any())
                {
                    dbContext.saveProjects.RemoveRange(property.Saves);
                }

                // التعامل مع الإعلان المرتبط
                if (property.AdvertisementID != null)
                {
                    var advertisement = await dbContext.advertisements.FindAsync(property.AdvertisementID);
                    if (advertisement != null)
                    {
                        property.AdvertisementID = null;
                        dbContext.Update(property);
                        await dbContext.SaveChangesAsync();
                        dbContext.Remove(advertisement);
                    }
                }

                // حذف العقار نفسه
                dbContext.Remove(property);
                await dbContext.SaveChangesAsync();

                return Ok(new { status = "success", message = "Property deleted successfully" });
            
        }

        [HttpPost("AddImageProperty")]
        public async Task<IActionResult> AddImageProperty(AddImagesDTOs request, int propertyId)
        {
           
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);
                if (!roles.Contains("provider") && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only admin or the provider can add image property" });

                // يمكن إضافة تحقق من وجود الـ propertyId إذا أردت

                ImageDetails details = new ImageDetails
                {
                    PropertyId = propertyId,
                    Image = await FileSettings.UploadFileAsync(request.Image),
                };
                await dbContext.images.AddAsync(details);
                await dbContext.SaveChangesAsync();

                return Ok(new { status = "success", message = "Image added successfully" });
            
        }

        [HttpPut("UpdateImageProperty")]
        public async Task<IActionResult> UpdateImageProperty(AddImagesDTOs request, int imageId)
        {
          
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);

                var resultImage = await dbContext.images.FindAsync(imageId);
                if (resultImage == null)
                    return NotFound(new { message = "Image not found" });

                var resultProperty = await dbContext.properties.FindAsync(resultImage.PropertyId);
                if (resultProperty == null)
                    return NotFound(new { message = "Property not found" });

                if (resultProperty.UsersID != requestUser.Id && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only admin or the provider can update image property" });

                await FileSettings.DeleteFileAsync(resultImage.Image);
                resultImage.Image = await FileSettings.UploadFileAsync(request.Image);
                dbContext.Update(resultImage);
                await dbContext.SaveChangesAsync();

                return Ok(new { status = "success", message = "Image updated successfully" });
            
        }


        [HttpDelete("DeleteImageProperty")]
        public async Task<IActionResult> DeleteImageProperty(int imageId)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header" });

                token = token.Substring("Bearer ".Length).Trim();
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);

                ImageDetails resultImage = await dbContext.images.FindAsync(imageId);
                if (resultImage is not null)
                {
                    PropertyProject resultProperty = await dbContext.properties.FindAsync(resultImage.PropertyId);
                    if (resultProperty is not null)
                    {
                        if (resultProperty.UsersID == requestUser.Id || role.Contains("admin"))
                        {
                            await FileSettings.DeleteFileAsync(resultImage.Image);
                            dbContext.RemoveRange(resultImage);
                            await dbContext.SaveChangesAsync();
                            return Created();

                        }
                        return Unauthorized(new { message = "Only admin or the provider can delete image property" });
                    }
                    return BadRequest(new { message = "not found service" });

                }
                return BadRequest(new { message = "not found image" });
            }
            return NotFound();
        }
        [HttpPost("AddReviewProperty")]
        public async Task<IActionResult> AddReviewProperty(AddReviewPropertyDTOs request)
        {
           
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);
                if (roles.Contains("admin"))
                    return Unauthorized(new { message = "Admins cannot add reviews" });

                var property = await dbContext.properties
                    .Include(p => p.Type)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == request.PropertyId);

                if (property == null)
                    return NotFound(new { message = "Property not found" });

                var existingReviews = await dbContext.reviews
                    .Where(r => r.UsersID == userId && r.PropertyId == request.PropertyId)
                    .ToListAsync();

                if (existingReviews.Any())
                    return BadRequest(new { message = "You have already submitted a review for this property." });

                var review = new Review
                {
                    Description = request.Description,
                    Rating = request.Rating,
                    CreateAt = DateTime.UtcNow,
                    UsersID = requestUser.Id,
                    PropertyId = request.PropertyId
                };

                await dbContext.reviews.AddAsync(review);
                await dbContext.SaveChangesAsync();

                string whatsappMessage = $"📌 إشعار بتعليق جديد\n\n" +
                                         $"تمت إضافة تعليق جديد على عقارك:\n" +
                                         $"🏠 العقار: {property.Type.Name}\n" +
                                         $"📝 التعليق: {review.Description}\n\n" +
                                         $"🕒 التاريخ: {DateTime.UtcNow:yyyy-MM-dd HH:mm}\n\n" +
                                         $"شكراً لاستخدامك منصتنا!";

                await WhatsAppService.SendMessageAsync(property.User.PhoneNumber, whatsappMessage);

                return Ok(new { message = true });
            
        }

        [HttpPut("UpdateReviewProperty")]
        public async Task<IActionResult> UpdateReviewProperty(UpdateReviewDTOs request, int reviewId)
        {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);

                var review = await dbContext.reviews.FindAsync(reviewId);
                if (review == null)
                    return NotFound(new { message = "Review not found" });

                if (review.UsersID != requestUser.Id && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only the review owner or admin can update this review" });

                review.Description = request.Description;
                review.Rating = request.Rating;

                dbContext.Update(review);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Update successful" });
            
        }

        [HttpDelete("DeleteReviewProperty")]
        public async Task<IActionResult> DeleteReviewProperty(int reviewId)
        {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Unauthorized(new { message = "Authorization header missing" });

                string token = authHeader.ToString();
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Invalid Authorization header format" });

                token = token.Substring("Bearer ".Length).Trim();

                int? userId = await extractClaims.ExtractUserId(token);
                if (!userId.HasValue)
                    return Unauthorized(new { message = "Invalid token or user not found" });

                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
                if (requestUser == null)
                    return Unauthorized(new { message = "User not found" });

                var roles = await userManager.GetRolesAsync(requestUser);

                var review = await dbContext.reviews.FindAsync(reviewId);
                if (review == null)
                    return NotFound(new { message = "Review not found" });

                if (review.UsersID != requestUser.Id && !roles.Contains("admin"))
                    return Unauthorized(new { message = "Only the review owner or admin can delete this review" });

                dbContext.Remove(review);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Remove successful" });
            
        }

        [HttpGet("AllProperty")]
        public async Task<IActionResult> AllProperty(string? type, string? address)
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
                .Include(p => p.Address)
                .Include(p => p.Type)
                .Include(p => p.User)
                .Select(s => new GetAllPropertyDTOs
                {
                    Id = s.Id,
                    UserID = s.UsersID,
                    Description = s.Description,
                    TypeName = s.Type.Name,
                    StartAt = s.StartAt,
                    updateAt = s.updateAt,
                    Price = s.Price,
                    AddressName = s.Address.Name,
                    userName = s.User.UserName,
                    lat=s.lat,
                    lng=s.lng,
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

                    AvgRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0

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
             .Include(p => p.Type)
             .Include(p => p.User)
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
                updateAt = Property.updateAt,
                lat= Property.lat,
                lng= Property.lng,  
                AddressName = Property.Address?.Name ?? "Unknown",
                userName = Property.User.UserName,
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
                    .ToList() ?? new List<GetAllReviewDTOs>(),

                AvgRating = Property.Reviews.Any() ? Property.Reviews.Average(r => r.Rating) : 0

            };

            return Ok(new { message = true, AllProperty = getProperty });
        }
    }
}
