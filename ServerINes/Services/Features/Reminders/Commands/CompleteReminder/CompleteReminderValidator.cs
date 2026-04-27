using FluentValidation;

namespace INest.Services.Features.Reminders.Commands.CompleteReminder
{
    public class CompleteReminderValidator : AbstractValidator<CompleteReminderCommand>
    {
        public CompleteReminderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ReminderId).NotEmpty();
        }
    }
}
