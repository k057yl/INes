using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Lendings.Commands.ReturnItem
{
    public class ReturnItemValidator : AbstractValidator<ReturnItemCommand>
    {
        public ReturnItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.Dto.ReturnedDate)
                .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.Dto.ReturnedDate.HasValue)
                .WithMessage(ERRORS.FUTURE_DATE);
        }
    }
}
