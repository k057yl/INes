using INest.Features.Telegram.Dtos;
using MediatR;

namespace INest.Features.Telegram.Queries.SearchItems
{
    public record SearchItemsQuery(long ChatId, string SearchTerm) : IRequest<List<ItemSearchResultDto>>;
}
