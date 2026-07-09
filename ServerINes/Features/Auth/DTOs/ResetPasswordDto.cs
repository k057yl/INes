namespace INest.Features.Auth.DTOs
{
    public record ResetPasswordDto(string Email, string Token, string NewPassword);
}
