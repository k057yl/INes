namespace INest.Constants
{
    public static class CacheConstants
    {
        public const string CATEGORIES_PREFIX = "categories_user_";
        public const string ITEMS_PREFIX = "items_user_";
        public const string LOCATIONS_TREE_PREFIX = "locations_tree_";
        public const string USER_LOCATIONS_LIST_PREFIX = "user_locations_list_";
        public const string PLATFORMS_PREFIX = "platforms_user_";
        public const string SALES_HISTORY_PREFIX = "sales_history_";

        public static string GET_CATEGORIES_KEY(Guid userId) => $"{CATEGORIES_PREFIX}{userId}";
        public static string GET_ITEMS_KEY(Guid userId) => $"{ITEMS_PREFIX}{userId}";
        public static string GET_LOCATIONS_TREE_KEY(Guid userId) => $"{LOCATIONS_TREE_PREFIX}{userId}";
        public static string GET_USER_LOCATIONS_LIST_KEY(Guid userId) => $"{USER_LOCATIONS_LIST_PREFIX}{userId}";
        public static string GET_PLATFORMS_KEY(Guid userId) => $"{PLATFORMS_PREFIX}{userId}";
        public static string GET_SALES_HISTORY_KEY(Guid userId) => $"{SALES_HISTORY_PREFIX}{userId}";
    }
}
