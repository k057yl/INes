using FluentValidation;
using INest.Constants;

namespace INest.Services.Features.Items.Commands.CreateItem
{
    public class CreateItemValidator : AbstractValidator<CreateItemCommand>
    {
        public CreateItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .MaximumLength(100);

            RuleFor(x => x.Dto.CategoryId)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.Dto.StorageLocationId)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.Dto.PurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage(LocalizationConstants.ERRORS.NEGATIVE_NUMBER)
                .When(x => x.Dto.PurchasePrice.HasValue);

            RuleFor(x => x.Dto.PurchaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(LocalizationConstants.ERRORS.FUTURE_DATE)
                .When(x => x.Dto.PurchaseDate.HasValue);
        }
    }
}