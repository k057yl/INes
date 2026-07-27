using INest.Data.Enums;
using INest.Features.Feedback.DTOs;
using MediatR;

namespace INest.Features.Feedback.Queries.GetFeedbacks
{
    public record GetFeedbacksQuery(
        int Page = 1,
        int PageSize = 20,
        bool? IsProcessed = null,
        FeedbackType? Type = null) : IRequest<PagedFeedbackResultDto>;
}
