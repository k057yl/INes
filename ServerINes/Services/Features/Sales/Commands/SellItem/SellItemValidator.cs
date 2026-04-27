using FluentValidation;
using INest.Constants;
using INest.Services.Features.Sales.Commands.SellItem;

namespace INest.Services.Features.Sales.Commands.SellItem
{
    public class SellItemValidator : AbstractValidator<SellItemCommand>
    {
        public SellItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.Dto.ItemId)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.Dto.SalePrice)
                .GreaterThan(0)
                .WithMessage(LocalizationConstants.ERRORS.NEGATIVE_NUMBER);

            RuleFor(x => x.Dto.SoldDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage(LocalizationConstants.ERRORS.FUTURE_DATE);
        }
    }
}