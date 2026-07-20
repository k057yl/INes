using INest.Data.Enums;
using INest.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Items.Commands.DeleteArchivedItem
{
    public class DeleteArchivedItemHandler : IRequestHandler<DeleteArchivedItemCommand>
    {
        private readonly AppDbContext _context;

        public DeleteArchivedItemHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteArchivedItemCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.UserId == request.UserId, cancellationToken);

            if (item == null)
                throw new KeyNotFoundException(ITEMS.ERRORS.NOT_FOUND);

            if (item.Status != ItemStatus.Archived)
            {
                throw new AppException(ITEMS.ERRORS.ONLY_ARCHIVED_CAN_BE_DELETED);
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
