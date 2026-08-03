using INest.Features.Dashboard.DTOs;
using INest.Features.Dashboard.Queries.GetGeneralStats;
using INest.Features.Dashboard.Queries.GetLendingsAndWarrantiesStats;
using INest.Features.Dashboard.Queries.GetRemindersStats;
using MediatR;

namespace INest.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IMediator _mediator;

        public GetDashboardStatsQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            var general = await _mediator.Send(new GetGeneralStatsQuery(request.UserId), cancellationToken);
            var reminders = await _mediator.Send(new GetRemindersStatsQuery(request.UserId, nowUtc), cancellationToken);
            var lendings = await _mediator.Send(new GetLendingsAndWarrantiesStatsQuery(request.UserId, nowUtc), cancellationToken);

            var attentionItems = reminders.Items
                .Concat(lendings.Items)
                .OrderByDescending(x => x.Severity == "danger")
                .ThenBy(x => x.Date)
                .ToList();

            return new DashboardStatsDto
            {
                TotalItemsCount = general.TotalItemsCount,
                TotalLocationsCount = general.TotalLocationsCount,
                ExpiredRemindersCount = reminders.ExpiredCount + lendings.ExpiredLendingsCount,
                ExpiringWarrantiesCount = lendings.ExpiringWarrantiesCount + lendings.ExpiringLendingsCount,
                ActiveRemindersCount = reminders.ActiveCount,
                LentItemsCount = general.LentItemsCount,
                SoldItemsCount = general.SoldItemsCount,
                AttentionItems = attentionItems
            };
        }
    }
}