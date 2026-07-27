namespace INest.Features.Feedback.DTOs
{
    public record PagedFeedbackResultDto(
        List<FeedbackItemDto> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
