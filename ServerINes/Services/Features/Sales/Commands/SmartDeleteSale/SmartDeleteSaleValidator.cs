using FluentValidation;

namespace INest.Services.Features.Sales.Commands.SmartDeleteSale
{
    public class SmartDeleteSaleValidator : AbstractValidator<SmartDeleteSaleCommand>
    {
        public SmartDeleteSaleValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.SaleId).NotEmpty();
        }
    }
}
