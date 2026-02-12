namespace INest.Models.DTOs.ItemPhoto
{
    public class UploadItemPhotoDto
    {
        public Guid ItemId { get; set; }
        public IFormFile File { get; set; } = null!;
        public bool IsMain { get; set; } = false;
    }
}
