using Ganss.Xss;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Lendings.Commands.LendItem
{
    public class LendItemHandler : IRequestHandler<LendItemCommand, Lending>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public LendItemHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Lending> Handle(LendItemCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var safePersonName = _sanitizer.Sanitize(dto.PersonName);

            if (string.IsNullOrWhiteSpace(safePersonName))
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var safeComment = !string.IsNullOrEmpty(dto.Comment) ? _sanitizer.Sanitize(dto.Comment) : null;

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            var existingLending = await _context.Lendings
                .FirstOrDefaultAsync(l => l.ItemId == item.Id, cancellationToken);

            if (existingLending != null && existingLending.ReturnedDate == null)
                throw new InvalidOperationException(LENDING.ERRORS.ALREADY_LENT);

            if (existingLending != null)
            {
                _context.Lendings.Remove(existingLending);
            }

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                PersonName = safePersonName,
                DateGiven = DateTime.UtcNow,
                ExpectedReturnDate = dto.ExpectedReturnDate,
                ValueAtLending = dto.ValueAtLending ?? item.EstimatedValue,
                Comment = safeComment,
                Direction = LendingDirection.Out
            };

            item.Status = ItemStatus.Lent;
            _context.Lendings.Add(lending);

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = ItemHistoryType.Lent,
                NewValue = $"{safePersonName}|{lending.ValueAtLending}$"
            });

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return lending;
        }
    }
}