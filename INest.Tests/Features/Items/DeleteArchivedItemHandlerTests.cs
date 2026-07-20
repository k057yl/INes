using INest.Constants;
using INest.Data.Entities.Core;
using INest.Exceptions;
using INest.Features.Items.Commands.DeleteArchivedItem;
using INest.Tests.Helpers;
using Shouldly;
using Microsoft.EntityFrameworkCore;

namespace INest.Tests.Features.Items
{
    public class DeleteArchivedItemHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteProperty_WhenItemIsArchived()
        {
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Старый перфоратор",
                CategoryId = Guid.NewGuid()
            };

            item.Archive();

            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteArchivedItemHandler(db);
            var command = new DeleteArchivedItemCommand(userId, item.Id);

            await handler.Handle(command, CancellationToken.None);

            var deletedItem = await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
            deletedItem.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenItemIsActive()
        {
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Активный ноутбук",
                CategoryId = Guid.NewGuid()
            };

            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteArchivedItemHandler(db);
            var command = new DeleteArchivedItemCommand(userId, item.Id);

            var exception = await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            exception.Message.ShouldBe(LocalizationConstants.ITEMS.ERRORS.ONLY_ARCHIVED_CAN_BE_DELETED);

            var itemInDb = await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
            itemInDb.ShouldNotBeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
        {
            using var db = DbContextFactory.Create();
            var handler = new DeleteArchivedItemHandler(db);
            var command = new DeleteArchivedItemCommand(Guid.NewGuid(), Guid.NewGuid());

            var exception = await Should.ThrowAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            exception.Message.ShouldBe(LocalizationConstants.ITEMS.ERRORS.NOT_FOUND);
        }
    }
}
