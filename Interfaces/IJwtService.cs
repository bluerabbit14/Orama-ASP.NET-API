using Orama_API.Domain;

namespace Orama_API.Interfaces
{
    public interface IJwtService
    {
        public string GenerateToken(UserProfile user);
        public DateTime? GetTokenExpiration(string token);
        public string ValidateAndDebugToken(string token);
        public string DecodeTokenClaims(string token);
    }
}
