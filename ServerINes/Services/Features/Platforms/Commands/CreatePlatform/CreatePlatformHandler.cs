using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Services.Tracker;
using MediatR;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Platforms.Commands.CreatePlatform
{
    public class CreatePlatformHandler : IRequestHandler<CreatePlatformCommand, Platform>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public CreatePlatformHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Platform> Handle(CreatePlatformCommand request, CancellationToken cancellationToken)
        {
            var sanitizedName = _sanitizer.Sanitize(request.Dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(PLATFORMS.ERRORS.INVALID_NAME, 400);

            var platform = new Platform
            {
                Id = Guid.NewGuid(),
                Name = sanitizedName,
                UserId = request.UserId
            };

            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return platform;
        }
    }
}
