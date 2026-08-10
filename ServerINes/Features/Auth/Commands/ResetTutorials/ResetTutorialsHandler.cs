using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ResetTutorials
{
    public class ResetTutorialsHandler : IRequestHandler<ResetTutorialsCommand, bool>
    {
        private readonly UserManager<AppUser> _userManager;

        public ResetTutorialsHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(ResetTutorialsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException(AUTH.ERRORS.USER_NOT_FOUND);

            user.CompletedTutorials = TutorialSteps.None;

            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}
