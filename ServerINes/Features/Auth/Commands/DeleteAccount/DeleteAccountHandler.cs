using INest.Data.Entities.Infrastructure;
using INest.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.DeleteAccount
{
    public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteAccountHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new AppException(AUTH.ERRORS.USER_NOT_FOUND, 404);
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new AppException(AUTH.ERRORS.INVALID_CREDENTIALS, 400);
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                {
                    throw new AppException(AUTH.ERRORS.INVALID_CREDENTIALS, 400);
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errorDescription = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new AppException($"{SYSTEM.DEFAULT_ERROR}: {errorDescription}", 500);
            }

            return true;
        }
    }
}
