namespace INest.Features.Auth.DTOs
{
    public class GetMeResponseDto
    {
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }
}
