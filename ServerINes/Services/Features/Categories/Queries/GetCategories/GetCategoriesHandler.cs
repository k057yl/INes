using INest.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<Category>>
    {
        private readonly AppDbContext _context;

        public GetCategoriesHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .Where(c => c.UserId == request.UserId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
