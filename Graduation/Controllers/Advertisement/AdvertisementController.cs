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

        public AdvertisementController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,ExtractClaims extractClaims)
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
                int? userId =  await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);
                if (role.Contains("provider") || role.Contains("admin"))
                {
                    ServiceProject result = await dbContext.services.FindAsync(request.serviceId);
                    if (result.AdvertisementID is null)
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
                    if (result.AdvertisementID is null)
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
                    return BadRequest(new { message = "There is an advertisement" });
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
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);

                AdvertisementProject result = await dbContext.advertisements.FindAsync(AdvertisementId);

                if (result is not null)
                {
                    ServiceProject resultService = await dbContext.services.FirstOrDefaultAsync(x => x.AdvertisementID == result.Id);
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
                        return Unauthorized(new { message = "Only Admins or the service provider can update this advertisement" });
                    }
                    return BadRequest(new { message = "not found service" });
                }
                return BadRequest(new { message = "not found Advertisement" });
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
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);

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
                        return Unauthorized(); return Unauthorized(new { message = "Only Admins or the property provider can update this advertisement" });
                    }
                    return BadRequest(new { message = "not found property" });
                }
                return BadRequest(new { message = "not found Advertisement" });

            }
            return NotFound();
        }
        [HttpDelete("DeleteServiceAdvertisement")]
        public async Task<IActionResult> DeleteServiceAdvertisement(int AdvertisementId)
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
                        return Unauthorized(new { message = "Only Admins or the service provider can delete this advertisement" });
                    }
                    return BadRequest(new { message = "not found service" });
                }
                return BadRequest(new { message = "not found Advertisement" });

            }
            return NotFound();
        }
        [HttpDelete("DeletePropertyAdvertisement")]
        public async Task<IActionResult> DeletePropertyAdvertisement(int AdvertisementId)
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
                        return Unauthorized(new { message = "Only Admins or the property provider can delete this advertisement" });
                    }
                    return BadRequest(new { message = "not found property" });
                }
                return BadRequest(new { message = "not found Advertisement" });
            }
            return NotFound();
        }
        [HttpGet("AllAdvertisement")]
        public async Task<IActionResult> AllAdvertisement(string? type, string? address)
        {
            var query = dbContext.services.AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.Type.Name.Contains(type));
            }
            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(p => p.Address.Name.Contains(address));
            }
            var queryP = dbContext.properties.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                queryP = queryP.Where(p => p.Type.Name.Contains(type));
            }
            if (!string.IsNullOrEmpty(address))
            {
                queryP = queryP.Where(p => p.Address.Name.Contains(address));
            }

            var deleteEnd = await dbContext.advertisements
                .Where(adv => adv.EndAt <= DateTime.Now)
                .Include(adv => adv.Services)
                .Include(adv => adv.Properties)
                .ToListAsync();
            foreach (var adv in deleteEnd)
            {

                dbContext.advertisements.RemoveRange(adv);
                await dbContext.SaveChangesAsync();
            }
            var result = await dbContext.advertisements
                .Where(adv => adv.StartAt < DateTime.Now)
            .Include(a => a.Properties)
                .ThenInclude(p => p.Type)
            .Include(a => a.Properties)
                .ThenInclude(p => p.ImageDetails)
            .Include(a => a.Properties)
                .ThenInclude(p => p.Reviews)
                 .Include(a => a.Properties)
            .ThenInclude(p => p.Address)
            .Include(a => a.Services)
                .ThenInclude(s => s.Type)
            .Include(a => a.Services)
                .ThenInclude(s => s.ImageDetails)
            .Include(a => a.Services)
                .ThenInclude(s => s.Reviews)
                .Include(a => a.Services)
            .ThenInclude(s => s.Address)
                .OrderByDescending(adv => adv.Id)
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
                    lat=p.lat,
                    lng=p.lng,
                    TypeName = p.Type != null ? p.Type.Name : null,
                    userName = p.User.UserName,
                    AddressName = p.Address.Name,
                    ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    }).ToList(),
                    Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        UserId = r.UsersID,
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
                    lng=ser.lng,
                    lat =ser.lat,
                    UserName = ser.User.UserName,
                    AddressName = ser.Address.Name,
                    ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    }).ToList(),
                    Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        UserId = r.UsersID,
                        date = r.CreateAt,
                        description = r.Description,
                        rating = r.Rating,
                    }).ToList(),
                }).ToList()
            }).ToListAsync();
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

            var deleteEnd = await dbContext.advertisements
                .Where(adv => adv.EndAt <= DateTime.Now)
                .ToListAsync();

            if (deleteEnd.Any())
            {
                dbContext.advertisements.RemoveRange(deleteEnd);
                await dbContext.SaveChangesAsync();
            }
            var result = await dbContext.advertisements
                .Include(a => a.Properties)
                    .ThenInclude(p => p.Type)
                .Include(a => a.Properties)
                    .ThenInclude(p => p.ImageDetails)
                .Include(a => a.Properties)
                    .ThenInclude(p => p.Reviews)
                .Include(a => a.Properties)
                    .ThenInclude(p => p.Address)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Type)
                .Include(a => a.Services)
                    .ThenInclude(s => s.ImageDetails)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Reviews)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Address)
                .Where(adv =>
                    (adv.Properties.Any(p => p.Address.Name == userCity)) ||
                    (adv.Services.Any(s => s.Address.Name == userCity)))
                .Select(adv => new GetAllAdvertisementDTOs
                {
                    Id = adv.Id,
                    StartAt = adv.StartAt,
                    EndAt = adv.EndAt,
                    Properties = adv.Properties
                        .Select(p => new GetAllPropertyDTOs
                        {
                            Id = p.Id,
                            Description = p.Description,
                            StartAt = p.StartAt,
                            EndAt = p.EndAt,
                            UserID = p.UsersID,
                            lat=p.lat,
                            lng=p.lng,
                            AddressName = p.Address.Name,
                            TypeName = p.Type != null ? p.Type.Name : null,
                            userName = p.User.UserName,
                            Price = p.Price,
                            AvgRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        }).ToList(),
                    Services = adv.Services
                        .Select(ser => new GetAllServiceDTOs
                        {
                            Id = ser.Id,
                            Description = ser.Description,
                            PriceRange = ser.PriceRange,
                            TypeName = ser.Type != null ? ser.Type.Name : null,
                            userId = ser.UsersID,
                            lat=ser.lat,
                            lng=ser.lng,
                            UserName = ser.User.UserName,
                            AddressName = ser.Address.Name,
                            AvgRating = ser.Reviews.Any() ? ser.Reviews.Average(r => r.Rating) : 0,
                            ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
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

            var sortedAdvertisements = result
                .OrderByDescending(adv =>
                    Math.Max(
                        adv.Properties.DefaultIfEmpty().Max(p => p?.AvgRating ?? 0),
                        adv.Services.DefaultIfEmpty().Max(s => s?.AvgRating ?? 0)
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
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            var user = await userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Address == null)
                return NotFound(new { message = "User or Address Not Found" });
            string userCity = user.Address.Name;
            var deleteEnd = await dbContext.advertisements
                .Where(adv => adv.EndAt <= DateTime.Now)
                .Include(adv => adv.Services)
                .Include(adv => adv.Properties)
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
                .Include(a => a.Properties)
                    .ThenInclude(p => p.Address)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Type)
                .Include(a => a.Services)
                    .ThenInclude(s => s.ImageDetails)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Reviews)
                .Include(a => a.Services)
                    .ThenInclude(s => s.Address)
                .Where(adv =>
                    (adv.Properties.Any(p => p.Address.Name == userCity)) ||
                    (adv.Services.Any(s => s.Address.Name == userCity)))
                .Select(adv => new GetAllAdvertisementDTOs
                {
                    Id = adv.Id,
                    StartAt = adv.StartAt,
                    EndAt = adv.EndAt,
                    Properties = adv.Properties
                        .Where(p => p.Address.Name == userCity)
                        .Select(p => new GetAllPropertyDTOs
                        {
                            Id = p.Id,
                            Description = p.Description,
                            StartAt = p.StartAt,
                            EndAt = p.EndAt,
                            UserID = p.UsersID,
                            lat=p.lat,
                            lng=p.lng,
                            userName = p.User.UserName,
                            AddressName = p.Address.Name,
                            TypeName = p.Type != null ? p.Type.Name : null,
                            ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                UserId = r.UsersID,
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        }).ToList(),
                    Services = adv.Services
                        .Where(s => s.Address.Name == userCity)
                        .Select(ser => new GetAllServiceDTOs
                        {
                            Id = ser.Id,
                            Description = ser.Description,
                            PriceRange = ser.PriceRange,
                            TypeName = ser.Type != null ? ser.Type.Name : null,
                            userId = ser.UsersID,
                            lat=ser.lat,
                            lng=ser.lng,
                            UserName = ser.User.UserName,
                            AddressName = ser.Address.Name,
                            ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                UserId = r.UsersID,
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
