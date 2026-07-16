using Ganss.Xss;
using System.Text.RegularExpressions;

namespace INest.Infrastructure.Sanitizer
{
    public class SanitizerService : ISanitizerService
    {
        private readonly HtmlSanitizer _sanitizer;

        private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);

        public SanitizerService()
        {
            _sanitizer = new HtmlSanitizer();
        }

        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return _sanitizer.Sanitize(input);
        }

        public string StripAllHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return HtmlTagRegex.Replace(input, string.Empty).Trim();
        }
    }
}
