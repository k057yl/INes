using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Lendings.Commands.LendItem
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

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var item = await _context.Items
                    .Include(i => i.Lending)
                    .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == request.UserId, cancellationToken);

                if (item == null)
                    throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

                if (item.Lending != null && item.Lending.ReturnedDate == null)
                    throw new InvalidOperationException(LENDING.ERRORS.ALREADY_LENT);

                if (item.Lending != null)
                {
                    _context.Lendings.Remove(item.Lending);
                }

                var lending = new Lending
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
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
                    Type = ItemHistoryType.Lent,
                    NewValue = $"{safePersonName}|{lending.ValueAtLending}$",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _tracker.InvalidateUserCache(request.UserId);
                return lending;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}