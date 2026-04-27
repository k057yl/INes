using INest.Models.Entities;
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
            var all = await _context.StorageLocations
                .Where(l => l.UserId == request.UserId)
                .AsNoTracking()
                .Include(l => l.Items.Where(i => i.Status != ItemStatus.Sold))
                    .ThenInclude(i => i.Category)
                .ToListAsync(cancellationToken);

            return BuildTree(all, null);
        }

        private List<StorageLocation> BuildTree(List<StorageLocation> all, Guid? parentId)
        {
            return all
                .Where(l => l.ParentLocationId == parentId)
                .OrderBy(l => l.SortOrder)
                .Select(l => {
                    l.Children = BuildTree(all, l.Id);
                    return l;
                })
                .ToList();
        }
    }
}