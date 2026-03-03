namespace INest.Constants
{
    public static class CacheConstants
    {
        public const string LocationsTreePrefix = "locations_tree_";
        public const string UserLocationsListPrefix = "user_locations_list_";

        public static string GetLocationsTreeKey(Guid userId) => $"{LocationsTreePrefix}{userId}";
        public static string GetUserLocationsListKey(Guid userId) => $"{UserLocationsListPrefix}{userId}";
    }
}
