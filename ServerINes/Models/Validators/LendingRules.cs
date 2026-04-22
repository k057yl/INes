using FluentValidation;
using INest.Models.DTOs.Lending;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class LendItemRules : AbstractValidator<LendItemDto>
    {
        public LendItemRules()
        {
            RuleFor(x => x.ItemId).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.PersonName).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.ExpectedReturnDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.ExpectedReturnDate.HasValue)
                .WithMessage(ERRORS.FUTURE_DATE);
        }
    }

    public class ReturnItemRules : AbstractValidator<ReturnItemDto>
    {
        public ReturnItemRules()
        {
            RuleFor(x => x.ReturnedDate)
                .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.ReturnedDate.HasValue)
                .WithMessage(ERRORS.FUTURE_DATE);
        }
    }
}