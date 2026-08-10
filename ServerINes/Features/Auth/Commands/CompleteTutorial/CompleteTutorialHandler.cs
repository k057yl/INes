using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.CompleteTutorial
{
    public class CompleteTutorialHandler : IRequestHandler<CompleteTutorialCommand, TutorialSteps>
    {
        private readonly UserManager<AppUser> _userManager;

        public CompleteTutorialHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<TutorialSteps> Handle(CompleteTutorialCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException(AUTH.ERRORS.USER_NOT_FOUND);

            user.CompletedTutorials |= request.Step;

            await _userManager.UpdateAsync(user);

            return user.CompletedTutorials;
        }
    }
}
