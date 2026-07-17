using INest.Constants;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;

namespace INest.Data.Entities.Core
{
    public class Item : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ItemStatus Status { get; private set; } = ItemStatus.Active;

        public Guid? StorageLocationId { get; set; }
        public StorageLocation? StorageLocation { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public AppUser User { get; set; } = null!;

        // Связь 1-к-1 с вынесенными деталями
        public ItemDetails? Details { get; set; }

        public string? PhotoUrl { get; set; }
        public string? PublicId { get; set; }

        public ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>();
        public ICollection<ItemHistory> History { get; set; } = new List<ItemHistory>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();


        public void Lend()
        {
            EnsureStatus(
                ItemStatus.Active,
                LocalizationConstants.ITEMS.ERRORS.CANNOT_LEND);

            Status = ItemStatus.Lent;
        }

        public void Return()
        {
            EnsureStatus(
                ItemStatus.Lent,
                LocalizationConstants.ITEMS.ERRORS.CANNOT_RETURN);

            Status = ItemStatus.Active;
        }

        public void Sell()
        {
            EnsureStatus(
                ItemStatus.Active,
                LocalizationConstants.ITEMS.ERRORS.CANNOT_SELL);

            Status = ItemStatus.Sold;

            RemoveFromLocation();
        }

        public void CancelSale()
        {
            EnsureStatus(
                ItemStatus.Sold,
                LocalizationConstants.ITEMS.ERRORS.CANNOT_CANCEL_SALE);

            Status = ItemStatus.Active;
        }

        public void Archive()
        {
            if (Status == ItemStatus.Archived)
            {
                throw new AppException(
                    LocalizationConstants.ITEMS.ERRORS.ALREADY_ARCHIVED);
            }

            Status = ItemStatus.Archived;

            RemoveFromLocation();
        }

        public void MoveToLocation(Guid? targetLocationId)
        {
            if (Status == ItemStatus.Sold || Status == ItemStatus.Archived)
            {
                throw new AppException(LocalizationConstants.ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED);
            }

            StorageLocationId = targetLocationId;
        }

        public void Borrow()
        {
            EnsureStatus(
                ItemStatus.Active,
                LocalizationConstants.ITEMS.ERRORS.INVALID_INITIAL_STATUS);

            Status = ItemStatus.Borrowed;
        }

        public void ReturnBorrowed()
        {
            EnsureStatus(
                ItemStatus.Borrowed,
                LocalizationConstants.ITEMS.ERRORS.CANNOT_RETURN_BORROWED);

            Status = ItemStatus.Active;

            RemoveFromLocation();
        }

        private void EnsureStatus(ItemStatus expectedStatus, string error)
        {
            if (Status != expectedStatus)
                throw new AppException(error);
        }

        private void RemoveFromLocation()
        {
            StorageLocationId = null;
        }
    }
}