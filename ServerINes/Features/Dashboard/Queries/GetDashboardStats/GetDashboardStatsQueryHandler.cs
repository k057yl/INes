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

            var totalItems = await _context.Items
                .AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId
                              && i.Status != ItemStatus.Archived
                              && i.Status != ItemStatus.Sold, cancellationToken);

            var totalLocations = await _context.StorageLocations
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId, cancellationToken);

            var expiredReminders = await _context.Reminders
                .AsNoTracking()
                .CountAsync(r => r.Item!.UserId == request.UserId
                              && !r.IsCompleted
                              && r.TriggerAt <= nowUtc
                              && r.Item.Status != ItemStatus.Archived, cancellationToken);

            var expiringWarranties = await _context.ItemDetails
                .AsNoTracking()
                .CountAsync(d => d.Item!.UserId == request.UserId
                              && d.WarrantyExpiration != null
                              && d.WarrantyExpiration <= nowUtc.AddDays(30)
                              && d.WarrantyExpiration >= nowUtc
                              && d.Item.Status != ItemStatus.Archived, cancellationToken);

            var lentCount = await _context.Lendings
                .AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null, cancellationToken);

            var archivedAndSoldCount = await _context.Items
                .AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId
                              && (i.Status == ItemStatus.Archived || i.Status == ItemStatus.Sold), cancellationToken);

            return new DashboardStatsDto
            {
                TotalItemsCount = totalItems,
                TotalLocationsCount = totalLocations,
                ExpiredRemindersCount = expiredReminders,
                ExpiringWarrantiesCount = expiringWarranties,
                LentItemsCount = lentCount,
                ArchivedAndSoldItemsCount = archivedAndSoldCount
            };
        }
    }
}