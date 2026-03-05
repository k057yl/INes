namespace INest.Constants
{
    public static class CacheConstants
    {
        public const string LOCATIONS_TREE_PREFIX = "locations_tree_";
        public const string USER_LOCATIONS_LIST_PREFIX = "user_locations_list_";
        public const string CATEGORIES_PREFIX = "categories_user_";
        public const string PLATFORMS_PREFIX = "platforms_user_";

        public static string GetLocationsTreeKey(Guid userId) => $"{LOCATIONS_TREE_PREFIX}{userId}";
        public static string GetUserLocationsListKey(Guid userId) => $"{USER_LOCATIONS_LIST_PREFIX}{userId}";
        public static string GetCategoriesKey(Guid userId) => $"{CATEGORIES_PREFIX}{userId}";
        public static string GetPlatformsKey(Guid userId) => $"{PLATFORMS_PREFIX}{userId}";
    }
}
