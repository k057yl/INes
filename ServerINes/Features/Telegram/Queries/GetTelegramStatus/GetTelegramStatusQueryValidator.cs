using FluentValidation;
using INest.Constants;

namespace INest.Features.Telegram.Queries.GetTelegramStatus
{
    public class GetTelegramStatusQueryValidator : AbstractValidator<GetTelegramStatusQuery>
    {
        public GetTelegramStatusQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .NotEqual(Guid.Empty)
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);
        }
    }
}
