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
                throw new KeyNotFoundException("Item not found");

            if (item.Status == ItemStatus.Sold)
                throw new InvalidOperationException("Item already sold");

            decimal purchasePrice = item.PurchasePrice ?? 0;
            decimal profit = request.SalePrice - purchasePrice;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                SalePrice = request.SalePrice,
                Profit = profit,
                SoldDate = request.SoldDate,
                PlatformId = request.PlatformId,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            var oldStatus = item.Status;
            item.Status = ItemStatus.Sold;

            var history = new ItemHistory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Type = ItemHistoryType.StatusChanged,
                CreatedAt = DateTime.UtcNow,
                Comment = $"Sold for {request.SalePrice}",
                OldValue = oldStatus.ToString(),
                NewValue = ItemStatus.Sold.ToString()
            };

            _context.Sales.Add(sale);
            _context.Add(history);

            await _context.SaveChangesAsync();

            string? platformName = null;
            if (request.PlatformId.HasValue)
            {
                var platform = await _context.StorageLocations
                    .Where(p => p.Id == request.PlatformId)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync();
                platformName = platform;
            }

            return new SaleResponseDto
            {
                SaleId = sale.Id,
                ItemId = item.Id,
                ItemName = item.Name,
                SalePrice = sale.SalePrice,
                Profit = sale.Profit,
                SoldDate = sale.SoldDate,
                PlatformName = platformName
            };
        }

        public async Task<List<SaleResponseDto>> GetSalesAsync(Guid userId)
        {
            return await _context.Sales
                .Include(s => s.Item)
                .Include(s => s.Platform)
                .Where(s => s.Item.UserId == userId)
                .OrderByDescending(s => s.SoldDate)
                .Select(s => new SaleResponseDto
                {
                    SaleId = s.Id,
                    ItemId = s.ItemId,
                    ItemName = s.Item.Name,
                    SalePrice = s.SalePrice,
                    Profit = s.Profit,
                    SoldDate = s.SoldDate,
                    PlatformName = s.Platform != null ? s.Platform.Name : null
                })
                .ToListAsync();
        }
    }
}