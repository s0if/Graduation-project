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

        public AdvertisementController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpPost("AddServiceAdvertisement")]
        public async Task<IActionResult> AddServiceAdvertisement(AddServiceAdvertisementDTOs request)
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
                return Unauthorized();
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
                if (role.Contains("provider"))
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
        public async Task<IActionResult> DeleteServiceAdvertisement(int AdvertisementId)
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
        public async Task<IActionResult> DeletePropertyAdvertisement(int AdvertisementId)
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

            var deleteEnd = await dbContext.advertisements.Where(adv => adv.EndAt < DateTime.Now)
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
                    AddressName = p.Address.Name,
                    ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    }).ToList(),
                    Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        UserName = dbContext.users.Where(u => u.Id == r.UsersID).Select(u => u.UserName).FirstOrDefault(),
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
                    AddressName = ser.Address.Name,
                    ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                    {
                        Id = img.Id,
                        Name = img.Image
                    }).ToList(),
                    Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
                    {
                        Id = r.Id,
                        UserName = dbContext.users.Where(u => u.Id == r.UsersID).Select(u => u.UserName).FirstOrDefault(),
                        date = r.CreateAt,
                        description = r.Description,
                        rating = r.Rating,
                    }).ToList(),
                }).ToList()
            }).ToListAsync();
            return Ok(result);

        }

        [HttpGet("SuggestAdvertisement")]
        public async Task<IActionResult> SuggestAdvertisement()
        {
           
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            var user = await userManager.Users
                .Include(u => u.Address) 
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Address == null)
                return NotFound(new { message = "User or Address Not Found" });
            string userCity = user.Address.Name; 
            var deleteEnd = await dbContext.advertisements
                .Where(adv => adv.EndAt < DateTime.Now)
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
                    (adv.Properties.Any(p => p.Address.Name == userCity )) ||
                    (adv.Services.Any(s => s.Address.Name == userCity))  )
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
                                UserName = dbContext.users.Where(u => u.Id == r.UsersID).Select(u => u.UserName).FirstOrDefault(),
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
                            AddressName = ser.Address.Name,
                            ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id = img.Id,
                                Name = img.Image
                            }).ToList(),
                            Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
                            {
                                Id = r.Id,
                                UserName = dbContext.users.Where(u => u.Id == r.UsersID).Select(u => u.UserName).FirstOrDefault(),
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        }).ToList()
                }).ToListAsync();

            return Ok(result);
        }




        //[HttpGet("SuggestAdvertisementReview")]
        //public async Task<IActionResult> SuggestAdvertisementReview()
        //{
        //    // Extract and validate the token
        //    string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
        //    if (string.IsNullOrEmpty(token))
        //        return Unauthorized(new { message = "Token Is Missing" });

        //    // Extract the user ID from the token
        //    int? userId = ExtractClaims.ExtractUserId(token);
        //    if (string.IsNullOrEmpty(userId.ToString()))
        //        return Unauthorized(new { message = "Token Is Missing" });

        //    // Fetch the user's address
        //    var user = await userManager.Users
        //        .Include(u => u.Address) // Assuming the user has an Address navigation property
        //        .FirstOrDefaultAsync(u => u.Id == userId);

        //    if (user == null || user.Address == null)
        //        return NotFound(new { message = "User or Address Not Found" });

        //    string userCity = user.Address.Name; // Assuming the address has a Name property

        //    // Fetch the user's last review (either for a service or property)
        //    var lastReview = await dbContext.reviews
        //        .Where(r => r.UsersID == userId)
        //        .OrderByDescending(r => r.CreateAt)
        //        .FirstOrDefaultAsync();

        //    double? userRating = lastReview?.Rating; // Get the rating from the last review

        //    // Delete expired advertisements
        //    var deleteEnd = await dbContext.advertisements
        //        .Where(adv => adv.EndAt < DateTime.Now)
        //        .Include(adv => adv.Services)
        //        .Include(adv => adv.Properties)
        //        .ToListAsync();

        //    foreach (var adv in deleteEnd)
        //    {
        //        dbContext.advertisements.RemoveRange(adv);
        //        await dbContext.SaveChangesAsync();
        //    }

        //    // Fetch advertisements with similar addresses and matching ratings
        //    var result = await dbContext.advertisements
        //        .Include(a => a.Properties)
        //            .ThenInclude(p => p.Type)
        //        .Include(a => a.Properties)
        //            .ThenInclude(p => p.ImageDetails)
        //        .Include(a => a.Properties)
        //            .ThenInclude(p => p.Reviews)
        //        .Include(a => a.Properties)
        //            .ThenInclude(p => p.Address)
        //        .Include(a => a.Services)
        //            .ThenInclude(s => s.Type)
        //        .Include(a => a.Services)
        //            .ThenInclude(s => s.ImageDetails)
        //        .Include(a => a.Services)
        //            .ThenInclude(s => s.Reviews)
        //        .Include(a => a.Services)
        //            .ThenInclude(s => s.Address)
        //        .Where(adv =>
        //            (adv.Properties.Any(p => p.Address.Name == userCity &&
        //                                    (!userRating.HasValue || p.Reviews.Any(r => r.Rating >= userRating)))) ||
        //            (adv.Services.Any(s => s.Address.Name == userCity &&
        //                                  (!userRating.HasValue || s.Reviews.Any(r => r.Rating >= userRating))))  )
        //        .Select(adv => new GetAllAdvertisementDTOs
        //        {
        //            Id = adv.Id,
        //            StartAt = adv.StartAt,
        //            EndAt = adv.EndAt,
        //            Properties = adv.Properties
        //                .Where(p => p.Address.Name == userCity &&
        //                            (!userRating.HasValue || p.Reviews.Any(r => r.Rating >= userRating)))
        //                .Select(p => new GetAllPropertyDTOs
        //                {
        //                    Id = p.Id,
        //                    Description = p.Description,
        //                    StartAt = p.StartAt,
        //                    EndAt = p.EndAt,
        //                    UserID = p.UsersID,
        //                    AddressName = p.Address.Name,
        //                    TypeName = p.Type != null ? p.Type.Name : null,
        //                    ImageDetails = p.ImageDetails.Select(img => new GetImageDTOs
        //                    {
        //                        Id = img.Id,
        //                        Name = img.Image
        //                    }).ToList(),
        //                    Reviews = p.Reviews.Select(r => new GetAllReviewDTOs
        //                    {
        //                        Id = r.Id,
        //                        date = r.CreateAt,
        //                        description = r.Description,
        //                        rating = r.Rating,
        //                    }).ToList(),
        //                }).ToList(),
        //            Services = adv.Services
        //                .Where(s => s.Address.Name == userCity &&
        //                            (!userRating.HasValue || s.Reviews.Any(r => r.Rating >= userRating)))
        //                .Select(ser => new GetAllServiceDTOs
        //                {
        //                    Id = ser.Id,
        //                    Description = ser.Description,
        //                    PriceRange = ser.PriceRange,
        //                    TypeName = ser.Type != null ? ser.Type.Name : null,
        //                    userId = ser.UsersID,
        //                    AddressName = ser.Address.Name,
        //                    ImageDetails = ser.ImageDetails.Select(img => new GetImageDTOs
        //                    {
        //                        Id = img.Id,
        //                        Name = img.Image
        //                    }).ToList(),
        //                    Reviews = ser.Reviews.Select(r => new GetAllReviewDTOs
        //                    {
        //                        Id = r.Id,
        //                        date = r.CreateAt,
        //                        description = r.Description,
        //                        rating = r.Rating,
        //                    }).ToList(),
        //                }).ToList()
        //        }).ToListAsync();

        //    return Ok(result);
        //}





        [HttpGet("SuggestAdvertisementReview")]
        public async Task<IActionResult> SuggestAdvertisementReview()
        {
            // Extract and validate the token
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            // Extract the user ID from the token
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });

            // Fetch the user's address
            var user = await userManager.Users
                .Include(u => u.Address) // Assuming the user has an Address navigation property
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Address == null)
                return NotFound(new { message = "User or Address Not Found" });

            string userCity = user.Address.Name; // Assuming the address has a Name property

            // Fetch the user's last review (either for a service or property)
            var lastReview = await dbContext.reviews
                .Where(r => r.UsersID == userId)
                .OrderByDescending(r => r.CreateAt)
                .FirstOrDefaultAsync();

            double? userRating = lastReview?.Rating; // Get the rating from the last review

            // Delete expired advertisements
            var deleteEnd = await dbContext.advertisements
                .Where(adv => adv.EndAt < DateTime.Now)
                .Include(adv => adv.Services)
                .Include(adv => adv.Properties)
                .ToListAsync();

            foreach (var adv in deleteEnd)
            {
                dbContext.advertisements.RemoveRange(adv);
                await dbContext.SaveChangesAsync();
            }

            // Fetch advertisements with similar addresses and matching ratings
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
                    (adv.Properties.Any(p => p.Address.Name == userCity &&
                                            (!userRating.HasValue || p.Reviews.Average(r => r.Rating) >= userRating))) ||
                    (adv.Services.Any(s => s.Address.Name == userCity &&
                                          (!userRating.HasValue || s.Reviews.Average(r => r.Rating) >= userRating))))
                .Select(adv => new GetAllAdvertisementDTOs
                {
                    Id = adv.Id,
                    StartAt = adv.StartAt,
                    EndAt = adv.EndAt,
                    Properties = adv.Properties
                        .Where(p => p.Address.Name == userCity &&
                                    (!userRating.HasValue || p.Reviews.Average(r => r.Rating) >= userRating))
                        .Select(p => new GetAllPropertyDTOs
                        {
                            Id = p.Id,
                            Description = p.Description,
                            StartAt = p.StartAt,
                            EndAt = p.EndAt,
                            UserID = p.UsersID,
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
                                date = r.CreateAt,
                                description = r.Description,
                                rating = r.Rating,
                            }).ToList(),
                        }).ToList(),
                    Services = adv.Services
                        .Where(s => s.Address.Name == userCity &&
                                    (!userRating.HasValue || s.Reviews.Average(r => r.Rating) >= userRating))
                        .Select(ser => new GetAllServiceDTOs
                        {
                            Id = ser.Id,
                            Description = ser.Description,
                            PriceRange = ser.PriceRange,
                            TypeName = ser.Type != null ? ser.Type.Name : null,
                            userId = ser.UsersID,
                            AddressName = ser.Address.Name,
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

            return Ok(result);
        }



    }
}
