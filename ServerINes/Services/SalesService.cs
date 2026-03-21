using INest.Constants;
using INest.Models.DTOs.Sale;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class SalesService : ISalesService
    {
        private readonly AppDbContext _context;

        public SalesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SaleResponseDto> SellItemAsync(Guid userId, SellItemRequestDto request)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == userId);

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            if (item.Status == ItemStatus.Sold)
                throw new InvalidOperationException(LocalizationConstants.SALES.ALREADY_SOLD);

            decimal purchasePrice = item.PurchasePrice ?? 0;
            decimal profit = request.SalePrice - purchasePrice;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                ItemNameSnapshot = item.Name,
                CategoryNameSnapshot = item.Category?.Name,
                SalePrice = request.SalePrice,
                Profit = profit,
                SoldDate = request.SoldDate,
                PlatformId = request.PlatformId,
                Comment = request.Comment,
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
                Comment = $"Sold for {request.SalePrice}",
                OldValue = oldStatus.ToString(),
                NewValue = ItemStatus.Sold.ToString()
            });

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            string? platformName = null;
            if (request.PlatformId.HasValue)
            {
                platformName = await _context.Platforms
                    .AsNoTracking()
                    .Where(p => p.Id == request.PlatformId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
            }

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

        public async Task<List<SaleResponseDto>> GetSalesAsync(Guid userId)
        {
            return await _context.Sales
                .Include(s => s.Platform)
                .AsNoTracking()
                .Where(s => _context.Items.Any(i => i.Id == s.ItemId && i.UserId == userId) || s.ItemId == null)
                .OrderByDescending(s => s.SoldDate)
                .Select(s => new SaleResponseDto
                {
                    SaleId = s.Id,
                    ItemId = s.ItemId ?? Guid.Empty,
                    ItemName = s.ItemNameSnapshot,
                    SalePrice = s.SalePrice,
                    Profit = s.Profit,
                    SoldDate = s.SoldDate,
                    PlatformName = s.Platform != null ? s.Platform.Name : null
                })
                .ToListAsync();
        }

        public async Task<bool> CancelSaleAsync(Guid userId, Guid itemId)
        {
            var item = await _context.Items
                .Include(i => i.Sale)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null || item.Sale == null)
                throw new KeyNotFoundException(LocalizationConstants.SALES.NOT_FOUND);

            var oldStatus = item.Status;
            _context.Sales.Remove(item.Sale);
            item.Status = ItemStatus.Active;

            _context.ItemHistories.Add(new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Type = ItemHistoryType.StatusChanged,
                CreatedAt = DateTime.UtcNow,
                Comment = "Sale cancelled",
                OldValue = oldStatus.ToString(),
                NewValue = ItemStatus.Active.ToString()
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSaleRecordAsync(Guid userId, Guid saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Item)
                .FirstOrDefaultAsync(s => s.Id == saleId && (s.Item == null || s.Item.UserId == userId));

            if (sale == null)
                throw new KeyNotFoundException(LocalizationConstants.SALES.NOT_FOUND);

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SmartDeleteAsync(Guid userId, Guid saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Item)
                .FirstOrDefaultAsync(s => s.Id == saleId && (s.Item == null || s.Item.UserId == userId));

            if (sale == null)
                throw new KeyNotFoundException(LocalizationConstants.SALES.NOT_FOUND);

            if (sale.ItemId.HasValue)
            {
                var item = await _context.Items.FindAsync(sale.ItemId.Value);
                if (item != null)
                {
                    _context.Items.Remove(item);
                }
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}