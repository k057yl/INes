using INest.Features.Telegram.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace INest.Features.Telegram.Queries.SearchItems
{
    public class SearchItemsHandler : IRequestHandler<SearchItemsQuery, List<ItemSearchResultDto>>
    {
        private readonly AppDbContext _context;

        public SearchItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ItemSearchResultDto>> Handle(SearchItemsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TelegramChatId == request.ChatId, cancellationToken);

            if (user == null) return new List<ItemSearchResultDto>();

            var term = request.SearchTerm.ToLower().Trim();

            var items = await _context.Items
                .Include(i => i.StorageLocation)
                .Where(i => i.UserId == user.Id && !i.IsDeleted)
                .Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{term}%")
                         || (i.Description != null && EF.Functions.Like(i.Description.ToLower(), $"%{term}%")))
                .Take(5)
                .Select(i => new ItemSearchResultDto(
                    i.Name,
                    i.StorageLocation != null ? i.StorageLocation.Name : "Не указано",
                    i.Description
                ))
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
