namespace INest.Features.Lendings.DTOs
{
    public record LendItemDto(
        Guid ItemId,
        string PersonName,
        DateTime? ExpectedReturnDate,
        string? Comment,
        decimal? ValueAtLending,
        string? ContactEmail,
        bool SendNotification,
        int Direction
    );
}
