using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Features.Sales.DTOs;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Sales.Commands.SellItem
{
    public class SellItemHandler : IRequestHandler<SellItemCommand, SaleResponseDto>
    {
        private readonly AppDbContext _context;
        private readonly ISanitizerService _sanitizer;
        private readonly ICacheTracker _tracker;

        public SellItemHandler(AppDbContext context, ISanitizerService sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<SaleResponseDto> Handle(SellItemCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Details)
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            item.Sell();

            var activeReminders = await _context.Reminders
                .Where(r => r.ItemId == item.Id && r.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            if (activeReminders.Any())
            {
                _context.Reminders.RemoveRange(activeReminders);
            }

            var safeComment = !string.IsNullOrEmpty(dto.Comment) ? _sanitizer.SanitizeHtml(dto.Comment) : null;

            decimal purchasePrice = item.Details?.PurchasePrice ?? 0;
            string currency = item.Details?.Currency ?? "USD";

            decimal platformFee = dto.PlatformFee ?? 0;
            decimal profit = dto.SalePrice - purchasePrice;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ItemId = item.Id,
                ItemNameSnapshot = item.Name,
                CategoryNameSnapshot = item.Category?.Name,
                CategoryId = item.CategoryId,
                PurchasePriceSnapshot = purchasePrice,
                Currency = currency,
                PlatformFee = platformFee,
                SalePrice = dto.SalePrice,
                Profit = profit,
                SoldDate = dto.SoldDate,
                PlatformId = dto.PlatformId,
                Comment = safeComment
            };

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = request.UserId,
                Type = ItemHistoryType.Sold,
                Comment = HISTORY.SOLD_FOR,
                OldValue = item.Details?.PurchasePrice?.ToString() ?? "0",
                NewValue = dto.SalePrice.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync(cancellationToken);

            string? platformName = null;
            if (dto.PlatformId.HasValue)
            {
                platformName = await _context.Platforms
                    .AsNoTracking()
                    .Where(p => p.Id == dto.PlatformId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            _tracker.InvalidateUserCache(request.UserId);

            return new SaleResponseDto
            {
                SaleId = sale.Id,
                ItemId = item.Id,
                ItemName = sale.ItemNameSnapshot,
                SalePrice = sale.SalePrice,
                Profit = sale.Profit,
                SoldDate = sale.SoldDate,
                PlatformName = platformName
            };
        }
    }
}