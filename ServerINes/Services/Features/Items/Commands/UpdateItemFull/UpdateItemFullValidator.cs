using FluentValidation;
using INest.Constants;

namespace INest.Services.Features.Items.Commands.UpdateItemFull
{
    public class UpdateItemFullValidator : AbstractValidator<UpdateItemFullCommand>
    {
        public UpdateItemFullValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();

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
        }
    }
}