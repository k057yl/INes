using INest.Data.Entities.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace INest.Features.Auth.Commands.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand>
    {
        private readonly UserManager<AppUser> _userManager;

        public LogoutHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
