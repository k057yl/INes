using INest.Data.Enums;
using INest.Features.Items.DTOs;
using INest.Features.Locations.DTOs;
using INest.Features.Reminders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Locations.Queries.GetLocationTree
{
    public class GetLocationTreeHandler : IRequestHandler<GetLocationTreeQuery, List<StorageLocationTreeDto>>
    {
        private readonly AppDbContext _context;

        public GetLocationTreeHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StorageLocationTreeDto>> Handle(
            GetLocationTreeQuery request,
            CancellationToken cancellationToken)
        {
            var locations = await _context.StorageLocations
                .Where(x => x.UserId == request.UserId)
                .Include(x => x.Items.Where(i => i.Status != ItemStatus.Sold))
                    .ThenInclude(i => i.Details)
                .Include(x => x.Items.Where(i => i.Status != ItemStatus.Sold))
                    .ThenInclude(i => i.Reminders)
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            var dtoList = locations
                .Select(x => new StorageLocationTreeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Color = x.Color,
                    Icon = x.Icon,
                    ParentLocationId = x.ParentLocationId,
                    SortOrder = x.SortOrder,
                    IsSalesLocation = x.IsSalesLocation,
                    IsLendingLocation = x.IsLendingLocation,

                    Items = x.Items
                .Select(i => new ItemTreeDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    PhotoUrl = i.PhotoUrl,
                    Status = i.Status,
                    Details = i.Details != null ? new ItemFinanceDto
                    {
                        PurchasePrice = i.Details.PurchasePrice,
                        EstimatedValue = i.Details.EstimatedValue,
                        Currency = i.Details.Currency ?? "USD",
                        PurchaseDate = i.Details.PurchaseDate
                    } : null,

                    Reminders = i.Reminders.Select(r => new ReminderDto
                    {
                        Id = r.Id,
                        ItemId = r.ItemId,
                        Type = r.Type,
                        Recurrence = r.Recurrence,
                        TriggerAt = r.TriggerAt,
                        IsCompleted = r.IsCompleted
                    }).ToList()
                })
                .ToList()
                })
                .ToList();

            var lookup = dtoList.ToDictionary(x => x.Id);
            var roots = new List<StorageLocationTreeDto>();

            foreach (var location in dtoList)
            {
                if (location.ParentLocationId is Guid parentId &&
                    lookup.TryGetValue(parentId, out var parent))
                {
                    parent.Children.Add(location);
                }
                else
                {
                    roots.Add(location);
                }
            }

            SortTree(roots);
            return roots;
        }

        private static void SortTree(List<StorageLocationTreeDto> locations)
        {
            locations.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            foreach (var location in locations)
            {
                SortTree(location.Children);
            }
        }
    }
}