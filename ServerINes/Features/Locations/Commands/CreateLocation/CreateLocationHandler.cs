using INest.Data.Entities.Core;
using INest.Exceptions;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Locations.Commands.CreateLocation
{
    public class CreateLocationHandler : IRequestHandler<CreateLocationCommand, StorageLocation>
    {
        private readonly AppDbContext _context;
        private readonly ISanitizerService _sanitizer;
        private readonly ICacheTracker _tracker;

        public CreateLocationHandler(AppDbContext context, ISanitizerService sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<StorageLocation> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (dto.ParentLocationId.HasValue)
            {
                var parentExists = await _context.StorageLocations
                    .AnyAsync(l => l.Id == dto.ParentLocationId && l.UserId == request.UserId, cancellationToken);

                if (!parentExists)
                    throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);
            }

            var sanitizedName = _sanitizer.StripAllHtml(dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new AppException(LOCATIONS.ERRORS.INVALID_NAME, 400);
            }

            var safeDescription = !string.IsNullOrEmpty(dto.Description)
                ? _sanitizer.SanitizeHtml(dto.Description)
                : null;

            var location = new StorageLocation
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = sanitizedName,
                Description = safeDescription,
                ParentLocationId = dto.ParentLocationId,
                SortOrder = dto.SortOrder,
                Color = dto.Color ?? "#007bff",
                Icon = dto.Icon ?? "fa-folder",
                CreatedAt = DateTime.UtcNow,
                IsSalesLocation = dto.IsSalesLocation,
                IsLendingLocation = dto.IsLendingLocation
            };

            _context.StorageLocations.Add(location);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return location;
        }
    }
}