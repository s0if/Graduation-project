using Azure.Storage.Blobs.Models;
using Graduation.Model;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Graduation.Service
{
    public class ExtractClaims
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ExtractClaims(UserManager<ApplicationUser>userManager)
        {
            this.userManager = userManager;
        }

        public async Task<int?> ExtractUserId(string Token)
        {
            
            JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = TokenHandler.ReadJwtToken(Token);

            if (jwtToken.ValidTo < DateTime.UtcNow)

                return null;
           

            Claim userIdClaim = jwtToken.Claims.FirstOrDefault(type => type.Type == ClaimTypes.NameIdentifier);
            string? tokenIdClim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TokenId")?.Value;
            if (userIdClaim is null|| tokenIdClim is null)
           
                return null;
               if(! int.TryParse(userIdClaim.Value, out int userId))
                return null;
               var user =await userManager.FindByIdAsync(userIdClaim.Value);
            if (user is not null&&user.CurrentTokenId== tokenIdClim)
                return userId;


            return null;
        }
    }
}
