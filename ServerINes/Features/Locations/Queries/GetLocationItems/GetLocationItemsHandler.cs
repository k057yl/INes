using INest.Data.Enums;
using INest.Features.Items.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Locations.Queries.GetLocationItems
{
    public class GetLocationItemsHandler : IRequestHandler<GetLocationItemsQuery, IEnumerable<ItemDto>>
    {
        private readonly AppDbContext _context;

        public GetLocationItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemDto>> Handle(GetLocationItemsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Items
                .Where(i => i.UserId == request.UserId && i.StorageLocationId == request.LocationId && i.Status != ItemStatus.Sold)
                .AsNoTracking()
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Status = i.Status,
                    PhotoUrl = i.PhotoUrl,
                    StorageLocationId = i.StorageLocationId,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.Name : null
                })
                .ToListAsync(cancellationToken);
        }
    }
}
