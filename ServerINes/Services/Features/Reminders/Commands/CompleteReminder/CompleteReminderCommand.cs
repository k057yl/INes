using MediatR;

namespace INest.Services.Features.Reminders.Commands.CompleteReminder
{
    public record CompleteReminderCommand(Guid UserId, Guid ReminderId) : IRequest<bool>;
}
