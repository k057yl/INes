using MediatR;

namespace INest.Features.Feedback.Commands.RateFeedback
{
    public record RateFeedbackCommand(
        Guid FeedbackId,
        int Rating,
        string? MissingFeatures) : IRequest;
}
