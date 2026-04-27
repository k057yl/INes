using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Services.Tracker;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Category>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public UpdateCategoryHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Category> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId, cancellationToken);

            if (category == null)
                throw new KeyNotFoundException(CATEGORIES.ERRORS.NOT_FOUND);

            if (request.Dto.ParentCategoryId == request.CategoryId)
                throw new AppException(SYSTEM.ERRORS.VALIDATION_FAILED, 400);

            var sanitizedName = _sanitizer.Sanitize(request.Dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(CATEGORIES.ERRORS.INVALID_NAME, 400);

            category.Name = sanitizedName;
            if (request.Dto.Color != null) category.Color = request.Dto.Color;
            category.ParentCategoryId = request.Dto.ParentCategoryId;

            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return category;
        }
    }
}
