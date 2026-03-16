using FluentValidation;
using INest.Constants;
using INest.Models.DTOs.Item;

namespace INest.Models.Validators
{
    public class ItemCreateRules : AbstractValidator<CreateItemDto>
    {
        public ItemCreateRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .MaximumLength(100);

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.StorageLocationId)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage(LocalizationConstants.ERRORS.NEGATIVE_NUMBER)
                .When(x => x.PurchasePrice.HasValue);

            RuleFor(x => x.PurchaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(LocalizationConstants.ERRORS.FUTURE_DATE)
                .When(x => x.PurchaseDate.HasValue);
        }
    }
}
