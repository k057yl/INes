using INest.Constants;
using INest.Models.DTOs.Lending;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class LendingService : ILendingService
    {
        private readonly AppDbContext _context;
        public LendingService(AppDbContext context) => _context = context;

        public async Task<Lending> LendItemAsync(Guid userId, LendItemDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == userId);

            if (item == null)
                throw new KeyNotFoundException(LocalizationConstants.ITEMS.NOT_FOUND);

            if (item.Status == ItemStatus.Lent)
                throw new InvalidOperationException(LocalizationConstants.LENDING.ALREADY_LENT);

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                PersonName = dto.PersonName,
                DateGiven = DateTime.UtcNow,
                ExpectedReturnDate = dto.ExpectedReturnDate,
                ValueAtLending = item.EstimatedValue,
                Comment = dto.Comment
            };

            item.Status = ItemStatus.Lent;
            _context.Lendings.Add(lending);
            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = item.Id,
                Type = ItemHistoryType.Lent,
                NewValue = $"{LocalizationConstants.HISTORY.LENT}|{dto.PersonName}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return lending;
        }

        public async Task<bool> ReturnItemAsync(Guid userId, Guid itemId, ReturnItemDto dto)
        {
            var item = await _context.Items
                .Include(i => i.Lending)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item?.Lending == null)
                throw new KeyNotFoundException(LocalizationConstants.LENDING.NOT_LENT);

            item.Status = ItemStatus.Active;
            item.Lending.ReturnedDate = dto.ReturnedDate ?? DateTime.UtcNow;

            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = item.Id,
                Type = ItemHistoryType.Returned,
                NewValue = LocalizationConstants.HISTORY.RETURNED,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
