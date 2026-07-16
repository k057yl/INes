using FluentValidation;
using INest.Constants;

namespace INest.Features.Telegram.Commands.UnlinkTelegram
{
    public class UnlinkTelegramCommandValidator : AbstractValidator<UnlinkTelegramCommand>
    {
        public UnlinkTelegramCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .NotEqual(Guid.Empty)
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);
        }
    }
}
