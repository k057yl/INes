using FluentValidation;

namespace INest.Services.Features.Items.Commands.CancelSale
{
    public class CancelSaleValidator : AbstractValidator<CancelSaleCommand>
    {
        public CancelSaleValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
        }
    }
}
