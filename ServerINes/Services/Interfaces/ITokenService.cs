using INest.Models.Entities;
using System.Security.Claims;

namespace INest.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(AppUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        string HashRefreshToken(string refreshToken);
    }
}
