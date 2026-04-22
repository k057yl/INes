namespace INest.Constants
{
    public static class SharedConstants
    {
        public const string LOCALHOST = "https://localhost:4200";
        public const string PWA = "https://localhost:8080";
        public const string PWA_FROM_IP = "https://127.0.0.1:8080";
        public const string PWA_MOBILE = "https://192.168.0.104:8080";
        public const string WSL_IP = "https://172.27.128.1:8080";

        public const string PWA_MOBILE_API = "https://192.168.0.104:7068";
        public static string CONTENT_SECURITY_POLICY =>
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com data:; " +
            "img-src 'self' data: https://res.cloudinary.com; " +
            $"connect-src 'self' https://localhost:7068 {PWA_MOBILE_API};";


        public const string CATEGORY_OTHER = "Other";

        public const string DEFAULT_ROLE = "inest_app_user";

        public const string ADMIN_ROLE = "inest_app_admin";

        public const string JWT_KEY_MISSING = "JWT Key missing";

        public const string CONTENT_TYPE_JSON = "application/json";
        //Item service
        public const string OLD_VALUE = "Sold";
        public const string NEW_VALUE = "Active";
    }
}
