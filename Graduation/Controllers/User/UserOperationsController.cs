using Graduation.Data;
using Graduation.DTOs.Auth;
using Graduation.Model;
using Graduation.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers.User
{
    [Route("[controller]")]
    [ApiController]
    public class UserOperationsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public UserOperationsController(UserManager<ApplicationUser> userManager,ApplicationDbContext dbContext)
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
                IEnumerable<ApplicationUser> GetUsers = await userManager.Users.ToListAsync();
                foreach (ApplicationUser user in GetUsers)
                {
                    AllUser.Add(new AuthGetAllUserDTOs
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        Address = user.Address,
                        Role = string.Join(",", await userManager.GetRolesAsync(user))
                    });
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
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            AuthGetAllUserDTOs result = new AuthGetAllUserDTOs
            {
                Id = requestUser.Id,
                UserName = requestUser.UserName,
                Email = requestUser.Email,
                Phone = requestUser.PhoneNumber,
                Address = requestUser.PhoneNumber,
                Role = string.Join(",", await userManager.GetRolesAsync(requestUser))
            };
            return Ok(result );
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
            IList<string> role =await userManager.GetRolesAsync(requestUser);
            if (role.Contains("admin"))
            {
                ApplicationUser User = await userManager.Users.FirstOrDefaultAsync(u=>u.Id==Id);
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
                return BadRequest(new {message="user not found"});
           
        }



        [HttpPut("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress(string address)
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
                requestUser.Address = address;
                await userManager.UpdateAsync(requestUser);

                return Ok(new { status = 200, message = "update successful" });
            }

               ;
            return BadRequest(new { message = "user not found" });

        }
    }
}
