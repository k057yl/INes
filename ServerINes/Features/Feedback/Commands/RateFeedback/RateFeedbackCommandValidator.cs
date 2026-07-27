using FluentValidation;

namespace INest.Features.Feedback.Commands.RateFeedback
{
    public class RateFeedbackCommandValidator : AbstractValidator<RateFeedbackCommand>
    {
        public RateFeedbackCommandValidator()
        {
            RuleFor(x => x.FeedbackId)
                .NotEmpty();

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5);

            RuleFor(x => x.MissingFeatures)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.MissingFeatures));
        }
    }
}
