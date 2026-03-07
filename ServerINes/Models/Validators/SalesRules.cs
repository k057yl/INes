using FluentValidation;
using INest.Models.DTOs.Sale;

namespace INest.Models.Validators
{
    public class SalesRules : AbstractValidator<SellItemRequestDto>
    {
        public SalesRules()
        {
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.SalePrice).GreaterThan(0).WithMessage("Цена должна быть больше нуля");
            RuleFor(x => x.SoldDate).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Дата не может быть в будущем");
        }
    }
}
