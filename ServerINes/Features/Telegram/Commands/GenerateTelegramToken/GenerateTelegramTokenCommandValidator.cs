using FluentValidation;
using INest.Constants;

namespace INest.Features.Telegram.Commands.GenerateTelegramToken
{
    public class GenerateTelegramTokenCommandValidator : AbstractValidator<GenerateTelegramTokenCommand>
    {
        public GenerateTelegramTokenCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .NotEqual(Guid.Empty)
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);
        }
    }
}
