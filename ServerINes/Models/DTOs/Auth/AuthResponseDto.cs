namespace INest.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public bool IsEmailConfirmed { get; set; } = true;
        public string? Message { get; set; }
        public bool Success { get; set; } = true;
    }
}
