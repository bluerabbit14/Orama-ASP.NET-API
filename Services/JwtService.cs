using Microsoft.IdentityModel.Tokens;
using Orama_API.Domain;
using Orama_API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Orama_API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateToken(UserProfile user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var tokenValidityMins = _config.GetValue<int>("Jwt:TokenValidityMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            if (string.IsNullOrWhiteSpace(user.Email))
            {
               throw new ArgumentException("User email cannot be null or empty");
            }
            
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.UserId.ToString()), // Custom claim for userId
                    new Claim(ClaimTypes.Role, user.Role?.Trim().ToLower() == "user" ? "User" : user.Role ?? "User"), // Role claim
                    new Claim("UserEmail", user.Email), // Custom email claim name
                    new Claim("Password", user.Password), // Password claim
                    new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(tokenExpiryTimeStamp).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Expiry time claim
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);
        }
        
        public DateTime? GetTokenExpiration(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;
            var jwtToken = handler.ReadJwtToken(token);
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
            if (expClaim != null && long.TryParse(expClaim.Value, out long expUnix))
            {
                // Convert Unix time to UTC DateTime  
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                return expirationTime;
            }
            return null;
        }
        
        public string ValidateAndDebugToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return "Token cannot be read - invalid format";
                
                var jwtToken = handler.ReadJwtToken(token);
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5) // Match the main JWT Bearer configuration
                };
                
                SecurityToken validatedToken;
                var principal = handler.ValidateToken(token, validationParameters, out validatedToken);
                
                return "Token is valid";
            }
            catch (SecurityTokenExpiredException)
            {
                return "Token has expired";
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return "Invalid issuer";
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return "Invalid audience";
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                return "Invalid signing key";
            }
            catch (Exception ex)
            {
                return $"Token validation failed: {ex.Message}";
            }
        }

        public string DecodeTokenClaims(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return "Token cannot be read - invalid format";
                
                var jwtToken = handler.ReadJwtToken(token);
                var claims = jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                
                // Check for custom email claim
                var userEmailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserEmail");
                var emailStatus = userEmailClaim != null ? $"✓ UserEmail found: {userEmailClaim.Value}" : "✗ UserEmail claim not found";
                
                return $"Token claims:\n{string.Join("\n", claims)}\n\nEmail Status: {emailStatus}";
            }
            catch (Exception ex)
            {
                return $"Error decoding token: {ex.Message}";
            }
        }
    }
}
