using FluentValidation;
using INest.Constants;

namespace INest.Features.Telegram.Commands.ConnectTelegram
{
    public class ConnectTelegramCommandValidator : AbstractValidator<ConnectTelegramCommand>
    {
        public ConnectTelegramCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.ChatId)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);
        }
    }
}
