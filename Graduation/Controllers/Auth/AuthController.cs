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
                ApplicationUser user = new ApplicationUser()
                {
                    Email = request.Email,
                    UserName = request.Name,
                    PhoneNumber = request.Phone,
                    Address = request.Address
                };
                IdentityResult result = await userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    if (request.role == "admin")
                    {
                        return BadRequest(new {message="you cannot create an account Admin"});
                    }
                    var resultRole = await userManager.AddToRoleAsync(user, request.role);
                    string token = await authServices.CreateTokenasync(user, userManager);
                    string confirmEmail = Url.Action("ConfirmEmail", "Auth", new { email = user.Email, token = token }, protocol: HttpContext.Request.Scheme);

                    EmailDTOs emailDTOs = new EmailDTOs()
                    {
                        Subject = "Confirm Email",
                        Recivers = user.Email,
                        Body = confirmEmail
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
        public async Task<IActionResult> ConfirmEmail(string email)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });

                ApplicationUser user = await userManager.FindByIdAsync(userId.ToString());
                if (user is not null && user.Email == email)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                    return Created();
                }
                return BadRequest(new { message = "user not found" });
            }
            return NotFound(ModelState);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthLoginDTOs request)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(request.Email);
                if (user is not null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, request.Password, true, true);
                    if (result.Succeeded)
                    {
                        string token = await authServices.CreateTokenasync(user, userManager);
                        return Ok(new { status = 200, message = token });
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
        public async Task<IActionResult> ChangePassword(AuthChangeEmailDTOs request)
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
                        user.Email = request.NewEmail;
                        user.EmailConfirmed = false;

                        await userManager.UpdateAsync(user);
                        string newtoken = await authServices.CreateTokenasync(user, userManager);
                        string confirmEmail = Url.Action("ConfirmEmail", "Auth", new { email = user.Email, token = newtoken }, protocol: HttpContext.Request.Scheme);

                        EmailDTOs emailDTOs = new EmailDTOs()
                        {
                            Subject = "Confirm Email",
                            Recivers = user.Email,
                            Body = confirmEmail
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
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string confirmEmail = Url.Action("ResetPassword", "Auth", new { email = user.Email, token = token }, protocol: HttpContext.Request.Scheme);

                    EmailDTOs emailDTOs = new EmailDTOs()
                    {
                        Subject = "Confirm Email",
                        Recivers = user.Email,
                        Body = confirmEmail
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
                    var decodedToken = HttpUtility.UrlDecode(request.Token);
                    var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
                    if (result.Succeeded)
                    {
                        return Ok(new { message = "change password successful" });
                    }
                    List<string> errorMessage = result.Errors.Select(error => error.Description).ToList();
                    return BadRequest(new { message = "change password failed" + string.Join(", ", errorMessage) });
                }
                return BadRequest(new { message = "user not found " });
            }

            return NotFound(ModelState);

        }
        
    }
}
