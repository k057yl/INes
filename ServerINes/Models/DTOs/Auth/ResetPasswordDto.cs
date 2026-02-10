namespace INest.Models.DTOs.Auth
{
    public record ResetPasswordDto(string Email, string Token, string NewPassword);
}
