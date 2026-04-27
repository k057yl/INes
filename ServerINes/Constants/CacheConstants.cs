namespace INest.Constants
{
    public static class CacheConstants
    {
        public const string CATEGORIES_PREFIX = "categories_user_";
        public const string ITEMS_PREFIX = "items_user_";
        public const string ITEM_DETAIL_PREFIX = "item_detail_user_";
        public const string LOCATIONS_TREE_PREFIX = "locations_tree_";
        public const string USER_LOCATIONS_LIST_PREFIX = "user_locations_list_";
        public const string PLATFORMS_PREFIX = "platforms_user_";
        public const string SALES_HISTORY_PREFIX = "sales_history_";
        public const string ITEM_HISTORY_PREFIX = "item_history_user_";
        public const string LOCATION_DETAIL_PREFIX = "location_detail_user_";
        public const string ACTIVE_REMINDERS_PREFIX = "active_reminders_user_";
        public const string ITEM_REMINDERS_PREFIX = "item_reminders_user_";

        public static string GET_CATEGORIES_KEY(Guid userId) => $"{CATEGORIES_PREFIX}{userId}";
        public static string GET_ITEMS_KEY(Guid userId) => $"{ITEMS_PREFIX}{userId}";
        public static string GET_ITEM_DETAIL_KEY(Guid userId, Guid itemId) => $"{ITEM_DETAIL_PREFIX}{userId}_{itemId}";
        public static string GET_LOCATIONS_TREE_KEY(Guid userId) => $"{LOCATIONS_TREE_PREFIX}{userId}";
        public static string GET_USER_LOCATIONS_LIST_KEY(Guid userId) => $"{USER_LOCATIONS_LIST_PREFIX}{userId}";
        public static string GET_PLATFORMS_KEY(Guid userId) => $"{PLATFORMS_PREFIX}{userId}";
        public static string GET_SALES_HISTORY_KEY(Guid userId) => $"{SALES_HISTORY_PREFIX}{userId}";
        public static string GET_ITEM_HISTORY_KEY(Guid userId, Guid itemId) => $"{ITEM_HISTORY_PREFIX}{userId}_{itemId}";
        public static string GET_LOCATION_DETAIL_KEY(Guid userId, Guid locationId) => $"{LOCATION_DETAIL_PREFIX}{userId}_{locationId}";
        public static string GET_ACTIVE_REMINDERS_KEY(Guid userId) => $"{ACTIVE_REMINDERS_PREFIX}{userId}";
        public static string GET_ITEM_REMINDERS_KEY(Guid userId, Guid itemId) => $"{ITEM_REMINDERS_PREFIX}{userId}_{itemId}";
    }
}
