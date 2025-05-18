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
using System.Numerics;
using Azure.Core;
using System.Runtime.CompilerServices;

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
        private readonly ExtractClaims extractClaims;

        public AuthController(
            ApplicationDbContext dbContext, AuthServices authServices,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ExtractClaims extractClaims)
        {
            this.dbContext = dbContext;
            this.authServices = authServices;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.extractClaims = extractClaims;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(AuthRegisterDTOs request, bool? WhatsApp)
        {
            if (ModelState.IsValid)
            {

              
                if (request.role == "admin")
                {
                    return BadRequest(new { message = "you cannot create an account Admin" });

                }
                if (request.Email is not null || WhatsApp is false)
                {

                    ApplicationUser applicationEmail = await userManager.FindByEmailAsync(request.Email);

                    if (applicationEmail is not null)
                    {
                        if (applicationEmail.EmailConfirmed == false&& applicationEmail.PhoneNumberConfirmed == false)
                        {
                            await userManager.DeleteAsync(applicationEmail);
                        }
                        else
                        {
                            return BadRequest(new { message = "Email already registered" });
                        }
                    }
                   
                }

                ApplicationUser applicationPhone = await userManager.Users.Where(user => user.PhoneNumber == request.Phone).FirstOrDefaultAsync();
                if (applicationPhone is not null)
                {
                    if (applicationPhone.EmailConfirmed == false && applicationPhone.PhoneNumberConfirmed == false)
                    {
                        await userManager.DeleteAsync(applicationPhone);
                    }
                    else
                    {
                        return BadRequest(new { message = "Phone number already registered" });
                    }
                }

                ApplicationUser applicationName = await userManager.FindByNameAsync(request.Name);
                if (applicationName is not null)
                {

                    if (applicationName.EmailConfirmed == false&&applicationName.PhoneNumberConfirmed==false)
                    {
                        await userManager.DeleteAsync(applicationName);
                    }
                    else
                    {
                        return BadRequest(new { message = "Username already registered" });
                    }
                }

                ApplicationUser user = new ApplicationUser();
                if (request.Email is not null)
                {

                    user = new ApplicationUser()
                    {
                        Email = request.Email,
                        UserName = request.Name,
                        PhoneNumber = request.Phone,
                        AddressId = request.addressId    ,
                        CreateAt = DateTime.Now,
                    };
                }
                else
                {
                    user = new ApplicationUser()
                    {
                        UserName = request.Name,
                        PhoneNumber = request.Phone,
                        AddressId = request.addressId   ,
                        CreateAt= DateTime.Now,
                    };
                }
                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                   
                    string code = new Random().Next(100000, 999999).ToString();
                    user.ConfirmationCode = code;
                    user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(user);
                    IdentityResult resultRole = await userManager.AddToRoleAsync(user, request.role);
                    if (request.Email is null ||( WhatsApp is true&& !string.IsNullOrEmpty(request.Phone)))
                    {
                        var returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber,
                           $"📞 *تأكيد رقم الهاتف*\r\n\r\n" +
                           $"مرحبًا {user.UserName}، وشكرًا لتسجيلك معنا! 🎉\r\n\r\n" +
                           $"🔐 *رمز التأكيد الخاص بك:* {user.ConfirmationCode}\r\n\r\n" +
                           $"⏳ *الرمز صالح لمدة 20 دقيقة فقط.*\r\n\r\n" +
                           $"⚠️ إذا لم تطلب هذا الرمز، يمكنك تجاهل هذه الرسالة."
                        );
                        return Ok(new { returnWhatsapp });
                    }
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
                    return Ok(new { message = $"User registered successfully. Confirmation email sent." });


                }
                List<string> errorMessage = result.Errors.Select(error => error.Description).ToList();
                return BadRequest(string.Join(", ", errorMessage));
            }
            return NotFound(ModelState);

        }
        [HttpPost("Confirm")]
        public async Task<IActionResult> Confirm(string? email, string? name, string? phone, string code)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                if (!string.IsNullOrEmpty(email))
                {
                    user = await userManager.FindByEmailAsync(email);

                }
                else if (!string.IsNullOrEmpty(name))
                {
                    user = await userManager.FindByNameAsync(name);
                }
                else if (!string.IsNullOrEmpty(phone))
                {
                    user = await userManager.Users.Where(u => u.PhoneNumber == phone).FirstOrDefaultAsync();
                }
                else
                {
                    return BadRequest(new { message = "enter username or email or phone" });
                }
                if (user is not null && (user.Email == email || user.UserName == name || user.PhoneNumber == phone))
                {
                    if (user.ConfirmationCode == code)
                    {
                        if (user.ConfirmationCodeExpiry >= DateTime.Today.Add(DateTime.Now.TimeOfDay))
                        {
                            
                                 if(!String.IsNullOrEmpty(email))

                                    user.EmailConfirmed = true;
                                  else
                                      user.PhoneNumberConfirmed = true;
                            await userManager.UpdateAsync(user);
                            return !String.IsNullOrEmpty(email)? Ok(new { message = "success confirm email" }): Ok(new { message = "success confirm phone" });
                        }

                        return BadRequest(new { message = "the code is finished" });

                    }
                    return BadRequest(new { message = "invalid code" });
                }
                return BadRequest(new { message = "user not found" });
            }
            return NotFound(ModelState);
        }
        [HttpPost("RestCode")]
        public async Task<IActionResult> RestCode(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                ApplicationUser user = new ApplicationUser();
                
                 if (!string.IsNullOrEmpty(name))
                {
                    user = await userManager.FindByNameAsync(name);
                }
                else
                    return BadRequest(new { message = "enter username " });
                 if(user is not null)
                {
                    string code = new Random().Next(100000, 999999).ToString();
                    user.ConfirmationCode = code;
                    user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(user);
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        var returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber,
                            $"📞 *تأكيد رقم الهاتف*\r\n\r\n" +
                            $"مرحبًا {user.UserName}، شكرًا لتسجيلك معنا! 🎉\r\n\r\n" +
                            $"🔐 *رمز التأكيد الخاص بك:* {user.ConfirmationCode}\r\n\r\n" +
                            $"⏳ *الرمز صالح لمدة 20 دقيقة فقط.*\r\n\r\n" +
                            $"⚠️ إذا لم تطلب هذا الرمز، يمكنك تجاهل هذه الرسالة."
                        );
                        return Ok(new { returnWhatsapp });
                    }
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
                    return Ok(new { message = $"rest code successful" });
                }
                   return BadRequest(new {message ="user not found"});
                
            }
            return NotFound(new {message="enter name"});
        }
        [HttpPut("ConfirmStatus")]
        public async Task<IActionResult> ConfirmStatus(int Id)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = await extractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser resultUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (resultUser is null)
                    return NotFound(new { message = "User not found" });
                var role = await userManager.GetRolesAsync(resultUser);
                if (role.Contains("admin"))
                {
                    ApplicationUser user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == Id);
                    if (user is null)
                        return BadRequest(new
                        {
                            message = "user not found"
                        });
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        user.PhoneNumberConfirmed = !user.PhoneNumberConfirmed;
                    }
                    else
                    {
                        user.EmailConfirmed = !user.EmailConfirmed;
                    }

                    var result = await userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Failed to update user status", errors = result.Errors });
                    }

                    bool isPhoneConfirmation = !string.IsNullOrEmpty(user.PhoneNumber);
                    bool isConfirmed = isPhoneConfirmation ? user.PhoneNumberConfirmed : user.EmailConfirmed;

                    return Ok(new
                    {
                        message = isConfirmed ? "Successfully confirmed" : "Successfully unconfirmed",
                        field = isPhoneConfirmation ? "phone" : "email",
                        currentStatus = isConfirmed
                    });
                }
            }
            return NotFound(ModelState);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthLoginDTOs request)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user = await userManager.FindByEmailAsync(request.Email);

                }
                else if (!string.IsNullOrEmpty(request.Name))
                {
                    user = await userManager.FindByNameAsync(request.Name);
                }
                else if (!string.IsNullOrEmpty(request.Phone))
                {
                    user = await userManager.Users.Where(u => u.PhoneNumber == request.Phone).FirstOrDefaultAsync();
                }
                else
                {
                    return BadRequest(new { message = "enter username or email or phone" });
                }
                if (user is not null)
                {
                    if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
                    {
                        return BadRequest(new
                        {
                            message = "Verification required",
                            detail = "Please confirm your email and phone number",
                        });
                    } 
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
                        string resultToken = await authServices.CreateTokenAsync(user, userManager);
                        return Ok(new { status = 200, userId = user.Id, name = user.UserName, role = resltRole, token = resultToken });
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
            int? userId = await extractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });

            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByIdAsync(userId.ToString());
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
            int? userId = await extractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(request.Email);

                if (user is not null)
                {
                    if (userId == user.Id)
                    {

                        if (user.EmailConfirmed == false)
                        {
                            return BadRequest(new { message = "email not found" });
                        }
                        string code = new Random().Next(100000, 999999).ToString();
                        user.Email = request.NewEmail;
                        IdentityResult updateUser = await userManager.UpdateAsync(user);
                        if (!updateUser.Succeeded)
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
        public async Task<IActionResult> GenerateRestPassword(string? email, string? name, string? phone,bool? whatsApp)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                if (!string.IsNullOrEmpty(email))
                {
                    user = await userManager.FindByEmailAsync(email);

                }
                else if (!string.IsNullOrEmpty(name))
                {
                    user = await userManager.FindByNameAsync(name);
                }
                else if (!string.IsNullOrEmpty(phone))
                {
                    user = await userManager.Users.Where(u => u.PhoneNumber == phone).FirstOrDefaultAsync();
                }
                else
                {
                    return BadRequest(new { message = "enter username or email or phone" });
                }
                if (user is not null)
                {
                    string code = new Random().Next(100000, 999999).ToString();

                    user.ConfirmationCode = code;
                    user.ConfirmationCodeExpiry = DateTime.Today.Add(DateTime.Now.TimeOfDay).AddMinutes(20);
                    await userManager.UpdateAsync(user);
                    if (string.IsNullOrEmpty(user.Email) || phone is not null||whatsApp is true)
                    {
                        var returnWhatsapp = await WhatsAppService.SendMessageAsync(user.PhoneNumber,
                            $"🔐 *إعادة تعيين كلمة المرور*\r\n\r\n" +
                            $"مرحبًا { user.UserName}، لقد طلبت رمزًا لإعادة تعيين كلمة المرور الخاصة بك.\r\n\r\n" +
                            $"📌 *رمز التأكيد الخاص بك:* {user.ConfirmationCode}\r\n\r\n" +
                            $"⏳ *الرمز صالح لمدة 20 دقيقة فقط.*\r\n\r\n" +
                            $"⚠️ إذا لم تطلب هذا الرمز، يرجى تجاهل هذه الرسالة."
                        );
                        return Ok(new { returnWhatsapp });
                    }
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
                ApplicationUser user = new ApplicationUser();
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user = await userManager.FindByEmailAsync(request.Email);

                }
                else if (!string.IsNullOrEmpty(request.Name))
                {
                    user = await userManager.FindByNameAsync(request.Name);
                }
                else if (!string.IsNullOrEmpty(request.Phone))
                {
                    user = await userManager.Users.Where(u => u.PhoneNumber == request.Phone).FirstOrDefaultAsync();
                }
                else
                {
                    return BadRequest(new { message = "enter username or email or phone" });
                }
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
