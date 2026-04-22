using FluentValidation;
using INest.Models.DTOs.Item;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class UpdateItemFullRules : AbstractValidator<UpdateItemFullDto>
    {
        public UpdateItemFullRules()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
        }
    }

    public class UpdateItemPartialRules : AbstractValidator<UpdateItemPartialDto>
    {
        public UpdateItemPartialRules()
        {
            RuleFor(x => x.Name).NotEmpty().When(x => x.Name != null).WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue).WithMessage(ERRORS.NEGATIVE_NUMBER);
        }
    }
}