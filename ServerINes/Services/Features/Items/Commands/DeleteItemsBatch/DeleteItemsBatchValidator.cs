using FluentValidation;
using INest.Constants;

namespace INest.Services.Features.Items.Commands.DeleteItemsBatch
{
    public class DeleteItemsBatchValidator : AbstractValidator<DeleteItemsBatchCommand>
    {
        public DeleteItemsBatchValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ItemIds)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.EMPTY_LIST);
        }
    }
}
