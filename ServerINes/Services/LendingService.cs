using FluentValidation;
using Ganss.Xss;
using INest.Exceptions;
using INest.Models.DTOs.Lending;
using INest.Models.Entities;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services
{
    public class LendingService : ILendingService
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly IValidator<LendItemDto> _lendValidator;
        private readonly IValidator<ReturnItemDto> _returnValidator;

        public LendingService(
            AppDbContext context,
            IHtmlSanitizer sanitizer,
            IValidator<LendItemDto> lendValidator,
            IValidator<ReturnItemDto> returnValidator)
        {
            _context = context;
            _sanitizer = sanitizer;
            _lendValidator = lendValidator;
            _returnValidator = returnValidator;
        }

        public async Task<Lending> LendItemAsync(Guid userId, LendItemDto dto)
        {
            var valResult = await _lendValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            var safePersonName = _sanitizer.Sanitize(dto.PersonName);
            if (string.IsNullOrWhiteSpace(safePersonName))
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var safeComment = !string.IsNullOrEmpty(dto.Comment) ? _sanitizer.Sanitize(dto.Comment) : null;

            using var transaction = await _context.Database.BeginTransactionAsync();

            var item = await _context.Items
                .Include(i => i.Lending)
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.UserId == userId);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Lending != null && item.Lending.ReturnedDate == null)
                throw new InvalidOperationException(LENDING.ERRORS.ALREADY_LENT);

            if (item.Lending != null)
            {
                _context.Lendings.Remove(item.Lending);
                await _context.SaveChangesAsync();
            }

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                PersonName = safePersonName,
                DateGiven = DateTime.UtcNow,
                ExpectedReturnDate = dto.ExpectedReturnDate,
                ValueAtLending = dto.ValueAtLending ?? item.EstimatedValue,
                Comment = safeComment
            };

            item.Status = ItemStatus.Lent;
            _context.Lendings.Add(lending);

            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = item.Id,
                Type = ItemHistoryType.Lent,
                NewValue = $"{safePersonName}|{lending.ValueAtLending}$",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return lending;
        }

        public async Task<bool> ReturnItemAsync(Guid userId, Guid itemId, ReturnItemDto dto)
        {
            var valResult = await _returnValidator.ValidateAsync(dto);
            if (!valResult.IsValid) throw new ValidationAppException(valResult.Errors);

            var item = await _context.Items
                .Include(i => i.Lending)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item?.Lending == null)
                throw new KeyNotFoundException(LENDING.ERRORS.NOT_LENT);

            item.Status = ItemStatus.Active;
            item.Lending.ReturnedDate = dto.ReturnedDate ?? DateTime.UtcNow;

            _context.ItemHistories.Add(new ItemHistory
            {
                ItemId = item.Id,
                Type = ItemHistoryType.Returned,
                NewValue = HISTORY.RETURNED,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
