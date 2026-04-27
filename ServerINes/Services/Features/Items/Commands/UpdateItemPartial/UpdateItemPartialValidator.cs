using FluentValidation;
using INest.Constants;

namespace INest.Services.Features.Items.Commands.UpdateItemPartial
{
    public class UpdateItemPartialValidator : AbstractValidator<UpdateItemPartialCommand>
    {
        public UpdateItemPartialValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();

            RuleFor(x => x.Dto.Name)
                .NotEmpty()
                .When(x => x.Dto.Name != null)
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD);

            RuleFor(x => x.Dto.PurchasePrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Dto.PurchasePrice.HasValue)
                .WithMessage(LocalizationConstants.ERRORS.NEGATIVE_NUMBER);
        }
    }
}