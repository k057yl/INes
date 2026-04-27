using FluentValidation;

namespace INest.Services.Features.Items.Commands.MoveItem
{
    public class MoveItemValidator : AbstractValidator<MoveItemCommand>
    {
        public MoveItemValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
        }
    }
}