using INest.Constants;
using INest.Data.Enums;
using INest.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Dashboard.Queries.GetLendingsAndWarrantiesStats
{
    public class GetLendingsAndWarrantiesStatsQueryHandler : IRequestHandler<GetLendingsAndWarrantiesStatsQuery, LendingsAndWarrantiesStatsDto>
    {
        private readonly AppDbContext _context;

        public GetLendingsAndWarrantiesStatsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LendingsAndWarrantiesStatsDto> Handle(GetLendingsAndWarrantiesStatsQuery request, CancellationToken cancellationToken)
        {
            var targetDate = request.NowUtc.AddDays(30);
            var pastGracePeriod = request.NowUtc.AddDays(-14);

            var expiredLendings = await _context.Lendings.AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null && l.ExpectedReturnDate.HasValue && l.ExpectedReturnDate.Value <= request.NowUtc, cancellationToken);

            var expiringLendings = await _context.Lendings.AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null && l.ExpectedReturnDate.HasValue && l.ExpectedReturnDate.Value >= request.NowUtc && l.ExpectedReturnDate.Value <= targetDate, cancellationToken);

            var expiringWarranties = await _context.ItemDetails.AsNoTracking()
                .CountAsync(d => d.Item!.UserId == request.UserId && d.WarrantyExpiration != null && d.WarrantyExpiration <= targetDate && d.WarrantyExpiration >= request.NowUtc && d.Item.Status != ItemStatus.Archived, cancellationToken);

            var lendingsList = await _context.Lendings.AsNoTracking()
                .Where(l => l.UserId == request.UserId && l.ReturnedDate == null && l.ExpectedReturnDate.HasValue && l.ExpectedReturnDate.Value <= targetDate)
                .Select(l => new AttentionItemDto
                {
                    ItemId = l.ItemId,
                    ItemName = l.Item!.Name,
                    LocationName = l.Item.StorageLocation != null ? l.Item.StorageLocation.Name : string.Empty,
                    TypeKey = LocalizationConstants.REMINDERS.RETURN_ITEM,
                    Date = l.ExpectedReturnDate!.Value,
                    Severity = l.ExpectedReturnDate.Value <= request.NowUtc ? "danger" : "warning"
                })
                .ToListAsync(cancellationToken);

            var warrantyList = await _context.ItemDetails.AsNoTracking()
                .Where(d => d.Item!.UserId == request.UserId
                         && d.WarrantyExpiration != null
                         && d.Item.Status != ItemStatus.Archived
                         && d.WarrantyExpiration <= targetDate
                         && d.WarrantyExpiration >= pastGracePeriod)
                .Select(d => new AttentionItemDto
                {
                    ItemId = d.ItemId,
                    ItemName = d.Item!.Name,
                    LocationName = d.Item.StorageLocation != null ? d.Item.StorageLocation.Name : string.Empty,
                    TypeKey = LocalizationConstants.REMINDERS.WARRANTY,
                    Date = d.WarrantyExpiration!.Value,
                    Severity = d.WarrantyExpiration < request.NowUtc ? "danger" : "warning"
                })
                .ToListAsync(cancellationToken);

            return new LendingsAndWarrantiesStatsDto
            {
                ExpiredLendingsCount = expiredLendings,
                ExpiringLendingsCount = expiringLendings,
                ExpiringWarrantiesCount = expiringWarranties,
                Items = lendingsList.Concat(warrantyList).ToList()
            };
        }
    }
}