using INest.Data.Enums;
using MediatR;

namespace INest.Features.Auth.Commands.CompleteTutorial
{
    public record CompleteTutorialCommand(string UserId, TutorialSteps Step) : IRequest<TutorialSteps>;
}