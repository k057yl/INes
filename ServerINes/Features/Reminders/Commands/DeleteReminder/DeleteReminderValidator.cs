using FluentValidation;

namespace INest.Features.Reminders.Commands.DeleteReminder
{
    public class DeleteReminderValidator : AbstractValidator<DeleteReminderCommand>
    {
        public DeleteReminderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ReminderId).NotEmpty();
        }
    }
}
