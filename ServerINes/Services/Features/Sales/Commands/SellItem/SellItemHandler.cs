using Ganss.Xss;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using INest.Models.DTOs.Sale;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Sales.Commands.SellItem
{
    public class SellItemHandler : IRequestHandler<SellItemCommand, SaleResponseDto>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public SellItemHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
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
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status == ItemStatus.Sold)
                throw new InvalidOperationException(SALES.ERRORS.ALREADY_SOLD);

            var safeComment = !string.IsNullOrEmpty(dto.Comment) ? _sanitizer.Sanitize(dto.Comment) : null;

            decimal purchasePrice = item.PurchasePrice ?? 0;
            decimal profit = dto.SalePrice - purchasePrice;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ItemId = item.Id,
                ItemNameSnapshot = item.Name,
                CategoryNameSnapshot = item.Category?.Name,
                SalePrice = dto.SalePrice,
                Profit = profit,
                SoldDate = dto.SoldDate,
                PlatformId = dto.PlatformId,
                Comment = safeComment,
                CreatedAt = DateTime.UtcNow
            };

            var oldStatus = item.Status;
            item.Status = ItemStatus.Sold;

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Type = ItemHistoryType.StatusChanged,
                CreatedAt = DateTime.UtcNow,
                Comment = HISTORY.SOLD_FOR,
                OldValue = oldStatus.ToString(),
                NewValue = dto.SalePrice.ToString()
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