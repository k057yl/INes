namespace INest.Models.DTOs.Auth
{
    public record ConfirmRegisterDto(string Email, string Code, string Password);
}
