using INest.Data.Entities.Infrastructure;
using System.Security.Claims;

namespace INest.Infrastructure.Identity
{
    public interface ITokenService
    {
        string GenerateJwtToken(AppUser user, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        string HashRefreshToken(string refreshToken);
    }
}
