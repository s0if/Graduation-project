using Graduation.Data;
using Graduation.DTOs.Auth;
using Graduation.DTOs.Email;
using Graduation.DTOs.Images;
using Graduation.DTOs.Profile;
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

namespace Graduation.Controllers.User
{
    [Route("[controller]")]
    [ApiController]
    public class UserOperationsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public UserOperationsController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        [HttpGet("AllUser")]
        public async Task<IActionResult> AllUser()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            IList<string> role = await userManager.GetRolesAsync(requestUser);

            if (role.Contains("admin"))
            {

                List<AuthGetAllUserDTOs> AllUser = new List<AuthGetAllUserDTOs>();
                IEnumerable<ApplicationUser> GetUsers = await userManager.Users.AsSplitQuery().Include(A => A.Address).ToListAsync();
                foreach (ApplicationUser user in GetUsers)
                {
                    AllUser.Add(new AuthGetAllUserDTOs
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        Address = user.Address?.Name,
                        Role = string.Join(",", await userManager.GetRolesAsync(user))
                    });
                }
                return Ok(AllUser);
            }
            return Unauthorized();
        }
        [HttpGet("UserProvider")]
        public async Task<IActionResult> UserProvider()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            IList<string> role = await userManager.GetRolesAsync(requestUser);

            if (role.Contains("admin"))
            {

                List<AuthGetAllUserDTOs> AllUser = new List<AuthGetAllUserDTOs>();
                IEnumerable<ApplicationUser> GetUsers = await userManager.Users.AsSplitQuery().Include(A => A.Address).ToListAsync();
                foreach (ApplicationUser user in GetUsers)
                {
                    var result = await userManager.GetRolesAsync(user);
                    if (result.Contains("provider"))
                    {

                        AllUser.Add(new AuthGetAllUserDTOs
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            Phone = user.PhoneNumber,
                            Address = user.Address?.Name,
                            Role = string.Join(",", await userManager.GetRolesAsync(user))
                        });
                    }
                }
                return Ok(AllUser);
            }
            return Unauthorized();
        }
        [HttpGet("profile")]
        public async Task<IActionResult> profile()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.AsSplitQuery().Include(A => A.Address).FirstOrDefaultAsync(u => u.Id == userId);
            AuthGetAllUserDTOs result = new AuthGetAllUserDTOs
            {
                Id = requestUser.Id,
                UserName = requestUser.UserName,
                Email = requestUser.Email,
                Phone = requestUser.PhoneNumber,
                Address = requestUser.Address?.Name,
                Role = string.Join(",", await userManager.GetRolesAsync(requestUser))
            };
            return Ok(result);
        }

        [HttpGet("profileProvider")]
        public async Task<IActionResult> profileProvider(int Id)
        {

            ApplicationUser requestUser = await userManager.Users.
                FirstOrDefaultAsync(u=>u.Id==Id);
            var role=await userManager.GetRolesAsync(requestUser);  
            if (requestUser is null)
            {
                return BadRequest(new { message = "provider not found" });
            }


            var resultProperty =await dbContext.properties
                .Include(p => p.Address)
                .Include(p=>p.Type)
                .Include(p=>p.ImageDetails)
                .Include(p=>p.Reviews)
                .Where(p=>p.UsersID == Id).ToListAsync();
            var resultService=await dbContext.services
                .Include(s => s.Address)
                .Include(s => s.Type)
                .Include(s => s.ImageDetails)
                .Include(s => s.Reviews)
                .Where(s=>s.UsersID == Id).ToListAsync();
            if (resultProperty.Count<1 && resultService.Count<1)
            {
                return NotFound("No properties or services found for the given user ID.");
            }
            ProviderDTOs result = new ProviderDTOs
            {
              PropertyDTOs= resultProperty?.Select(RP => new GetAllPropertyDTOs
              {
                  Id = RP.Id,
                  AddressName = RP.Address.Name,
                  Description = RP.Description,
                  StartAt = RP.StartAt,
                  EndAt = RP.EndAt,
                  Price = RP.Price,
                  TypeName = RP.Type.Name,
                  userName = requestUser.UserName,
                  UserID = Id,
                  ImageDetails = RP.ImageDetails?
                  .Select(img => new GetImageDTOs
                  {
                      Id = img.Id,
                      Name = img.Image
                  }).ToList() ?? new List<GetImageDTOs>(),
                  Reviews = RP.Reviews?
                  .Select(r =>
                   new GetAllReviewDTOs
                   {
                       Id = r.Id,
                       UserId = r.UsersID,
                       description = r.Description,
                       date = r.CreateAt,
                       rating = r.Rating,

                   }

                  ).ToList() ?? new List<GetAllReviewDTOs>(),

                  AvgRating = RP.Reviews.Any() ? RP.Reviews.Average(r => r.Rating) : 0
              }).ToList() ?? new List<GetAllPropertyDTOs>(),
              ServiceDTOs = resultService?.Select(RS => new GetAllServiceDTOs
              {
                  Id = RS.Id,
                  userId = Id,
                  UserName = requestUser.UserName,
                  Description = RS.Description,
                  AddressName = RS.Address.Name,
                  PriceRange = RS.PriceRange,
                  TypeName = RS.Type.Name,
                  ImageDetails = RS.ImageDetails?
                   .Select(img => new GetImageDTOs
                   {
                       Id = img.Id,
                       Name = img.Image
                   })
                   .ToList() ?? new List<GetImageDTOs>(),
                  Reviews = RS.Reviews?
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

                  AvgRating = RS.Reviews.Any() ? RS.Reviews.Average(r => r.Rating) : 0
              }).ToList() ?? new List<GetAllServiceDTOs>(),
            };
            
            return Ok(result);
        }

        [HttpGet("User")]
        public async Task<IActionResult> User(int Id)
        {

            ApplicationUser requestUser = await userManager.Users.AsSplitQuery().Include(A => A.Address).FirstOrDefaultAsync(u => u.Id == Id);
            AuthGetAllUserDTOs result = new AuthGetAllUserDTOs
            {
                Id = requestUser.Id,
                UserName = requestUser.UserName,
                Email = requestUser.Email,
                Phone = requestUser.PhoneNumber,
                Address = requestUser.Address?.Name,
                Role = string.Join(",", await userManager.GetRolesAsync(requestUser))
            };
            return Ok(result);
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            IList<string> role = await userManager.GetRolesAsync(requestUser);
            if (role.Contains("admin") || requestUser.Id == Id)
            {
                ApplicationUser User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == Id);
                await userManager.DeleteAsync(User);
                return Ok(new { message = "delete successful" });
            }

            return Unauthorized();
        }
        [HttpPut("changeRole")]
        public async Task<IActionResult> changeRole(AuthChangeRoleDTOs request)
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

                if (role.Contains("admin"))
                {
                    ApplicationUser user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
                    IList<string> OldRoles = await userManager.GetRolesAsync(user);

                    foreach (var oldRole in OldRoles)
                    {

                        IdentityResult deleteRole = await userManager.RemoveFromRoleAsync(user, oldRole);
                        if (deleteRole.Succeeded)
                        {
                            IdentityResult addRole = await userManager.AddToRoleAsync(user, request.NewRole);
                            if (addRole.Succeeded)
                            {
                                return Ok(new { status = 200, message = "successful change role" });
                            }
                            IList<string> errorMessageAdd = deleteRole.Errors.Select(error => error.Description).ToList();
                            return BadRequest(string.Join(", ", errorMessageAdd));
                        }
                        IList<string> errorMessageDelete = deleteRole.Errors.Select(error => error.Description).ToList();
                        return BadRequest(string.Join(", ", errorMessageDelete));
                    }


                }
                return Unauthorized();
            }
            return NotFound();

        }
        [HttpPut("UpdatePhone")]
        public async Task<IActionResult> UpdatePhone(string phone)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (requestUser is not null)
            {
                requestUser.PhoneNumber = phone;
                await userManager.UpdateAsync(requestUser);

                return Ok(new { status = 200, message = "update successful" });
            }

               ;
            return BadRequest(new { message = "user not found" });

        }

        [HttpPut("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress(int addressId)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (requestUser is not null)
            {
                requestUser.AddressId = addressId;
                await userManager.UpdateAsync(requestUser);

                return Ok(new { status = 200, message = "update successful" });
            }

               ;
            return BadRequest(new { message = "user not found" });

        }


        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail(SendEmailDTOs request)
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

                if (role.Contains("admin"))
                {
                    ApplicationUser user = await dbContext.users.FindAsync(request.UserId);
                    if (user is not null)
                    {

                        string Body = "";
                        if (request.Image is not null)
                        {

                            ImageDetails imageDetails = new ImageDetails()
                            {
                                Image = await FileSettings.UploadFileAsync(request.Image),
                            };
                            await dbContext.images.AddAsync(imageDetails);
                            await dbContext.SaveChangesAsync();
                            var imageName = imageDetails.Image;
                             Body = $@"
                                <html>
                                <body>
                                    <p>{request.Body}</p>
                                    <img src='{imageName}' alt='Embedded Image' style='max-width: 50%; height: auto;'/>
                                </body>
                                </html>";
                        }
                        if (Body == "")
                        {
                            Body = $@"
                                <html>
                                <body>
                                    <p>{request.Body}</p>
                                   
                                </body>
                                </html>";
                        }
                        EmailDTOs email = new EmailDTOs()
                        {
                            Subject = request.Title,
                            Recivers = user.Email,
                            Body = Body
                        };

                        EmailSetting.SendEmail(email);
                        return Ok(new { mwssage = "Send email successful" });
                    }
                    return BadRequest(new { message = "not found user" });

                }
                return Unauthorized();
            }
            return NotFound();

        }

    }
}
