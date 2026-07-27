using INest.Data.Enums;

namespace INest.Features.Feedback.DTOs
{
    public record FeedbackItemDto(
        Guid Id,
        string UserName,
        string UserEmail,
        FeedbackType Type,
        string Message,
        int? Rating,
        string? MissingFeatures,
        DateTime CreatedAt,
        bool IsProcessed);
}
