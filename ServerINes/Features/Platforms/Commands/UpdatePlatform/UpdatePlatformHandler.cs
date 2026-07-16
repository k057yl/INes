using INest.Data.Entities.Finances;
using INest.Exceptions;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Platforms.Commands.UpdatePlatform
{
    public class UpdatePlatformHandler : IRequestHandler<UpdatePlatformCommand, Platform>
    {
        private readonly AppDbContext _context;
        private readonly ISanitizerService _sanitizer;
        private readonly ICacheTracker _tracker;

        public UpdatePlatformHandler(AppDbContext context, ISanitizerService sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Platform> Handle(UpdatePlatformCommand request, CancellationToken cancellationToken)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == request.PlatformId && p.UserId == request.UserId, cancellationToken);

            if (platform == null)
                throw new KeyNotFoundException(PLATFORMS.ERRORS.NOT_FOUND);

            var sanitizedName = _sanitizer.StripAllHtml(request.Dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(PLATFORMS.ERRORS.INVALID_NAME, 400);

            platform.Name = sanitizedName;
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return platform;
        }
    }
}