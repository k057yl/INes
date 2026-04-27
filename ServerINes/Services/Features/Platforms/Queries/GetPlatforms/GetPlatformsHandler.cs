using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Platforms.Queries.GetPlatforms
{
    public class GetPlatformsHandler : IRequestHandler<GetPlatformsQuery, IEnumerable<Platform>>
    {
        private readonly AppDbContext _context;

        public GetPlatformsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Platform>> Handle(GetPlatformsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Platforms
                .Where(p => p.UserId == request.UserId)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
