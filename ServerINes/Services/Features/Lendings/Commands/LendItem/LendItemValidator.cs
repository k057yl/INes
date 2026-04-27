using FluentValidation;
using static INest.Constants.LocalizationConstants;
namespace INest.Services.Features.Lendings.Commands.LendItem
{
    public class LendItemValidator : AbstractValidator<LendItemCommand>
    {
        public LendItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Dto.ItemId).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.Dto.PersonName).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.Dto.ExpectedReturnDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.Dto.ExpectedReturnDate.HasValue)
                .WithMessage(ERRORS.FUTURE_DATE);
        }
    }
}
