using INest.Services.Interfaces;
using Microsoft.Extensions.Localization;

namespace INest.Services
{
    public class ItemService : IItemService
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ItemService(IStringLocalizer<SharedResource> localizer) 
        {
            _localizer = localizer;
        }
    }
}
