using Graduation.Data;
using Graduation.Data.Migrations;
using Graduation.DTOs.TypeToProject;
using Graduation.Model;
using Graduation.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Controllers.Address
{
    [Route("[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public AddressController(ApplicationDbContext dbContext,UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpPost("AddAddress")]
        public async Task<IActionResult> AddAddress(string name)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
            var role = await userManager.GetRolesAsync(requestUser);
            if (role.Contains("admin"))
            {
                AddressToProject address = new AddressToProject
                {
                    Name = name,
                };
                var type = await dbContext.addresses.AddAsync(address);
                await dbContext.SaveChangesAsync();
                return Ok(new { status = 200, message = "add address successful" });
            }
            return Unauthorized();
        }

        
        [HttpGet("GetAddress")]
        public async Task<IActionResult> GetAddress()
        {
            IEnumerable<AddressToProject> result = await dbContext.addresses.ToListAsync();
            IEnumerable<GetTypeDTOs> typeService = result.Adapt<IEnumerable<GetTypeDTOs>>();
            return Ok(new { status = 200, typeService });
        }

        //[HttpDelete("DeleteAddress")]
        //public async Task<IActionResult> DeleteAddress(int  addressId)
        //{
        //    string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
        //    if (string.IsNullOrEmpty(token))
        //        return Unauthorized(new { message = "Token Is Missing" });
        //    int? userId = ExtractClaims.ExtractUserId(token);
        //    if (string.IsNullOrEmpty(userId.ToString()))
        //        return Unauthorized(new { message = "Token Is Missing" });
        //    ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);
        //    var role = await userManager.GetRolesAsync(requestUser);
        //    if (role.Contains("admin"))
        //    {
        //        AddressToProject address=await dbContext.addresses.FirstOrDefaultAsync(a=>a.Id == addressId);
        //         dbContext.addresses.RemoveRange(address);
        //        await dbContext.SaveChangesAsync();
        //        return Ok(new { status = 200, message = "delete address successful" });
        //    }
        //    return Unauthorized();
        //}

    }
}
