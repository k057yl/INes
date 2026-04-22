using FluentValidation;
using INest.Models.DTOs.Reminder;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class ReminderRules : AbstractValidator<CreateReminderDto>
    {
        public ReminderRules()
        {
            RuleFor(x => x.ItemId).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.Title).NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
            RuleFor(x => x.TriggerAt)
                .GreaterThan(DateTime.UtcNow).WithMessage(ERRORS.FUTURE_DATE);
        }
    }
}