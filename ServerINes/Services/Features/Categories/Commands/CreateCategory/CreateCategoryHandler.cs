using Ganss.Xss;
using INest.Exceptions;
using INest.Models.Entities;
using INest.Services.Tracker;
using MediatR;
using static INest.Constants.LocalizationConstants;

namespace INest.Services.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Category>
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;
        private readonly ICacheTracker _tracker;

        public CreateCategoryHandler(AppDbContext context, IHtmlSanitizer sanitizer, ICacheTracker tracker)
        {
            _context = context;
            _sanitizer = sanitizer;
            _tracker = tracker;
        }

        public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var sanitizedName = _sanitizer.Sanitize(request.Dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(CATEGORIES.ERRORS.INVALID_NAME, 400);

            var cat = new Category
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = sanitizedName,
                Color = request.Dto.Color ?? "#007bff",
                ParentCategoryId = request.Dto.ParentCategoryId
            };

            _context.Categories.Add(cat);
            await _context.SaveChangesAsync(cancellationToken);

            _tracker.InvalidateUserCache(request.UserId);
            return cat;
        }
    }
}
