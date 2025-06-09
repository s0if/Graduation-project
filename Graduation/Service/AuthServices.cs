using Graduation.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Graduation.Service
{
    public class AuthServices
    {
        private readonly IConfiguration configuration;
        public AuthServices(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> CreateTokenAsync(ApplicationUser user,
       UserManager<ApplicationUser> userManager)
        {
            var configurations = configuration.GetSection("jwt")["secretkey"];
            var Authclaim = new List<Claim>()
             {
                 new Claim(ClaimTypes.GivenName,user.UserName) ,
                 
                 new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
             };
            if (!string.IsNullOrEmpty(user.Email))
                Authclaim.Add(new Claim(ClaimTypes.Email, user.Email));
            var userRole = await userManager.GetRolesAsync(user);
            foreach (var role in userRole)
                Authclaim.Add(new Claim(ClaimTypes.Role, role));
            var keyAuth = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurations));
            var tokenId = Guid.NewGuid().ToString(); 
            Authclaim.Add(new Claim("TokenId", tokenId));
            user.CurrentTokenId = tokenId;
            await userManager.UpdateAsync(user);
            var token = new JwtSecurityToken(
            //optinles
            audience: "GRADUATHION",
            issuer: "GRADUATHION PROJECT",
            
            //requierd
            claims: Authclaim,
            signingCredentials: new SigningCredentials(keyAuth,
           SecurityAlgorithms.HmacSha256Signature),
            //can change dateTime
            expires: DateTime.UtcNow.AddHours(1)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
