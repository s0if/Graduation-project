using Graduation.Data;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Graduation.Model;
using Graduation.DTOs.Auth;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Graduation.DTOs.Email;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Graduation.DTOs.TypeToProject;
using System.Collections.Generic;
using Microsoft.Extensions.Azure;

namespace Graduation.Controllers.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly AuthServices authServices;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthController(
            ApplicationDbContext dbContext, AuthServices authServices,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.dbContext = dbContext;
            this.authServices = authServices;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(AuthRegisterDTOs request)
        {
            if (ModelState.IsValid)
            {
               ApplicationUser application=await userManager.FindByEmailAsync(request.Email);
                if(application is not null)
                {
                    if (application.EmailConfirmed == false)
                    {
                        await userManager.DeleteAsync(application);
                    }
                }
                
                ApplicationUser user = new ApplicationUser()
                {
                    Email = request.Email,
                    UserName = request.Name,
                    PhoneNumber = request.Phone,
                    AddressId = request.addressId
                };
                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    if (request.role == "admin")
                    {
                        await userManager.DeleteAsync(user);
                        return BadRequest(new {message="you cannot create an account Admin"});

                    }
                   string code = new Random().Next(100000, 999999).ToString();
                    user.ConfirmationCode = code;
                    user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(user);
                    IdentityResult resultRole = await userManager.AddToRoleAsync(user, request.role);
                    string htmlBody = $@"
                        <!DOCTYPE html>
                        <html dir='rtl' lang='ar'>
                        <head>
                            <meta charset='UTF-8'>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    margin: 0;
                                    padding: 0;
                                    text-align: right;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    padding: 20px;
                                    background-color: #ffffff;
                                    border-radius: 8px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                    text-align: right;
                                }}
                                .header {{
                                    font-size: 24px;
                                    color: #333333;
                                    margin-bottom: 20px;
                                    text-align: right;
                                }}
                                .code {{
                                    font-size: 28px;
                                    font-weight: bold;
                                    color: #007BFF;
                                    margin: 20px 0;
                                    padding: 10px;
                                    background-color: #f8f9fa;
                                    border-radius: 4px;
                                    text-align: center;
                                }}
                                .footer {{
                                    margin-top: 20px;
                                    font-size: 14px;
                                    color: #666666;
                                    text-align: right;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>تأكيد البريد الإلكتروني</div>
                                <p>شكرًا لتسجيلك. يرجى استخدام الكود التالي لتأكيد عنوان بريدك الإلكتروني:</p>
                                <div class='code'>{code}</div>
                                <p>هذا الكود سينتهي خلال 20 دقيقة.</p>
                                <div class='footer'>
                                    إذا لم تطلب هذا الكود، يرجى تجاهل هذه الرسالة.
                                </div>
                            </div>
                        </body>
                        </html>";
                    EmailDTOs emailDTOs = new EmailDTOs()
                    {
                        Subject = "Confirm Email",
                        Recivers = user.Email,
                        Body = htmlBody
                    };

                    EmailSetting.SendEmail(emailDTOs);
                    return Ok("User registered successfully. Confirmation email sent.");


                }
                List<string> errorMessage = result.Errors.Select(error => error.Description).ToList();
                return BadRequest(string.Join(", ", errorMessage));
            }
            return NotFound(ModelState);

        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email,string code)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(email);
                if (user is not null && user.Email == email)
                {
                    if (user.ConfirmationCode == code )
                    {
                        if (user.ConfirmationCodeExpiry >= DateTime.Today.Add(DateTime.Now.TimeOfDay))
                        {
                            
                            user.EmailConfirmed = true;
                            await userManager.UpdateAsync(user);
                            return Ok(new { message = "success confirm" });
                        }

                        return BadRequest(new { message = "the code is finished" });

                    }
                    return BadRequest(new { message= "invalid code" });
                }
                return BadRequest(new { message = "user not found" });
            }
            return NotFound(ModelState);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthLoginDTOs request,string? email, string? token)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(request.Email);
                if (user is not null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, request.Password, true, true);
                    if (result.Succeeded)
                    {
                        var role = await userManager.GetRolesAsync(user);
                        string resltRole = "";
                        if (role.Contains("admin"))
                        {
                            resltRole = "admin";
                        }
                        else if (role.Contains("provider"))
                        {
                            resltRole = "provider";
                        }
                        else if (role.Contains("consumer"))
                        {
                            resltRole = "consumer";
                        }
                        string resultToken = await authServices.CreateTokenasync(user, userManager);
                        return Ok(new { status = 200, userId = user.Id,name = user.UserName , role = resltRole, token = resultToken });
                    }
                    if (result.IsNotAllowed)
                    {
                        return Unauthorized(new { message = "Login failed confirm email" });
                    }
                    return BadRequest(new { message = "Login failed try again" });

                }

                return BadRequest(new { message = "user not found " });

            }
            return NotFound(ModelState);
        }
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(AuthChangePasswordDTOs request)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });

            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(request.Email);
                if (user is not null)
                {
                    if (userId == user.Id)
                    {
                        var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);


                        if (result.Succeeded)
                        {
                            return Ok(new { status = 200, message = "change password successful" });
                        }
                        List<string> errorMessage = result.Errors.Select(error => error.Description).ToList();
                        return BadRequest(new { message = "change password failed\n" + string.Join(", ", errorMessage) });
                    }


                }
                return BadRequest(new { message = "user not found " });
            }
            return NotFound(ModelState);
        }
        [HttpPut("ChangeEmail")]
        public async Task<IActionResult> ChangeEmail(AuthChangeEmailDTOs request)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(request.Email);

                if (user is not null)
                {
                    if (userId == user.Id)
                    {
                     
                        if (user.EmailConfirmed==false)
                        {
                            return BadRequest(new {message="email not found"});
                        }
                        string code = new Random().Next(100000, 999999).ToString();
                        user.Email = request.NewEmail;
                        IdentityResult updateUser=await userManager.UpdateAsync(user);
                        if(!updateUser.Succeeded)
                        {
                            List<string> errorMessage = updateUser.Errors.Select(error => error.Description).ToList();
                            return BadRequest(string.Join(", ", errorMessage));
                        }
                        user.EmailConfirmed = false;
                        user.ConfirmationCode = code;
                        user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                        await userManager.UpdateAsync(user);
                        string htmlBody = $@"
                        <!DOCTYPE html>
                        <html dir='rtl' lang='ar'>
                        <head>
                            <meta charset='UTF-8'>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    background-color: #f4f4f4;
                                    margin: 0;
                                    padding: 0;
                                    text-align: right;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    padding: 20px;
                                    background-color: #ffffff;
                                    border-radius: 8px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                    text-align: right;
                                }}
                                .header {{
                                    font-size: 24px;
                                    color: #333333;
                                    margin-bottom: 20px;
                                    text-align: right;
                                }}
                                .code {{
                                    font-size: 28px;
                                    font-weight: bold;
                                    color: #007BFF;
                                    margin: 20px 0;
                                    padding: 10px;
                                    background-color: #f8f9fa;
                                    border-radius: 4px;
                                    text-align: center;
                                }}
                                .footer {{
                                    margin-top: 20px;
                                    font-size: 14px;
                                    color: #666666;
                                    text-align: right;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>تأكيد البريد الإلكتروني</div>
                                <p>شكرًا لتسجيلك. يرجى استخدام الكود التالي لتأكيد عنوان بريدك الإلكتروني:</p>
                                <div class='code'>{code}</div>
                                <p>هذا الكود سينتهي خلال 20 دقيقة.</p>
                                <div class='footer'>
                                    إذا لم تطلب هذا الكود، يرجى تجاهل هذه الرسالة.
                                </div>
                            </div>
                        </body>
                        </html>";

                        EmailDTOs emailDTOs = new EmailDTOs()
                        {
                            Subject = "Confirm Email",
                            Recivers = user.Email,
                            Body = htmlBody
                        };

                        EmailSetting.SendEmail(emailDTOs);
                        return Ok(new { status = 200, message = "change Email successful   Confirmation email sent" });
                    }


                }
                return BadRequest(new { message = "user not found " });
            }
            return NotFound(ModelState);
        }
        [HttpPost("GenerateRestPassword")]
        public async Task<IActionResult> GenerateRestPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user is not null)
                {
                    string code = new Random().Next(100000, 999999).ToString();

                    user.ConfirmationCode = code;
                    user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(user);
                       string htmlBody = $@"
                            <!DOCTYPE html>
                            <html dir='rtl' lang='ar'>
                            <head>
                                <meta charset='UTF-8'>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        background-color: #f4f4f4;
                                        margin: 0;
                                        padding: 0;
                                         text-align: right;
                                    }}
                                    .container {{
                                        max-width: 600px;
                                        margin: 20px auto;
                                        padding: 20px;
                                        background-color: #ffffff;
                                        border-radius: 8px;
                                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                         text-align: right;
                                    }}
                                    .header {{
                                        font-size: 24px;
                                        color: #333333;
                                        margin-bottom: 20px;
                                         text-align: right;
                                    }}
                                    .code {{
                                        font-size: 28px;
                                        font-weight: bold;
                                        color: #007BFF;
                                        margin: 20px 0;
                                        padding: 10px;
                                        background-color: #f8f9fa;
                                        border-radius: 4px;
                                        text-align: center;
                                    }}
                                    .footer {{
                                        margin-top: 20px;
                                        font-size: 14px;
                                        color: #666666;
                                         text-align: right;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>إعادة تعيين كلمة المرور</div>
                                    <p>مرحبًا،</p>
                                    <p>لقد تلقينا طلبًا لإعادة تعيين كلمة المرور الخاصة بحسابك.  يرجى استخدام الكود التالي لتأكيد عنوان بريدك الإلكتروني:</p>
                                    <div class='code'>{code}</div>
                                    <p>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة.</p>
                                    <div class='footer'>
                                        شكرًا لاستخدامك تطبيقنا.
                                    </div>
                                </div>
                            </body>
                            </html>";

                    EmailDTOs emailDTOs = new EmailDTOs()
                    {
                        Subject = "Confirm Email",
                        Recivers = user.Email,
                        Body = htmlBody
                    };

                    EmailSetting.SendEmail(emailDTOs);
                    return Ok("Rest Password successfully. Confirmation email sent.");

                }

                return BadRequest(new { message = "user not found " });
            }
            return NotFound(ModelState);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(AuthRestPasswordDTOs request)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user is not null)
                {
                    if (request.code == user.ConfirmationCode)
                    {
                        if (user.ConfirmationCodeExpiry >= DateTime.Today.Add(DateTime.Now.TimeOfDay))
                        {
                            string token = await userManager.GeneratePasswordResetTokenAsync(user);
                            IdentityResult result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
                            if (result.Succeeded)
                            {
                                return Ok(new { message = "change password successful" });
                            }
                            List<string> errorMessage = result.Errors.Select(error => error.Description).ToList();
                            return BadRequest(new { message = "change password failed" + string.Join(", ", errorMessage) });

                        }
                        return BadRequest(new { message = "the code is finished" });
                    }
                    return BadRequest(new { message = "code error" });
                  
                }
                return BadRequest(new { message = "user not found " });
            }

            return NotFound(ModelState);

        }
        
    }
}
