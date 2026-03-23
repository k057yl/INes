namespace INest.Models.DTOs.Lending
{
    public record LendItemDto(Guid ItemId, string PersonName, DateTime? ExpectedReturnDate, string? Comment);
}
