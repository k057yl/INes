using INest.Data.Enums;
using MediatR;

namespace INest.Features.Feedback.Commands.CreateFeedback
{
    public record CreateFeedbackCommand(
        Guid UserId,
        FeedbackType Type,
        string Message) : IRequest<Guid>;
}
