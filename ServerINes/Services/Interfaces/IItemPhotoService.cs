using INest.Models.Entities;

namespace INest.Services.Interfaces
{
    public interface IItemPhotoService
    {
        Task<ItemPhoto> UploadPhotoAsync(Guid userId, Guid itemId, IFormFile file, bool isMain = false);
        Task<IEnumerable<ItemPhoto>> GetPhotosAsync(Guid userId, Guid itemId);
        Task<bool> DeletePhotoAsync(Guid userId, Guid photoId);
    }
}
