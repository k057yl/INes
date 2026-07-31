using INest.Data.Enums;
using INest.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Dashboard.Queries.GetRemindersStats
{
    public class GetRemindersStatsQueryHandler : IRequestHandler<GetRemindersStatsQuery, RemindersStatsDto>
    {
        private readonly AppDbContext _context;

        public GetRemindersStatsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RemindersStatsDto> Handle(GetRemindersStatsQuery request, CancellationToken cancellationToken)
        {
            var warningThreshold = request.NowUtc.AddDays(3);

            var expiredCount = await _context.Reminders.AsNoTracking()
                .CountAsync(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.TriggerAt <= request.NowUtc && r.Item.Status != ItemStatus.Archived, cancellationToken);

            var activeCount = await _context.Reminders.AsNoTracking()
                .CountAsync(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.TriggerAt > request.NowUtc && r.Item.Status != ItemStatus.Archived, cancellationToken);

            var items = await _context.Reminders.AsNoTracking()
                .Where(r => r.Item!.UserId == request.UserId && !r.IsCompleted && r.Item.Status != ItemStatus.Archived)
                .Select(r => new AttentionItemDto
                {
                    ItemId = r.ItemId,
                    ItemName = r.Item!.Name,
                    LocationName = r.Item.StorageLocation != null ? r.Item.StorageLocation.Name : string.Empty,
                    TypeKey = r.Type == ReminderType.ReturnItem ? "DASHBOARD_STATS.LENT" :
                              r.Type == ReminderType.Warranty ? "DASHBOARD_STATS.WARRANTY_SHORT" : "DASHBOARD_STATS.ATTENTION",
                    Date = r.TriggerAt,
                    Severity = r.TriggerAt < request.NowUtc ? "danger" : (r.TriggerAt <= warningThreshold ? "warning" : "info")
                })
                .ToListAsync(cancellationToken);

            return new RemindersStatsDto
            {
                ExpiredCount = expiredCount,
                ActiveCount = activeCount,
                Items = items
            };
        }
    }
}
