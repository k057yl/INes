using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface ITokenService
    {
        public string GenerateJwtToken(AppUser user, IList<string> roles);
    }
}
