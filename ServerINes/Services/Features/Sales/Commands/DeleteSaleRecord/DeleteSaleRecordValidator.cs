using FluentValidation;

namespace INest.Services.Features.Sales.Commands.DeleteSaleRecord
{
    public class DeleteSaleRecordValidator : AbstractValidator<DeleteSaleRecordCommand>
    {
        public DeleteSaleRecordValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.SaleId).NotEmpty();
        }
    }
}
