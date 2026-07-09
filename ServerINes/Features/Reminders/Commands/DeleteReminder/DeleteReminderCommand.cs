using MediatR;

namespace INest.Features.Reminders.Commands.DeleteReminder
{
    public record DeleteReminderCommand(Guid UserId, Guid ReminderId) : IRequest<bool>;
}
