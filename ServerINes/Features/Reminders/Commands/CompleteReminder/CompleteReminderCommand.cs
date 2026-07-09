using MediatR;

namespace INest.Features.Reminders.Commands.CompleteReminder
{
    public record CompleteReminderCommand(Guid UserId, Guid ReminderId) : IRequest<bool>;
}
