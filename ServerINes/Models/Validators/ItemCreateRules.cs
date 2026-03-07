using FluentValidation;
using INest.Models.DTOs.Item;

namespace INest.Models.Validators
{
    public class ItemCreateRules : AbstractValidator<CreateItemDto>
    {
        public ItemCreateRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.StorageLocationId).NotEmpty();

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PurchasePrice.HasValue);

            RuleFor(x => x.PurchaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.PurchaseDate.HasValue);
        }
    }
}
