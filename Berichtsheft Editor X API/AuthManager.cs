using Berichtsheft_Editor_X.Models;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Berichtsheft_Editor_X_API
{
    public class AuthManager
    {
        public CustomSettings _CustomSettings;

        public AuthManager(CustomSettings customSettings)
        {
            _CustomSettings = customSettings;
        }

        public string GetPrivateKey()
        {
            return File.ReadAllText(_CustomSettings.PrivateKeyLocation);
        }

        public string GenerateToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetPrivateKey());
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaimsIdentity(user),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = credentials,
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaimsIdentity(User user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));

            var roles = user.Roles.Split(',');

            foreach (var role in roles)
                claims.AddClaim(new Claim(ClaimTypes.Role, role));

            return claims;
        }
    }
}