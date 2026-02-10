using INest.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace INest.Services.Interfaces
{
    public interface IAuthService
    {
        public Task SendConfirmationCodeAsync(RegisterDto dto);
        public Task<bool> ConfirmRegistrationAsync(string email, string code);
        public Task<string?> LoginAsync(LoginDto dto);
        public Task ForgotPasswordAsync(ForgotPasswordDto dto);
        public Task<IdentityResult?> ResetPasswordAsync(ResetPasswordDto dto);
        public Task LogoutAsync(string userId);
    }
}
