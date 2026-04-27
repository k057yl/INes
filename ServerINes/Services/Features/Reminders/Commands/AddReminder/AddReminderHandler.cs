using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Reminders.Commands.AddReminder
{
    public class AddReminderHandler : IRequestHandler<AddReminderCommand, Reminder>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public AddReminderHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Reminder> Handle(AddReminderCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var itemExists = await _context.Items.AnyAsync(i => i.Id == dto.ItemId && i.UserId == request.UserId, cancellationToken);
            if (!itemExists) throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var safeTitle = _sanitizer.Sanitize(dto.Title);
            if (string.IsNullOrWhiteSpace(safeTitle))
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                ItemId = dto.ItemId,
                Title = safeTitle,
                Type = dto.Type,
                Recurrence = dto.Recurrence,
                TriggerAt = dto.TriggerAt,
                IsCompleted = false,
                IsNotificationSent = false
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return reminder;
        }
    }
}
