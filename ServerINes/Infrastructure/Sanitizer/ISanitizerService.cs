namespace INest.Infrastructure.Sanitizer
{
    public interface ISanitizerService
    {
        string SanitizeHtml(string input);
        string StripAllHtml(string input);
    }
}
