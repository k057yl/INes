using FluentValidation;
using INest.Data.Enums;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.CompleteTutorial
{
    public class CompleteTutorialCommandValidator : AbstractValidator<CompleteTutorialCommand>
    {
        public CompleteTutorialCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.Step)
                .IsInEnum()
                .WithMessage(TUTORIAL.ERRORS.INVALID_STEP)
                .Must(step => step != TutorialSteps.None)
                .WithMessage(TUTORIAL.ERRORS.INVALID_STEP);
        }
    }
}
