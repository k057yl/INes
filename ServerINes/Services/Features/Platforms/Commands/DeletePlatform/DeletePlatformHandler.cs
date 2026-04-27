using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Platforms.Commands.DeletePlatform
{
    public class DeletePlatformHandler : IRequestHandler<DeletePlatformCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeletePlatformHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeletePlatformCommand request, CancellationToken cancellationToken)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == request.PlatformId && p.UserId == request.UserId, cancellationToken);

            if (platform == null)
                throw new KeyNotFoundException(PLATFORMS.ERRORS.NOT_FOUND);

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }
    }
}
