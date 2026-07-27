using INest.Data.Enums;

namespace INest.Features.Feedback.DTOs
{
    public record CreateFeedbackDto(FeedbackType Type, string Message);
}
