using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.DomainHelpers
{
    public class LendingStateHelper
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<LendingStateHelper> _logger;

        public LendingStateHelper(AppDbContext context, IEmailService emailService, ILogger<LendingStateHelper> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SyncLendingStateAsync(
            Item item,
            ItemStatus newStatus,
            string personName,
            string? contactEmail,
            DateTime? expectedReturnDate,
            bool sendNotification)
        {
            if (newStatus == ItemStatus.Lent || newStatus == ItemStatus.Borrowed)
            {
                if (item.Lending == null)
                {
                    item.Lending = new Lending
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        DateGiven = DateTime.UtcNow,
                        ValueAtLending = item.EstimatedValue,
                        SendNotification = sendNotification,
                        NotificationSent = false
                    };

                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Type = newStatus == ItemStatus.Lent
                            ? ItemHistoryType.Lent
                            : ItemHistoryType.Borrowed,
                        NewValue = personName,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                item.Lending.PersonName = personName;
                item.Lending.ContactEmail = contactEmail;
                item.Lending.ExpectedReturnDate = expectedReturnDate;
                item.Lending.Direction =
                    newStatus == ItemStatus.Borrowed
                        ? LendingDirection.In
                        : LendingDirection.Out;

                item.Lending.SendNotification = sendNotification;

                if (sendNotification)
                {
                    item.Lending.NotificationSent = false;
                }

                if (sendNotification && expectedReturnDate.HasValue)
                {
                    var reminderDate = expectedReturnDate.Value.AddDays(-1);

                    if (reminderDate > DateTime.UtcNow)
                    {
                        var existingReminder = await _context.Reminders
                            .FirstOrDefaultAsync(r =>
                                r.ItemId == item.Id &&
                                r.Type == ReminderType.ReturnItem &&
                                !r.IsCompleted);

                        if (existingReminder != null)
                        {
                            existingReminder.TriggerAt = reminderDate;
                        }
                        else
                        {
                            _context.Reminders.Add(new Reminder
                            {
                                Id = Guid.NewGuid(),
                                ItemId = item.Id,
                                TriggerAt = reminderDate,
                                Type = ReminderType.ReturnItem,
                                IsCompleted = false
                            });
                        }
                    }
                }

                if (sendNotification && !string.IsNullOrEmpty(contactEmail))
                {
                    try
                    {
                        await _emailService.SendLendingNotificationAsync(
                            contactEmail,
                            item.Name,
                            personName,
                            expectedReturnDate,
                            newStatus == ItemStatus.Borrowed);

                        item.Lending.NotificationSent = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email failed for {Item}", item.Name);
                    }
                }
            }
            else if (item.Lending != null)
            {
                _context.Lendings.Remove(item.Lending);
            }
        }
    }
}