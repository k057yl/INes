using INest.Features.Feedback.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Feedback.Queries.GetFeedbacks
{
    public class GetFeedbacksHandler : IRequestHandler<GetFeedbacksQuery, PagedFeedbackResultDto>
    {
        private readonly AppDbContext _context;

        public GetFeedbacksHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedFeedbackResultDto> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Feedbacks
                .AsNoTracking()
                .Include(x => x.User)
                .AsQueryable();

            if (request.IsProcessed.HasValue)
            {
                query = query.Where(x => x.IsProcessed == request.IsProcessed.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new FeedbackItemDto(
                    x.Id,
                    x.User.DisplayName ?? x.User.Email ?? "Unknown",
                    x.User.Email ?? "",
                    x.Type,
                    x.Message,
                    x.Rating,
                    x.MissingFeatures,
                    x.CreatedAt,
                    x.IsProcessed
                ))
                .ToListAsync(cancellationToken);

            return new PagedFeedbackResultDto(items, totalCount, request.Page, request.PageSize);
        }
    }
}
