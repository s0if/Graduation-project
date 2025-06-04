using Azure.Core;
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
using Graduation.DTOs.Search;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Graduation.Controllers.User
{
    [Route("[controller]")]
    [ApiController]
    public class UserOperationsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;
        private readonly ExtractClaims extractClaims;
        private readonly ILogger<UserOperationsController> logger;

        public UserOperationsController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ExtractClaims extractClaims, ILogger<UserOperationsController> logger)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.extractClaims = extractClaims;
            this.logger = logger;
        }

        [HttpGet("AllUser")]
        public async Task<IActionResult> AllUser()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = await extractClaims.ExtractUserId(token);
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
                        Role = string.Join(",", await userManager.GetRolesAsync(user)),
                        ConfirmEmail = user.EmailConfirmed,
                        ConfirmPhone=user.PhoneNumberConfirmed,
                        CreateAt=user.CreateAt,
                        Notification=user.notification,
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
            int? userId = await extractClaims.ExtractUserId(token);
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
                            Role = string.Join(",", await userManager.GetRolesAsync(user)) ,
                            CreateAt = user.CreateAt  ,
                            Notification=user.notification
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
            int? userId = await extractClaims.ExtractUserId(token);
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
                Role = string.Join(",", await userManager.GetRolesAsync(requestUser))  ,
                CreateAt =  requestUser .CreateAt  ,
                Notification=requestUser.notification  ,
                ConfirmPhone = requestUser.PhoneNumberConfirmed,
                ConfirmEmail= requestUser.EmailConfirmed,
            };
            return Ok(result);
        }

        [HttpGet("profileProvider")]
        public async Task<IActionResult> profileProvider(int Id)
        {

            ApplicationUser requestUser = await userManager.Users.
                FirstOrDefaultAsync(u => u.Id == Id);
            if (requestUser is null)
            {
                return BadRequest(new { message = "provider not found" });
            }


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
            if (resultProperty.Count < 1 && resultService.Count < 1)
            {
                return NotFound("No properties or services found for the given user ID.");
            }
            ProviderDTOs result = new ProviderDTOs
            {
                PropertyDTOs = resultProperty?.Select(RP => new GetAllPropertyDTOs
                {
                    Id = RP.Id,
                    AddressName = RP.Address.Name,
                    Description = RP.Description,
                    StartAt = RP.StartAt,
                    updateAt = RP.updateAt,
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
                Role = string.Join(",", await userManager.GetRolesAsync(requestUser))   ,
                CreateAt = requestUser.CreateAt,
                Notification=requestUser.notification
            };
            return Ok(result);
        }
        [HttpGet("count")]
        public async Task<IActionResult> count()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = await extractClaims.ExtractUserId(token);
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
                    if (result.Contains("admin"))
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
                IEnumerable<AdvertisementProject> advertisements = await dbContext.advertisements
                    .Include(adv => adv.service)
                    .Include(adv => adv.property)
                    .ToListAsync();
                foreach (var item in advertisements)
                {
                    if (item.service != null)
                    {
                        advertisementsService += 1;
                    }
                    if (item.property != null)
                    {
                        advertisementsProperty += 1;
                    }
                }

                int service = await dbContext.services.CountAsync();
                int property = await dbContext.properties.CountAsync();
                int typeService = await dbContext.typeServices.CountAsync();
                int typeProperty = await dbContext.typeProperties.CountAsync();
                int complaint = await dbContext.complaints.CountAsync();

                CountDTOs count = new CountDTOs()
                {
                    CountAdmin = userAdmin,
                    CountProvider = userProvider,
                    CountConsumer = userConsumer,
                    CountUser = allUser.Count(),
                    AdvertisementsCount = advertisements.Count(),
                    AdvertisementsService = advertisementsService,
                    AdvertisementsProperty = advertisementsProperty,
                    AllProperty = property,
                    AllService = service,
                    TypeProperty = typeProperty,
                    TypeService = typeService,
                    Complaint = complaint,

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
            int? userId = await extractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            IList<string> role = await userManager.GetRolesAsync(requestUser);

            if (role.Contains("admin"))
            {
                List<CountAddressDTOs> countAddressToUser = new List<CountAddressDTOs>();
                List<CountAddressDTOs> countAddressToProperty = new List<CountAddressDTOs>();
                List<CountAddressDTOs> countAddressToAddress = new List<CountAddressDTOs>();


                IEnumerable<AddressToProject> address = await dbContext.addresses.ToListAsync();
                int countAddress = address.Count();
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
                    var Service = await dbContext.services.Where(s => s.AddressId == item.Id).CountAsync();
                    var countAddressService = new CountAddressDTOs()
                    {
                        NameAddress = item.Name,
                        CountAddress = Service
                    };
                    countAddressToAddress.Add(countAddressService);
                }
                return Ok(new { message = "All Address", countAddress, message1 = "user", countAddressToUser, message2 = "property", countAddressToProperty, message3 = "service", countAddressToAddress });
            }
            return Unauthorized(new { message = "Only admin  can get all user" });
        }
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = await extractClaims.ExtractUserId(token);
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
                if (resultProperty.Count > 1 && resultService.Count > 1)
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
                int? userId = await extractClaims.ExtractUserId(token);
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
            int? userId = await extractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (requestUser is not null)
            {
                var newPhone=await userManager.Users.Where(u=>u.PhoneNumber==phone).FirstOrDefaultAsync();
                if (newPhone is  null)
                {
                    requestUser.PhoneNumber = phone;
                    if (requestUser.EmailConfirmed)
                        requestUser.EmailConfirmed = false;
                    if (requestUser.PhoneNumberConfirmed)
                        requestUser.PhoneNumberConfirmed=false;
                    string code = new Random().Next(100000, 999999).ToString();
                    requestUser.ConfirmationCode = code;
                    requestUser.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(requestUser);
                    var returnWhatsapp = await WhatsAppService.SendMessageAsync(requestUser.PhoneNumber,
                           $"📞 *تأكيد رقم الهاتف*\r\n\r\n" +
                           $"مرحبًا {requestUser.UserName}، وشكرًا لتسجيلك معنا! 🎉\r\n\r\n" +
                           $"🔐 *رمز التأكيد الخاص بك:* {requestUser.ConfirmationCode}\r\n\r\n" +
                           $"⏳ *الرمز صالح لمدة 20 دقيقة فقط.*\r\n\r\n" +
                           $"⚠️ إذا لم تطلب هذا الرمز، يمكنك تجاهل هذه الرسالة."
                        );
                    return Ok(new { status = 200, message = "update successful" });

                }
                return BadRequest(new { status = 400, message = "Phone number already exists" });

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
            int? userId = await extractClaims.ExtractUserId(token);
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
        public async Task<IActionResult> SendEmail(SendEmailDTOs request, bool? whatsApp)
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
                                if (whatsApp is true)
                                {
                                    Body = $"{request.Body}\n\n";
                                    var returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber, Body, imageName);
                                    return Ok(new { returnWhatsapp });
                                }
                                else
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
                                if (whatsApp is true)
                                {
                                    Body = $"{request.Body}";
                                    var returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber, Body);
                                    return Ok(new { returnWhatsapp });

                                }
                                else
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
                    else
                    {
                        IEnumerable<ApplicationUser> allUser = await dbContext.users.ToListAsync();
                        if (allUser.Any())
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
                                if (whatsApp is true)
                                {
                                    Body = $"{request.Body}\n\n";
                                    var returnWhatsapp = "";
                                    foreach (var user in allUser)
                                    {
                                        if (user.PhoneNumber.Length < 11)
                                            continue;
                                        returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber, Body, imageName);
                                    }
                                    return Ok(new { returnWhatsapp });
                                }
                                else
                                    Body = $@"
                                    <html>
                                        <body>
                                            <img src='{imageName}' alt='Embedded Image' style='max-width: 50%; height: auto; display: block; margin: auto;'/>
                                            {request.Body}
                                        </body>
                                    </html> ";

                            }
                            if (Body == "")
                            {
                                if (whatsApp is true)
                                {
                                    Body = $"{request.Body}";
                                    var returnWhatsapp = "";
                                    foreach (var user in allUser)
                                    {
                                        if (user.PhoneNumber.Length<11)
                                            continue;

                                        returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber, Body);
                                    }
                                    return Ok(new { returnWhatsapp });

                                }
                                else
                                    Body = $@"
                                    <html>
                                    <body>
                                        {request.Body}
                                   
                                    </body>
                                    </html>";
                            }
                            foreach (var user in allUser)
                            {
                                if (user.Email is null)
                                    continue;
                                
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
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (!userId.HasValue || userId <= 0)
                return Unauthorized(new { message = "Invalid Token" });

            ApplicationUser sender = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            ApplicationUser receiver = await userManager.Users.FirstOrDefaultAsync(u => u.Id == request.ReceiverId);
            if (sender is null || receiver is null)
                return NotFound(new { message = "User not found" });

            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo palestineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime palestineTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, palestineTimeZone);

            ChatMessage message = new ChatMessage
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Message = request.Message,
                Timestamp = palestineTime
            };

            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync();

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
            string roomName = sender.Id < receiver.Id ? $"Chat_{sender.Id}{receiver.Id}" : $"Chat{receiver.Id}_{sender.Id}";

            await hubContext.Clients.Group(roomName).SendAsync("ReceiveMessage", new
            {
                SenderId = sender.Id,
                Message = request.Message,
                Timestamp = palestineTime,
                MessageId = message.Id
            });

            await hubContext.Clients.User(sender.Id.ToString()).SendAsync("MessageSent", message.Id);
            await hubContext.Clients.User(receiver.Id.ToString()).SendAsync("UpdateChatList");

            var lastMessage = await dbContext.Messages
                .Where(m => (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                            (m.SenderId == receiver.Id && m.ReceiverId == sender.Id))
                .OrderByDescending(m => m.Timestamp)  
                .Select(m => new
                {
                    m.Timestamp,
                })
                .FirstOrDefaultAsync();

            if (lastMessage is not null&& lastMessage.Timestamp.AddMinutes(59)>palestineTime)
            {
                if (receiver.EmailConfirmed is false && receiver.PhoneNumberConfirmed is false)
                    return BadRequest(new
                    {
                        message = "not validation"
                    });
                if (receiver.notification is true)
                {

                    if(receiver.Email is null)
                    {
                        await WhatsAppService.SendMessageAsync(receiver.PhoneNumber, $"""
                                📩 *رسالة جديدة على عقارتي*
        
                                مرحباً {receiver.UserName}،
                                لديك رسالة جديدة من *{sender.UserName}*:
        
                                ⏰ الوقت: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}
        
                                يمكنك الرد عبر التطبيق:
                                https://aqaraty.netlify.app/
        
                                لإيقاف الإشعارات:
                                انتقل إلى: الإعدادات → إشعارات الرسائل
        
                                مع تحيات فريق عقارتي
                                """);
                    }
                    else
                    {

                        EmailDTOs email = new EmailDTOs()
                        {
                            Subject = $"رسالة جديدة من {sender.UserName} تنتظرك!",
                            Recivers = receiver.Email,
                            Body = $@"
                                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e1e1e1; padding: 20px;'>
                                    <h2 style='color: #2c3e50;'>مرحباً {receiver.UserName},</h2>
                                    <p style='font-size: 16px;'>
                                        لديك رسالة جديدة من <strong>{sender.UserName}</strong> تنتظر قراءتك!
                                    </p>
                                    <p style='font-size: 14px; color: #7f8c8d;'>
                                        ⏰ آخر نشاط: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}
                                    </p>
                                    <a href='https://aqaraty.netlify.app/' 
                                       style='background-color: #3498db; color: white; padding: 10px 20px; 
                                              text-decoration: none; border-radius: 5px; display: inline-block;'>
                                         زيارة الموقع
                                    </a>
                                    <p style='font-size: 12px; color: #95a5a6; margin-top: 20px;'>
                                        يمكنك إيقاف هذه الإشعارات من إعدادات حسابك.
                                    </p>
                                </div>"
                         };

                        EmailSetting.SendEmail(email);
                    }
                }
               
            }

            return Ok(new
            {
                Message = "Message sent successfully.",
                Timestamp = palestineTime,
                MessageId = message.Id
            });
        }

        [HttpGet("HistoryMessage")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? currentUserId = await extractClaims.ExtractUserId(token);
            if (!currentUserId.HasValue || currentUserId <= 0)
                return Unauthorized(new { message = "Invalid Token" });

            ApplicationUser sender = await userManager.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            ApplicationUser receiver = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (sender is null || receiver is null)
                return NotFound(new { message = "User not found" });
            var messages = await dbContext.Messages
                .Where(m => (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                           (m.SenderId == receiver.Id && m.ReceiverId == sender.Id))
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Message,
                    m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId
                })
                .ToListAsync();

            var result = messages.Select(m => new HistoryMessageDTOs
            {
                Message = m.Message,
                Timestamp = m.Timestamp,
                SenderName = userManager.Users.Where(u => u.Id == m.SenderId).Select(u => u.UserName).FirstOrDefault(),
                ReceiverName = userManager.Users.Where(u => u.Id == m.ReceiverId).Select(u => u.UserName).FirstOrDefault()
            }).ToList();

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
            string roomName = currentUserId < userId ? $"Chat_{currentUserId}{userId}" : $"Chat{userId}_{currentUserId}";

            await hubContext.Clients.Group(roomName).SendAsync("ChatHistoryUpdated", new
            {
                Messages = result
            });

            return Ok(new
            {
                Messages = result,
                RoomName = roomName
            });
        }
        [HttpGet("ListMessage")]
        public async Task<IActionResult> ListMessage()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (!userId.HasValue || userId <= 0)
                return Unauthorized(new { message = "Invalid Token" });

            ApplicationUser currentUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser is null)
                return NotFound(new { message = "User not found" });

            var lastMessages = await dbContext.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .GroupBy(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()
                })
                .ToListAsync();

            var result = new List<MessageSummaryDTO>();
            foreach (var item in lastMessages)
            {
                var otherUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == item.OtherUserId);
                if (otherUser != null)
                {
                    result.Add(new MessageSummaryDTO
                    {
                        UserId = otherUser.Id,
                        UserName = otherUser.UserName,
                        LastMessage = item.LastMessage.Message,
                        LastMessageTime = item.LastMessage.Timestamp,
                        IsSender = item.LastMessage.SenderId == currentUser.Id
                    });
                }
            }

            var orderedResult = result.OrderByDescending(r => r.LastMessageTime).ToList();
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
            await hubContext.Clients.User(userId.ToString()).SendAsync("UpdateChatList");

            return Ok(orderedResult);
        }
        [HttpPost("notification")]
        public async Task<IActionResult> notification()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });

            int? userId = await extractClaims.ExtractUserId(token);
            if (!userId.HasValue || userId <= 0)
                return Unauthorized(new { message = "Invalid Token" });
            var user = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found" });
            user.notification=!user.notification;
            await dbContext.SaveChangesAsync();
            return Ok(new
            {
                Success = true,
                Message = "Notification status updated successfully",
                NotificationEnabled = user.notification
            });

        }

        [HttpGet("HelpSearch")]
        public async Task<IActionResult> HelpSearch(string Search)
        {

            if (string.IsNullOrWhiteSpace(Search))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Search term cannot be empty"
                });
            }

            var firstWord = Search.Split(' ')[0].ToLower();

            var serviceQuery = dbContext.services
                .Include(s => s.ImageDetails)
                .Include(s => s.Reviews)
                .Include(s => s.Address)
                .Include(s => s.Type)
                .Include(s => s.User)
                .Where(s =>
                    s.User.UserName.ToLower().StartsWith(firstWord) ||
                    s.Address.Name.ToLower().StartsWith(firstWord) ||
                    s.Type.Name.ToLower().StartsWith(firstWord))
                .AsQueryable();

            var propertyQuery = dbContext.properties
                .Include(p => p.ImageDetails)
                .Include(p => p.Reviews)
                .Include(p => p.Address)
                .Include(p => p.Type)
                .Include(p => p.User)
                .Where(p =>
                    p.User.UserName.ToLower().StartsWith(firstWord) ||
                    p.Address.Name.ToLower().StartsWith(firstWord) ||
                    p.Type.Name.ToLower().StartsWith(firstWord))
                .AsQueryable();
            var services = await serviceQuery.ToListAsync();
            var serviceDTOs = services.Select(item => new GetAllServiceDTOs
            {
                UserName = item.User.UserName,
                AddressName = item.Address.Name,
                TypeName = item.Type.Name,
                PriceRange = item.PriceRange,
                Description = item.Description,
                Id = item.Id,
                userId = item.User.Id,
                ImageDetails = item.ImageDetails.Select(img => new GetImageDTOs
                {
                    Id = img.Id,
                    Name = img.Image
                }).ToList(),
                Reviews = item.Reviews.Select(r => new GetAllReviewDTOs
                {
                    Id = r.Id,
                    description = r.Description,
                    date = r.CreateAt,
                    rating = r.Rating,
                    UserId = r.UsersID,
                }).ToList(),
                AvgRating = item.Reviews.Any() ? item.Reviews.Average(r => r.Rating) : 0
            }).ToList();

            var distinctServices = serviceDTOs
                .GroupBy(s => new { s.TypeName, s.AddressName, s.PriceRange })
                .Select(g => g.First())
                .ToList();

            var properties = await propertyQuery.ToListAsync();
            var propertyDTOs = properties.Select(item => new GetAllPropertyDTOs
            {
                Id = item.Id,
                UserID = item.UsersID,
                Description = item.Description,
                TypeName = item.Type.Name,
                StartAt = item.StartAt,
                updateAt = item.updateAt,
                Price = item.Price,
                AddressName = item.Address.Name,
                userName = item.User.UserName,
                ImageDetails = item.ImageDetails.Select(img => new GetImageDTOs
                {
                    Id = img.Id,
                    Name = img.Image
                }).ToList(),
                Reviews = item.Reviews.Select(r => new GetAllReviewDTOs
                {
                    Id = r.Id,
                    UserId = r.UsersID,
                    description = r.Description,
                    date = r.CreateAt,
                    rating = r.Rating,
                }).ToList(),
                AvgRating = item.Reviews.Any() ? item.Reviews.Average(r => r.Rating) : 0
            }).ToList();
            var distinctProperty = propertyDTOs
                .GroupBy(s => new { s.TypeName, s.AddressName, s.Price })
                .Select(g => g.First())
                .ToList();

            bool hasServices = distinctServices.Count > 0;
            bool hasProperties = distinctProperty.Count > 0;
            if (!hasServices && !hasProperties)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No services or properties found matching the specified criteria"
                });
            }
            return Ok(new
            {
                Success = true,
                Message = "Search results retrieved successfully",
                Data = new
                {
                    Services = hasServices ? new
                    {
                        Count = distinctServices.Count,
                        Items = distinctServices
                    } : null,
                    Properties = hasProperties ? new
                    {
                        Count = distinctProperty.Count,
                        Items = distinctProperty
                    } : null
                },
                Summary = new
                {
                    TotalServices = hasServices ? distinctServices.Count : 0,
                    TotalProperties = hasProperties ? distinctProperty.Count : 0,
                    TotalResults = (hasServices ? distinctServices.Count : 0) +
                                                 (hasProperties ? distinctProperty.Count : 0)
                }
            });
        }

    }
}
