using INest.Models.Entities;
using INest.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Locations.Queries.GetLocationById
{
    public class GetLocationByIdHandler : IRequestHandler<GetLocationByIdQuery, StorageLocation?>
    {
        private readonly AppDbContext _context;

        public GetLocationByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StorageLocation?> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.StorageLocations
                .Where(l => l.UserId == request.UserId && l.Id == request.LocationId)
                .Select(l => new
                {
                    Location = l,
                    CurrentItemsCount = l.Items.Count(i => i.Status != ItemStatus.Sold),
                    ChildrenData = l.Children.Select(c => new
                    {
                        Child = c,
                        Count = c.Items.Count(i => i.Status != ItemStatus.Sold)
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null) return null;

            var location = data.Location;
            location.ItemsCount = data.CurrentItemsCount;

            location.Children = data.ChildrenData.Select(cd =>
            {
                cd.Child.ItemsCount = cd.Count;
                return cd.Child;
            }).ToList();

            location.ParentLocation = await GetParentChainAsync(request.UserId, location.ParentLocationId, cancellationToken);

            await _context.Entry(location)
                .Collection(l => l.Items)
                .Query()
                .Where(i => i.Status != ItemStatus.Sold)
                .Include(i => i.Category)
                .LoadAsync(cancellationToken);

            return location;
        }

        private async Task<StorageLocation?> GetParentChainAsync(Guid userId, Guid? parentId, CancellationToken cancellationToken)
        {
            if (!parentId.HasValue) return null;

            var parent = await _context.StorageLocations
                .Where(l => l.UserId == userId && l.Id == parentId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (parent != null)
            {
                parent.ParentLocation = await GetParentChainAsync(userId, parent.ParentLocationId, cancellationToken);
            }

            return parent;
        }
    }
}