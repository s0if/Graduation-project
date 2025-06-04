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
        private readonly ExtractClaims extractClaims;

        public AdvertisementController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ExtractClaims extractClaims)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.extractClaims = extractClaims;
        }
        [HttpPost("AddServiceAdvertisement")]
        public async Task<IActionResult> AddServiceAdvertisement(AddServiceAdvertisementDTOs request)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider") || role.Contains("admin"))
                {
                    ServiceProject result = await dbContext.services.FindAsync(request.serviceId);
                    if (result is not null)
                    {
                        AdvertisementProject advertisementProject = new AdvertisementProject
                        {
                            StartAt = request.StartAt,
                            EndAt = request.EndAt,
                             serviceId = request.serviceId,

                        };
                        dbContext.advertisements.Add(advertisementProject);
                        await dbContext.SaveChangesAsync();

                        await dbContext.SaveChangesAsync();
                        return Ok(new { message = "add successful Advertisement" });
                    }
                    return BadRequest(new { message = "Service not found" });

                }
                return Unauthorized(new { message = "Only Admins or provider Can Delete Type Services" });
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
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider") || role.Contains("admin"))
                {
                    PropertyProject result = await dbContext.properties.FindAsync(request.PropertyId);
                    if (result is not null)
                    {

                        AdvertisementProject advertisementProject = new AdvertisementProject
                        {
                            StartAt = request.StartAt,
                            EndAt = request.EndAt,
                            propertyId = request.PropertyId,

                        };
                        dbContext.advertisements.Add(advertisementProject);
                        await dbContext.SaveChangesAsync();
                        return Ok(new { message = "add successful Advertisement" });
                    }
                    return BadRequest(new { message = "property not found" });
                }
                return Unauthorized(new { message = "Only Admins or provider Can Delete Type Services" });

            }
            return NotFound();
        }
        [HttpPut("UpdateServiceAdvertisement")]
        public async Task<IActionResult> UpdateServiceAdvertisement(UpdateAdvertisementDTOs request, int AdvertisementId)
        {
            if (ModelState.IsValid)
            {
                // التحقق من صحة التوكن والمستخدم
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });

                int? userId = await extractClaims.ExtractUserId(token);
                if (userId == null)
                    return Unauthorized(new { message = "Token Is Missing" });

                var requestUser = await userManager.FindByIdAsync(userId.ToString());
                var role = await userManager.GetRolesAsync(requestUser);

                // جلب الإعلان مع الخدمة المرتبطة به
                var advertisement = await dbContext.advertisements
                    .Include(a => a.service) // تضمين الخدمة المرتبطة
                    .FirstOrDefaultAsync(a => a.Id == AdvertisementId);

                if (advertisement == null)
                    return BadRequest(new { message = "Advertisement not found" });

                // التحقق من وجود الخدمة المرتبطة
                if (advertisement.service == null)
                    return BadRequest(new { message = "No service linked to this advertisement" });

                // التحقق من الصلاحيات
                if (advertisement.service.UsersID != requestUser.Id && !role.Contains("admin"))
                    return Unauthorized(new { message = "Only Admins or the service provider can update this advertisement" });

                // تحديث بيانات الإعلان
                advertisement.StartAt = request.StartAt;
                advertisement.EndAt = request.EndAt;

                // حفظ التغييرات
                dbContext.advertisements.Update(advertisement);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Advertisement updated successfully" });
            }
            return BadRequest(ModelState);
        }
        [HttpPut("UpdatePropertyAdvertisement")]
        public async Task<IActionResult> UpdatePropertyAdvertisement(UpdateAdvertisementDTOs request, int AdvertisementId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من التوكن والمستخدم
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (userId == null)
                return Unauthorized(new { message = "Token Is Missing or Invalid" });

            var requestUser = await userManager.FindByIdAsync(userId.ToString());
            var roles = await userManager.GetRolesAsync(requestUser);

            // جلب الإعلان مع العقار المرتبط
            var advertisement = await dbContext.advertisements
                .Include(a => a.property) // التضمين التلقائي للعقار
                .FirstOrDefaultAsync(a => a.Id == AdvertisementId);

            if (advertisement == null)
                return NotFound(new { message = "Advertisement not found" });

            // التحقق من وجود عقار مرتبط
            if (advertisement.property == null)
                return BadRequest(new { message = "No property linked to this advertisement" });

            // التحقق من الصلاحيات
            if (advertisement.property.UsersID != requestUser.Id && !roles.Contains("admin"))
                return Unauthorized(new { message = "Only Admins or the property owner can update this advertisement" });

            // تحديث بيانات الإعلان
            advertisement.StartAt = request.StartAt;
            advertisement.EndAt = request.EndAt;

            // حفظ التغييرات
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Advertisement updated successfully" });
        }
        [HttpDelete("DeleteServiceAdvertisement")]
        public async Task<IActionResult> DeleteServiceAdvertisement(int AdvertisementId)
        {
            // Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Authentication and authorization
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Authorization token is required" });

            var userId = await extractClaims.ExtractUserId(token);
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var requestUser = await userManager.FindByIdAsync(userId.ToString());
            if (requestUser == null)
                return Unauthorized(new { message = "User not found" });

            var roles = await userManager.GetRolesAsync(requestUser);

            // Get advertisement with related service in a single query
            var advertisement = await dbContext.advertisements
                .Include(a => a.service) // Include the related service
                .FirstOrDefaultAsync(a => a.Id == AdvertisementId);

            if (advertisement == null)
                return NotFound(new { message = "Advertisement not found" });

            // Check if service exists and user has permission
            if (advertisement.service == null)
                return BadRequest(new { message = "No service associated with this advertisement" });

            if (advertisement.service.UsersID != requestUser.Id && !roles.Contains("admin"))
                return Unauthorized(new { message = "Only admins or the service owner can delete this advertisement" });

            // Perform deletion
            
                // If you want to keep the service but just remove the advertisement relationship:
                // advertisement.service.AdvertisementID = null;
                // dbContext.services.Update(advertisement.service);

                dbContext.advertisements.Remove(advertisement);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = "Advertisement deleted successfully" });
          
            
        }
        [HttpDelete("DeletePropertyAdvertisement")]
        public async Task<IActionResult> DeletePropertyAdvertisement(int AdvertisementId)
        {
            // التحقق من صحة النموذج
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data" });

            // التحقق من صحة التوكن
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Authorization token is required" });

            // استخراج هوية المستخدم
            var userId = await extractClaims.ExtractUserId(token);
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            // جلب بيانات المستخدم
            var requestUser = await userManager.FindByIdAsync(userId.ToString());
            if (requestUser == null)
                return Unauthorized(new { message = "User not found" });

            // جلب صلاحيات المستخدم
            var roles = await userManager.GetRolesAsync(requestUser);

            // جلب الإعلان مع العقار المرتبط في استعلام واحد
            var advertisement = await dbContext.advertisements
                .Include(a => a.property)
                .FirstOrDefaultAsync(a => a.Id == AdvertisementId);

            if (advertisement == null)
                return NotFound(new { message = "Advertisement not found" });

            // التحقق من وجود عقار مرتبط
            if (advertisement.property == null)
                return BadRequest(new { message = "No property associated with this advertisement" });

            // التحقق من صلاحيات المستخدم
            if (advertisement.property.UsersID != requestUser.Id && !roles.Contains("admin"))
                return Unauthorized(new { message = "Only admins or property owner can delete this advertisement" });

           

            // حذف الإعلان
            dbContext.advertisements.Remove(advertisement);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Advertisement deleted successfully" });
        }
        [HttpGet("AllAdvertisement")]
        public async Task<IActionResult> AllAdvertisement(string? type, string? address)
        {
            
                // 1. حذف الإعلانات المنتهية
                var expiredAds = await dbContext.advertisements
                    .Where(adv => adv.EndAt <= DateTime.Now)
                    .ToListAsync();

                if (expiredAds.Any())
                {
                    dbContext.advertisements.RemoveRange(expiredAds);
                    await dbContext.SaveChangesAsync();
                }

                
                var query = dbContext.advertisements
                    .Where(adv => adv.StartAt.Date <= DateTime.Now.Date && adv.EndAt.Date > DateTime.Now.Date)
                    .Include(a => a.property)
                        .ThenInclude(p => p.Type)
                    .Include(a => a.property)
                        .ThenInclude(p => p.ImageDetails)
                    .Include(a => a.property)
                        .ThenInclude(p => p.Reviews)
                    .Include(a => a.property)
                        .ThenInclude(p => p.Address)
                    .Include(a => a.property)
                        .ThenInclude(p => p.User)
                    .Include(a => a.service)
                        .ThenInclude(s => s.Type)
                    .Include(a => a.service)
                        .ThenInclude(s => s.ImageDetails)
                    .Include(a => a.service)
                        .ThenInclude(s => s.Reviews)
                    .Include(a => a.service)
                        .ThenInclude(s => s.Address)
                    .Include(a => a.service)
                        .ThenInclude(s => s.User)
                    .AsNoTracking()
                    .AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(adv =>
               (adv.property != null && adv.property.Type != null && adv.property.Type.Name.Contains(type)) ||
               (adv.service != null && adv.service.Type != null && adv.service.Type.Name.Contains(type))
                );
            }
            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(adv =>
                    (adv.property != null && adv.property.Address != null && adv.property.Address.Name.Contains(address)) ||
                    (adv.service != null && adv.service.Address != null && adv.service.Address.Name.Contains(address))
                );
            }

            // 3. التنفيذ مع معالجة الأخطاء لكل إعلان
            var result = new List<GetAllAdvertisementDTOs>();
                var allAds = await query.ToListAsync();

                foreach (var adv in allAds)
                {
                   
                        var dto = new GetAllAdvertisementDTOs
                        {
                            Id = adv.Id,
                            StartAt = adv.StartAt,
                            EndAt = adv.EndAt,
                            Properties = adv.property != null ? new GetAllPropertyDTOs
                            {
                                
                                Id = adv.property.Id,
                                Description = adv.property.Description,
                                TypeName = adv.property.Type?.Name,
                                userName = adv.property.User?.UserName,
                                AddressName = adv.property.Address?.Name,
                                UserID= adv.property.UsersID,
                                lat=adv.property.lat,
                                lng=adv.property.lng,
                                ImageDetails = adv.property.ImageDetails?.Select(img => new GetImageDTOs
                                {
                                    Id = img.Id,
                                    Name = img.Image
                                }).ToList() ?? new List<GetImageDTOs>(),
                                Reviews = adv.property.Reviews?.Select(r => new GetAllReviewDTOs
                                {
                                    Id = r.Id,
                                    UserId = r.UsersID,
                                    date = r.CreateAt,
                                    description = r.Description,
                                    rating = r.Rating,
                                }).ToList() ?? new List<GetAllReviewDTOs>()
                            } : null,
                            Services = adv.service != null ? new GetAllServiceDTOs
                            {
                                // إضافة فحص NULL لكل خاصية
                                Id = adv.service.Id,
                                Description = adv.service.Description,
                                TypeName = adv.service.Type?.Name,
                                UserName = adv.service.User?.UserName,
                                userId = adv.service.UsersID,
                                AddressName = adv.service.Address?.Name,
                                ImageDetails = adv.service.ImageDetails?.Select(img => new GetImageDTOs
                                {
                                    Id = img.Id,
                                    Name = img.Image
                                }).ToList() ?? new List<GetImageDTOs>(),
                                Reviews = adv.service.Reviews?.Select(r => new GetAllReviewDTOs
                                {
                                    Id = r.Id,
                                    UserId = r.UsersID,
                                    date = r.CreateAt,
                                    description = r.Description,
                                    rating = r.Rating,
                                }).ToList() ?? new List<GetAllReviewDTOs>()
                            } : null
                        };
                        result.Add(dto);
                    
                }

                return Ok(result);
           
        }
        [HttpGet("Suggest")]
        public async Task<IActionResult> Suggest()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (userId == null)
                return Unauthorized(new { message = "Token Is Missing or Invalid" });

            var user = await userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Address == null)
                return NotFound(new { message = "User or Address Not Found" });

            string userCity = user.Address.Name;
            if (string.IsNullOrEmpty(userCity))
                return BadRequest(new { message = "User City Is Missing" });

            // Remove expired ads
            var expiredAds = await dbContext.advertisements
                .Where(adv => adv.EndAt <= DateTime.Now)
                .Include(a => a.property)
                .Include(a => a.service)
                .ToListAsync();

            if (expiredAds.Any())
            {
                foreach (var ad in expiredAds)
                {
                    
                    dbContext.advertisements.Remove(ad);
                }
                await dbContext.SaveChangesAsync();
            }

            // Get active ads matching user's city
            var result = await dbContext.advertisements
                .Where(adv => adv.StartAt <= DateTime.Now && adv.EndAt > DateTime.Now)
                .Include(a => a.property)
                    .ThenInclude(p => p.Type)
                .Include(a => a.property)
                    .ThenInclude(p => p.ImageDetails)
                .Include(a => a.property)
                    .ThenInclude(p => p.Reviews)
                .Include(a => a.property)
                    .ThenInclude(p => p.Address)
                .Include(a => a.property)
                    .ThenInclude(p => p.User)
                .Include(a => a.service)
                    .ThenInclude(s => s.Type)
                .Include(a => a.service)
                    .ThenInclude(s => s.ImageDetails)
                .Include(a => a.service)
                    .ThenInclude(s => s.Reviews)
                .Include(a => a.service)
                    .ThenInclude(s => s.Address)
                .Include(a => a.service)
                    .ThenInclude(s => s.User)
                .Where(adv =>
                    (adv.property != null && adv.property.Address.Name == userCity) ||
                    (adv.service != null && adv.service.Address.Name == userCity))
                .Select(adv => new GetAllAdvertisementDTOs
                {
                    Id = adv.Id,
                    StartAt = adv.StartAt,
                    EndAt = adv.EndAt,
                    Properties = adv.property != null && adv.property.Address.Name == userCity ?
                        new GetAllPropertyDTOs
                        {
                            Id = adv.property.Id,
                            Description = adv.property.Description,
                            StartAt = adv.property.StartAt,
                            updateAt = adv.property.updateAt,
                            UserID = adv.property.UsersID,
                            lat = adv.property.lat,
                            lng = adv.property.lng,
                            AddressName = adv.property.Address.Name,
                            TypeName = adv.property.Type.Name,
                            userName = adv.property.User.UserName,
                            Price = adv.property.Price,
                            AvgRating = adv.property.Reviews.Any() ?
                                adv.property.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = adv.property.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = adv.property.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        } : null,
                    Services = adv.service != null && adv.service.Address.Name == userCity ?
                        new GetAllServiceDTOs
                        {
                            Id = adv.service.Id,
                            Description = adv.service.Description,
                            PriceRange = adv.service.PriceRange,
                            TypeName = adv.service.Type.Name,
                            userId = adv.service.UsersID,
                            UserName = adv.service.User.UserName,
                            AddressName = adv.service.Address.Name,
                            AvgRating = adv.service.Reviews.Any() ?
                                adv.service.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = adv.service.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = adv.service.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        } : null
                }).ToListAsync();

            // Sort by highest rating
            var sortedAdvertisements = result
                .OrderByDescending(adv =>
                    Math.Max(
                        adv.Properties?.AvgRating ?? 0,
                        adv.Services?.AvgRating ?? 0
                    ))
                .ToList();

            return Ok(sortedAdvertisements);
        }

        [HttpGet("SuggestAddress")]
        public async Task<IActionResult> SuggestAddress()
        {

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (userId == null)
                return Unauthorized(new { message = "Token Is Missing or Invalid" });

            var user = await userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Address == null)
                return NotFound(new { message = "User or Address Not Found" });

            string userCity = user.Address.Name;
            if (string.IsNullOrEmpty(userCity))
                return BadRequest(new { message = "User City Is Missing" });

            // Remove expired ads
            var expiredAds = await dbContext.advertisements
                .Where(adv => adv.EndAt <= DateTime.Now)
                .Include(a => a.property)
                .Include(a => a.service)
                .ToListAsync();

            if (expiredAds.Any())
            {
                foreach (var ad in expiredAds)
                {
                   
                    dbContext.advertisements.Remove(ad);
                }
                await dbContext.SaveChangesAsync();
            }

            // Get active ads matching user's city
            var result = await dbContext.advertisements
                .Where(adv => adv.StartAt <= DateTime.Now && adv.EndAt > DateTime.Now)
                .Include(a => a.property)
                    .ThenInclude(p => p.Type)
                .Include(a => a.property)
                    .ThenInclude(p => p.ImageDetails)
                .Include(a => a.property)
                    .ThenInclude(p => p.Reviews)
                .Include(a => a.property)
                    .ThenInclude(p => p.Address)
                .Include(a => a.property)
                    .ThenInclude(p => p.User)
                .Include(a => a.service)
                    .ThenInclude(s => s.Type)
                .Include(a => a.service)
                    .ThenInclude(s => s.ImageDetails)
                .Include(a => a.service)
                    .ThenInclude(s => s.Reviews)
                .Include(a => a.service)
                    .ThenInclude(s => s.Address)
                .Include(a => a.service)
                    .ThenInclude(s => s.User)
                .Where(adv =>
                    (adv.property != null && adv.property.Address.Name == userCity) ||
                    (adv.service != null && adv.service.Address.Name == userCity))
                .Select(adv => new GetAllAdvertisementDTOs
                {
                    Id = adv.Id,
                    StartAt = adv.StartAt,
                    EndAt = adv.EndAt,
                    Properties = adv.property != null && adv.property.Address.Name == userCity ?
                        new GetAllPropertyDTOs
                        {
                            Id = adv.property.Id,
                            Description = adv.property.Description,
                            StartAt = adv.property.StartAt,
                            updateAt = adv.property.updateAt,
                            UserID = adv.property.UsersID,
                            lat = adv.property.lat,
                            lng = adv.property.lng,
                            AddressName = adv.property.Address.Name,
                            TypeName = adv.property.Type.Name,
                            userName = adv.property.User.UserName,
                            Price = adv.property.Price,
                            AvgRating = adv.property.Reviews.Any() ?
                                adv.property.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = adv.property.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = adv.property.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        } : null,
                    Services = adv.service != null && adv.service.Address.Name == userCity ?
                        new GetAllServiceDTOs
                        {
                            Id = adv.service.Id,
                            Description = adv.service.Description,
                            PriceRange = adv.service.PriceRange,
                            TypeName = adv.service.Type.Name,
                            userId = adv.service.UsersID,
                            UserName = adv.service.User.UserName,
                            AddressName = adv.service.Address.Name,
                            AvgRating = adv.service.Reviews.Any() ?
                                adv.service.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = adv.service.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = adv.service.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        } : null
                }).ToListAsync();

            

            return Ok(result);
        }

    }
}
