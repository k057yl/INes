using INest.Data.Entities.Core;
using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Queries.GetLocationTree
{
    public class GetLocationTreeHandler : IRequestHandler<GetLocationTreeQuery, List<StorageLocation>>
    {
        private readonly AppDbContext _context;

        public GetLocationTreeHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StorageLocation>> Handle(GetLocationTreeQuery request, CancellationToken cancellationToken)
        {
            return await _context.StorageLocations
                .Where(l => l.UserId == request.UserId)
                .OrderBy(l => l.SortOrder)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}