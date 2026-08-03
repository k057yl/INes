using INest.Infrastructure.Sanitizer;
using Shouldly;

namespace INest.Tests.Infrastructure.Sanitizer
{
    public class SanitizerServiceTests
    {
        private readonly SanitizerService _sanitizer = new();

        [Fact]
        public void StripAllHtml_ShouldRemoveTagsAndTrimWhitespace()
        {
            // Arrange
            var input = "  <p>Привет, <b>Мир!</b></p>  ";

            // Act
            var result = _sanitizer.StripAllHtml(input);

            // Assert
            result.ShouldBe("Привет, Мир!");
        }

        [Fact]
        public void SanitizeHtml_ShouldRemoveScriptTags()
        {
            // Arrange
            var input = "<div>Hello<script>alert('XSS')</script></div>";

            // Act
            var result = _sanitizer.SanitizeHtml(input);

            // Assert
            result.ShouldNotContain("<script>");
            result.ShouldNotContain("alert");
        }
    }
}