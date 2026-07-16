using CloudinaryDotNet.Actions;

namespace INest.Infrastructure.Storage
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file, Guid? userId = null);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
