using INest.Data.Enums;
using INest.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Dashboard.Queries.GetGeneralStats
{
    public class GetGeneralStatsQueryHandler : IRequestHandler<GetGeneralStatsQuery, GeneralStatsDto>
    {
        private readonly AppDbContext _context;

        public GetGeneralStatsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GeneralStatsDto> Handle(GetGeneralStatsQuery request, CancellationToken cancellationToken)
        {
            var totalItems = await _context.Items.AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId && i.Status != ItemStatus.Archived && i.Status != ItemStatus.Sold, cancellationToken);

            var totalLocations = await _context.StorageLocations.AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId, cancellationToken);

            var lentCount = await _context.Lendings.AsNoTracking()
                .CountAsync(l => l.UserId == request.UserId && l.ReturnedDate == null, cancellationToken);

            var soldCount = await _context.Items.AsNoTracking()
                .CountAsync(i => i.UserId == request.UserId && i.Status == ItemStatus.Sold, cancellationToken);

            return new GeneralStatsDto
            {
                TotalItemsCount = totalItems,
                TotalLocationsCount = totalLocations,
                LentItemsCount = lentCount,
                SoldItemsCount = soldCount
            };
        }
    }
}
