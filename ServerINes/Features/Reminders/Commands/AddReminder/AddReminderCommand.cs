using INest.Data.Entities.Infrastructure;
using INest.Features.Reminders.DTOs;
using MediatR;

namespace INest.Features.Reminders.Commands.AddReminder
{
    public record AddReminderCommand(Guid UserId, CreateReminderDto Dto) : IRequest<Reminder>;
}
