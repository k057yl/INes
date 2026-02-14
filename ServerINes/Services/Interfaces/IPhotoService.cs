using CloudinaryDotNet.Actions;

namespace INest.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    }
}
