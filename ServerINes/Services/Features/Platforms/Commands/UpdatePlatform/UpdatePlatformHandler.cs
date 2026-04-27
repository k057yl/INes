using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Platforms.Commands.UpdatePlatform
{
    public class UpdatePlatformHandler : IRequestHandler<UpdatePlatformCommand, Platform>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public UpdatePlatformHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
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

            var sanitizedName = _sanitizer.Sanitize(request.Dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(PLATFORMS.ERRORS.INVALID_NAME, 400);

            platform.Name = sanitizedName;
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return platform;
        }
    }
}
