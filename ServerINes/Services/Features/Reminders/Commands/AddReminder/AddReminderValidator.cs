using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Reminders.Commands.AddReminder
{
    public class AddReminderValidator : AbstractValidator<AddReminderCommand>
    {
        public AddReminderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Dto.ItemId).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.Dto.Title).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.Dto.TriggerAt)
                .GreaterThan(DateTime.UtcNow).WithMessage(ERRORS.FUTURE_DATE);
        }
    }
}
