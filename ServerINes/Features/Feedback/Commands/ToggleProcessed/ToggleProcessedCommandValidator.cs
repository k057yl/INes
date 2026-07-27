using FluentValidation;

namespace INest.Features.Feedback.Commands.ToggleProcessed
{
    public class ToggleProcessedCommandValidator : AbstractValidator<ToggleProcessedCommand>
    {
        public ToggleProcessedCommandValidator()
        {
            RuleFor(x => x.FeedbackId)
                .NotEmpty();
        }
    }
}
