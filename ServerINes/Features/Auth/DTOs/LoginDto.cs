namespace INest.Features.Auth.DTOs
{
    public record LoginDto(string Email, string Password, string? TimeZoneId = null);
}
