namespace INest.Data.Enums
{
    [Flags]
    public enum TutorialSteps
    {
        None = 0,
        Dashboard = 1 << 0,
        Items = 1 << 1,
        Locations = 1 << 2,
        Settings = 1 << 3,
        LocationForm = 1 << 4,
        ItemForm = 1 << 5,
        Sales = 1 << 6,
        ItemsList = 1 << 7,
        SellForm = 1 << 8,
        FirstSaleCard = 1 << 9
    }
}
