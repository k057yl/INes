using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class ItemPhotoService : IItemPhotoService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ItemPhotoService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<ItemPhoto> UploadPhotoAsync(Guid userId, Guid itemId, IFormFile file, bool isMain = false)
        {
            var item = await _context.Items
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

            if (item == null)
                throw new InvalidOperationException("Item not found");

            var folder = Path.Combine(_env.ContentRootPath, "Uploads", "ItemPhotos");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            if (isMain)
            {
                foreach (var p in item.Photos)
                    p.IsMain = false;
            }

            var photo = new ItemPhoto
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                FilePath = $"/uploads/ItemPhotos/{fileName}",
                IsMain = isMain,
                UploadedAt = DateTime.UtcNow
            };

            _context.ItemPhotos.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }

        public async Task<IEnumerable<ItemPhoto>> GetPhotosAsync(Guid userId, Guid itemId)
        {
            return await _context.ItemPhotos
                .Include(p => p.Item)
                .Where(p => p.ItemId == itemId && p.Item.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> DeletePhotoAsync(Guid userId, Guid photoId)
        {
            var photo = await _context.ItemPhotos
                .Include(p => p.Item)
                .FirstOrDefaultAsync(p => p.Id == photoId && p.Item.UserId == userId);

            if (photo == null) return false;

            var fullPath = Path.Combine(_env.ContentRootPath, photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath)) File.Delete(fullPath);

            _context.ItemPhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
