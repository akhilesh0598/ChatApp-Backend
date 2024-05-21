using Domain;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(ApplicationUser user)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name,user.UserName),
                new(ClaimTypes.NameIdentifier,user.Id),
                new(ClaimTypes.Email,user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(""));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = cred
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var toekn = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(toekn);
        }
    }
}
