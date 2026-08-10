namespace INest.Data.Enums
{
    [Flags]
    public enum TutorialSteps
    {
        None = 0,
        Dashboard = 1 << 0,
        Items = 1 << 1,
        Locations = 1 << 2,
        Settings = 1 << 3
    }
}
