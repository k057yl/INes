using INest.Data.Entities.Infrastructure;
using INest.Models.DTOs.Reminder;
using MediatR;

namespace INest.Services.Features.Reminders.Commands.AddReminder
{
    public record AddReminderCommand(Guid UserId, CreateReminderDto Dto) : IRequest<Reminder>;
}
