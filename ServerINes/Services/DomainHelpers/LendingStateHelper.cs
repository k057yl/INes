using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
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
            item.Status = newStatus;

            var lending = await _context.Lendings
                .FirstOrDefaultAsync(l => l.ItemId == item.Id);

            if (newStatus == ItemStatus.Lent || newStatus == ItemStatus.Borrowed)
            {
                if (lending == null)
                {
                    lending = new Lending
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        UserId = item.UserId,
                        DateGiven = DateTime.UtcNow,
                        ValueAtLending = item.EstimatedValue,
                        SendNotification = sendNotification,
                        NotificationSent = false
                    };

                    _context.Lendings.Add(lending);

                    _context.ItemHistories.Add(new ItemHistory
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        UserId = item.UserId,
                        Type = newStatus == ItemStatus.Lent
                            ? ItemHistoryType.Lent
                            : ItemHistoryType.Borrowed,
                        NewValue = personName
                    });
                }

                lending.PersonName = personName;
                lending.ContactEmail = contactEmail;
                lending.ExpectedReturnDate = expectedReturnDate;
                lending.Direction = newStatus == ItemStatus.Borrowed
                        ? LendingDirection.In
                        : LendingDirection.Out;

                lending.SendNotification = sendNotification;

                if (sendNotification)
                {
                    lending.NotificationSent = false;
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
                                UserId = item.UserId,
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

                        lending.NotificationSent = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email failed for {Item}", item.Name);
                    }
                }
            }
            else if (lending != null)
            {
                _context.Lendings.Remove(lending);
            }
        }
    }
}