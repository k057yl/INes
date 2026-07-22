using INest.Data.Enums;
using INest.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly AppDbContext _context;

        public GetDashboardStatsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            var warningThreshold = nowUtc.AddDays(3);

            var totalItems = await _context.Items
                .AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold, cancellationToken);

            var totalLocations = await _context.StorageLocations
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId, cancellationToken);

            var expiredReminders = await _context.Reminders
                .AsNoTracking()
                .CountAsync(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.TriggerAt <= nowUtc && r.Item.Status != ItemStatus.Archived, cancellationToken);

            var expiredLendings = await _context.Lendings
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null && l.ExpectedReturnDate.HasValue && l.ExpectedReturnDate.Value <= nowUtc, cancellationToken);

            var expiringWarranties = await _context.ItemDetails
                .AsNoTracking()
                .CountAsync(d => d.Item!.UserId == request.UserId && d.WarrantyExpiration != null && d.WarrantyExpiration <= nowUtc.AddDays(30) && d.WarrantyExpiration >= nowUtc && d.Item.Status != ItemStatus.Archived, cancellationToken);

            var expiringLendings = await _context.Lendings
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null && l.ExpectedReturnDate.HasValue && l.ExpectedReturnDate.Value >= nowUtc && l.ExpectedReturnDate.Value <= nowUtc.AddDays(30), cancellationToken);

            var activeReminders = await _context.Reminders
                .AsNoTracking()
                .CountAsync(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.TriggerAt > nowUtc && r.Item.Status != ItemStatus.Archived, cancellationToken);

            var lentCount = await _context.Lendings
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null, cancellationToken);

            var soldCount = await _context.Items
                .AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId && i.Status == ItemStatus.Sold, cancellationToken);

            var remindersList = await _context.Reminders
                .AsNoTracking()
                .Where(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.Item.Status != ItemStatus.Archived)
                .Select(r => new AttentionItemDto
                {
                    ItemId = r.ItemId,
                    ItemName = r.Item!.Name,
                    LocationName = r.Item.StorageLocation != null ? r.Item.StorageLocation.Name : string.Empty,
                    TypeKey = r.Type == ReminderType.ReturnItem ? "DASHBOARD_STATS.LENT" :
                              r.Type == ReminderType.Warranty ? "DASHBOARD_STATS.WARRANTY_SHORT" : "DASHBOARD_STATS.ATTENTION",
                    Date = r.TriggerAt,
                    Severity = r.TriggerAt < nowUtc ? "danger" : (r.TriggerAt <= warningThreshold ? "warning" : "info")
                })
                .ToListAsync(cancellationToken);

            var warrantyList = await _context.ItemDetails
                .AsNoTracking()
                .Where(d => d.Item!.UserId == request.UserId && d.WarrantyExpiration != null && d.Item.Status != ItemStatus.Archived && d.WarrantyExpiration <= nowUtc.AddDays(30))
                .Select(d => new AttentionItemDto
                {
                    ItemId = d.ItemId,
                    ItemName = d.Item!.Name,
                    LocationName = d.Item.StorageLocation != null ? d.Item.StorageLocation.Name : string.Empty,
                    TypeKey = "DASHBOARD_STATS.WARRANTY_SHORT",
                    Date = d.WarrantyExpiration!.Value,
                    Severity = d.WarrantyExpiration < nowUtc ? "danger" : "warning"
                })
                .ToListAsync(cancellationToken);

            var attentionItems = remindersList
                .Concat(warrantyList)
                .OrderBy(x => x.Date)
                .ToList();

            return new DashboardStatsDto
            {
                TotalItemsCount = totalItems,
                TotalLocationsCount = totalLocations,
                ExpiredRemindersCount = expiredReminders + expiredLendings,
                ExpiringWarrantiesCount = expiringWarranties + expiringLendings,
                ActiveRemindersCount = activeReminders,
                LentItemsCount = lentCount,
                SoldItemsCount = soldCount,
                AttentionItems = attentionItems
            };
        }
    }
}