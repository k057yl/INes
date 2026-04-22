using Ganss.Xss;
using INest.Exceptions;
using INest.Models.DTOs.Platform;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Services
{
    public class PlatformService : IPlatformService
    {
        private readonly AppDbContext _context;
        private readonly IHtmlSanitizer _sanitizer;

        public PlatformService(AppDbContext context, IHtmlSanitizer sanitizer)
        {
            _context = context;
            _sanitizer = sanitizer;
        }

        public async Task<IEnumerable<Platform>> GetAllAsync(Guid userId)
        {
            return await _context.Platforms
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Platform> CreateAsync(Guid userId, PlatformDto dto)
        {
            var sanitizedName = _sanitizer.Sanitize(dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(PLATFORMS.ERRORS.INVALID_NAME, 400);

            var platform = new Platform
            {
                Id = Guid.NewGuid(),
                Name = sanitizedName,
                UserId = userId
            };

            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();
            return platform;
        }

        public async Task<Platform> UpdateAsync(Guid userId, Guid id, PlatformDto dto)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (platform == null)
                throw new KeyNotFoundException(PLATFORMS.ERRORS.NOT_FOUND);

            var sanitizedName = _sanitizer.Sanitize(dto.Name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
                throw new AppException(PLATFORMS.ERRORS.INVALID_NAME, 400);

            platform.Name = sanitizedName;
            await _context.SaveChangesAsync();
            return platform;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid id)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (platform == null)
                throw new KeyNotFoundException(PLATFORMS.ERRORS.NOT_FOUND);

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}