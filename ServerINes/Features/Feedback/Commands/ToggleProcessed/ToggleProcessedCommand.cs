using MediatR;

namespace INest.Features.Feedback.Commands.ToggleProcessed
{
    public record ToggleProcessedCommand(Guid FeedbackId) : IRequest;
}
