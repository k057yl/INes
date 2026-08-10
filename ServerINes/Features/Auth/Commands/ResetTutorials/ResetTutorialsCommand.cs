using MediatR;

namespace INest.Features.Auth.Commands.ResetTutorials
{
    public record ResetTutorialsCommand(string UserId) : IRequest<bool>;
}
