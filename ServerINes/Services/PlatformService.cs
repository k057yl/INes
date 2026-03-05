using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services
{
    public class PlatformService : IPlatformService
    {
        private readonly AppDbContext _context;

        public PlatformService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Platform>> GetAllAsync(Guid userId)
        {
            return await _context.Platforms
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Platform> CreateAsync(Guid userId, string name)
        {
            var platform = new Platform
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = userId
            };

            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();
            return platform;
        }

        public async Task<Platform?> UpdateAsync(Guid userId, Guid id, string name)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (platform == null) return null;

            platform.Name = name;
            await _context.SaveChangesAsync();
            return platform;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid id)
        {
            var platform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (platform == null) return false;

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
