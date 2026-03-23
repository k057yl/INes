using INest.Models.DTOs.Reminder;
using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface IReminderService
    {
        Task<Reminder> AddReminderAsync(Guid userId, CreateReminderDto dto);
        Task<bool> CompleteReminderAsync(Guid userId, Guid reminderId);
        Task<bool> DeleteReminderAsync(Guid userId, Guid reminderId);
        Task<IEnumerable<Reminder>> GetActiveRemindersAsync(Guid userId);
        Task<IEnumerable<Reminder>> GetItemRemindersAsync(Guid userId, Guid itemId);
    }
}
