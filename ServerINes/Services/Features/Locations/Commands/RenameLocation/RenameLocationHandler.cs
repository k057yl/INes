using Ganss.Xss;
using INest.Exceptions;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Locations.Commands.RenameLocation
{
    public class RenameLocationHandler : IRequestHandler<RenameLocationCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public RenameLocationHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<bool> Handle(RenameLocationCommand request, CancellationToken cancellationToken)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId && l.UserId == request.UserId, cancellationToken);

            if (location == null)
                throw new KeyNotFoundException(LOCATIONS.ERRORS.NOT_FOUND);

            var sanitizedName = _sanitizer.Sanitize(request.NewName);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new AppException(LOCATIONS.ERRORS.INVALID_NAME, 400);
            }

            location.Name = sanitizedName;
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}