using FluentValidation;
using INest.Models.DTOs.Sale;
using INest.Constants;

namespace INest.Models.Validators
{
    public class SalesRules : AbstractValidator<SellItemRequestDto>
    {
        public SalesRules()
        {
            RuleFor(x => x.ItemId)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.SalePrice)
                .GreaterThan(0)
                .WithMessage(LocalizationConstants.ERRORS.NEGATIVE_NUMBER);

            RuleFor(x => x.SoldDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage(LocalizationConstants.ERRORS.FUTURE_DATE);
        }
    }
}