namespace INest.Features.Items.DTOs
{
    public record ItemHistoryDto(
        Guid Id,
        int Type,
        string? OldValue,
        string? NewValue,
        string? Comment,
        DateTime CreatedAt
    );
}
