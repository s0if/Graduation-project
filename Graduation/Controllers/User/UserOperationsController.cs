using Azure.Storage.Blobs.Models;
using Graduation.Data;
using Graduation.DTOs.Address;
using Graduation.DTOs.Admin;
using Graduation.DTOs.Auth;
using Graduation.DTOs.Email;
using Graduation.DTOs.Images;
using Graduation.DTOs.Message;
using Graduation.DTOs.Profile;
using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.Reviews;
using Graduation.DTOs.ServiceToProject;
using Graduation.Helpers;
using Graduation.Model;
using Graduation.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
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
                        Role = string.Join(",", await userManager.GetRolesAsync(user))  ,
                        ConfirmEmail=user.EmailConfirmed,
                    });
                }
                return Ok(AllUser);
            }
            return Unauthorized(new { message = "Only admin  can get all user" });
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
            return Unauthorized(new { message = "Only admin  can get all provider" });
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


        [HttpGet("count")]
        public async Task<IActionResult> count()
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
                int userProvider = 0;
                int userConsumer = 0;
                int userAdmin = 0;
                IEnumerable<ApplicationUser> allUser = await userManager.Users.ToListAsync();
                foreach (var item in allUser)
                {
                    var result = await userManager.GetRolesAsync(item);
                    if(result.Contains("admin"))
                    {
                        ++userAdmin;
                    }
                    if (result.Contains("provider"))
                    {
                        ++userProvider;
                    }
                    if (result.Contains("consumer"))
                    {
                        ++userConsumer;
                    }

                }

                int advertisementsService = 0;
                int advertisementsProperty = 0;
                IEnumerable<AdvertisementProject> advertisements=await dbContext.advertisements
                    .Include(adv=>adv.Services)
                    .Include(adv=>adv.Properties)
                    .ToListAsync();
                foreach (var item in advertisements)
                {
                    advertisementsService += item.Services.Count();
                    advertisementsProperty += item.Properties.Count();
                }

                int service = await dbContext.services.CountAsync();
                int property = await dbContext.properties.CountAsync();
                int typeService=await dbContext.typeServices.CountAsync();
                int typeProperty = await dbContext.typeProperties.CountAsync();
                int complaint=await dbContext.complaints.CountAsync();

                CountDTOs count = new CountDTOs()
                {
                    CountAdmin = userAdmin,
                    CountProvider = userProvider,
                    CountConsumer = userConsumer,
                    CountUser = allUser.Count(),
                    AdvertisementsCount = advertisements.Count(),
                    AdvertisementsService=advertisementsService,
                    AdvertisementsProperty=advertisementsProperty ,
                    AllProperty=property,
                    AllService=service ,
                    TypeProperty=typeProperty,
                    TypeService=typeService,
                    Complaint=complaint,
                    
                };
                return Ok(new { message = "Site statistics", count });

            }
            return Unauthorized(new { message = "Only admin  can get all user" });
        }
        [HttpGet("countAddress")]
        public async Task<IActionResult> countAddress()
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
                List<CountAddressDTOs> countAddressToUser = new List<CountAddressDTOs>();
                List<CountAddressDTOs> countAddressToProperty = new List<CountAddressDTOs>();
                List<CountAddressDTOs> countAddressToAddress = new List<CountAddressDTOs>();


                IEnumerable<AddressToProject> address=await dbContext.addresses.ToListAsync();
                int countAddress =  address.Count();  
                foreach (var item in address)
                {
                    var user = await userManager.Users.Where(u => u.AddressId == item.Id).CountAsync();
                    var countAddressUser = new CountAddressDTOs()
                    {    
                        NameAddress = item.Name,
                        CountAddress = user
                    };
                    countAddressToUser.Add(countAddressUser);
                    var property = await dbContext.properties.Where(p => p.AddressId == item.Id).CountAsync();
                    var CountAddressProperty = new CountAddressDTOs()
                    {
                        NameAddress = item.Name,
                        CountAddress = property
                    };
                    countAddressToProperty.Add(CountAddressProperty);
                    var Service=await dbContext.services.Where(s=>s.AddressId==item.Id).CountAsync();
                    var countAddressService = new CountAddressDTOs()
                    {
                        NameAddress = item.Name,
                        CountAddress = Service
                    };
                    countAddressToAddress.Add(countAddressService);
                }
                return Ok(new {message="All Address", countAddress, message1="user", countAddressToUser,message2="property", countAddressToProperty,message3="service", countAddressToAddress });
            }
            return Unauthorized(new { message = "Only admin  can get all user" });
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
                var resultProperty = await dbContext.properties
                    .Include(p => p.Address)
                    .Include(p => p.Type)
                    .Include(p => p.ImageDetails)
                    .Include(p => p.Reviews)
                    .Where(p => p.UsersID == Id).ToListAsync();
                var resultService = await dbContext.services
                    .Include(s => s.Address)
                    .Include(s => s.Type)
                    .Include(s => s.ImageDetails)
                    .Include(s => s.Reviews)
                    .Where(s => s.UsersID == Id).ToListAsync();
                if (resultProperty.Count >1 && resultService.Count >1)
                {
                    foreach (var result in resultProperty)
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
                    }
                    foreach (var result in resultService)
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
                    }

                }
                ApplicationUser User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == Id);
                await userManager.DeleteAsync(User);
                return Ok(new { message = "delete successful" });
            }

            return Unauthorized(new { message = "Only admin or the user can delete user" });
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
                return Unauthorized(new { message = "Only admin  can change role" });
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
                    string Body = "";
                    if (request.UserId is not null)
                    {
                        ApplicationUser user = await dbContext.users.FindAsync(request.UserId);

                        if (user is not null)
                        {
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
                    if (request.UserId is  null)
                    {
                        IEnumerable <ApplicationUser> allUser = await dbContext.users.ToListAsync();
                        if (allUser.Any() )
                        {
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
                                            <img src='{imageName}' alt='Embedded Image' style='max-width: 50%; height: auto; display: block; margin: auto;'/>
                                            <p>{request.Body}</p>
                                        </body>
                                    </html> ";

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
                            foreach (var user in allUser)
                            {
                                EmailDTOs email = new EmailDTOs()
                                {
                                    Subject = request.Title,
                                    Recivers = user.Email,
                                    Body = Body
                                };
                            EmailSetting.SendEmail(email);
                            }
                           

                            return Ok(new { mwssage = "Send email all user successful" });
                        }
                        return BadRequest(new { message = "not found user" });
                    }
                    
                    

                }
                return Unauthorized(new { message = "Only admin  can send email" });
            }
            return NotFound();

        }


        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(ChatMessageDTOs request)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });

            
            ApplicationUser requestUserSent = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == request.ReceiverId);
            if (requestUser is null || requestUserSent is null)
                return NotFound(new { message = "User not found" });


            ChatMessage message = new ChatMessage
            {
                SenderId = requestUserSent.Id,
                ReceiverId = request.ReceiverId,
                Message = request.Message
            };
            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync();

            
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
            await hubContext.Clients.User(request.ReceiverId.ToString()).SendAsync("ReceiveMessage", requestUserSent.Id, request.Message);

            return Ok(new { Message = "Message sent successfully." });
        }

        [HttpGet("HistoryMessage")]
        public async Task<IActionResult> GetChatHistory(int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });

            
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            ApplicationUser requestUserSent = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (requestUser is null|| requestUserSent is null)
                return NotFound(new { message = "User not found" });

            IEnumerable<ChatMessage> messages = await dbContext.Messages
                .Where(m => (m.SenderId == requestUser.Id && m.ReceiverId == Id) ||
                           (m.SenderId == Id && m.ReceiverId == requestUser.Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            
            IEnumerable<HistoryMessageDTOs> historyMessage = messages.Select(m => new HistoryMessageDTOs
            {
                Message = m.Message,
                Timestamp = m.Timestamp,
            });

            return Ok(historyMessage);
        }
    }
}
