using INest.Constants;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICacheTracker _tracker;

        public DeleteCategoryHandler(AppDbContext context, ICacheTracker tracker)
        {
            _context = context;
            _tracker = tracker;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId, cancellationToken);

            if (category == null)
                throw new KeyNotFoundException(CATEGORIES.ERRORS.NOT_FOUND);

            if (category.Name == SharedConstants.CATEGORY_OTHER)
                throw new InvalidOperationException(CATEGORIES.ERRORS.CANNOT_DELETE_DEFAULT);

            if (category.Items.Any())
            {
                Guid targetId;

                if (request.TargetCategoryId.HasValue && request.TargetCategoryId.Value != request.CategoryId)
                {
                    var targetExists = await _context.Categories.AnyAsync(c => c.Id == request.TargetCategoryId.Value && c.UserId == request.UserId, cancellationToken);
                    targetId = targetExists ? request.TargetCategoryId.Value : await GetOrCreateDefaultCategoryId(request.UserId, cancellationToken);
                }
                else
                {
                    targetId = await GetOrCreateDefaultCategoryId(request.UserId, cancellationToken);
                }

                foreach (var item in category.Items)
                {
                    item.CategoryId = targetId;
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return true;
        }

        private async Task<Guid> GetOrCreateDefaultCategoryId(Guid userId, CancellationToken cancellationToken)
        {
            var defaultCat = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == SharedConstants.CATEGORY_OTHER, cancellationToken);

            if (defaultCat == null)
            {
                defaultCat = new INest.Models.Entities.Category
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = SharedConstants.CATEGORY_OTHER,
                    Color = "#64748b"
                };
                _context.Categories.Add(defaultCat);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return defaultCat.Id;
        }
    }
}
