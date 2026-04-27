using MediatR;

namespace INest.Services.Features.Reminders.Commands.DeleteReminder
{
    public record DeleteReminderCommand(Guid UserId, Guid ReminderId) : IRequest<bool>;
}
