using System.ComponentModel.DataAnnotations;

namespace INest.Features.Locations.DTOs
{
    public class RenameLocationDto
    {
        [Required(ErrorMessage = "Localization_LocationNameRequired")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Localization_LocationNameLength")]
        public string Name { get; set; } = string.Empty;
    }
}
