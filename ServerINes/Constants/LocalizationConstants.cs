namespace INest.Constants
{
    public static class LocalizationConstants
    {
        // Общие системные штуки
        public static class SYSTEM
        {
            public const string DEFAULT_ERROR = "SYSTEM.DEFAULT_ERROR";
            public const string VALIDATION_FAILED = "SYSTEM.VALIDATION_FAILED";
        }

        public static class EMAILS
        {
            public const string CONFIRM_SUBJECT = "CONFIRM_SUBJECT";
            public const string CONFIRM_BODY = "CONFIRM_BODY";
            public const string RESET_SUBJECT = "RESET_SUBJECT";
            public const string RESET_BODY = "RESET_BODY";
        }

        public static class AUTH
        {
            // Ошибки
            public const string EMAIL_ALREADY_EXISTS = "AUTH.ERRORS.EMAIL_ALREADY_EXISTS";
            public const string INVALID_CREDENTIALS = "AUTH.ERRORS.INVALID_CREDENTIALS";
            public const string EMAIL_UNCONFIRMED = "AUTH.ERRORS.EMAIL_UNCONFIRMED";
            public const string INVALID_OR_EXPIRED_CODE = "AUTH.ERRORS.INVALID_OR_EXPIRED_CODE";
            public const string USER_NOT_FOUND = "AUTH.ERRORS.USER_NOT_FOUND";
            public const string GOOGLE_AUTH_FAILED = "AUTH.ERRORS.GOOGLE_AUTH_FAILED";

            // Успешные действия / Уведомления
            public const string OTP_SENT = "AUTH.SUCCESS.OTP_SENT";
            public const string RESET_EMAIL_SENT = "AUTH.SUCCESS.RESET_EMAIL_SENT";
            public const string PASSWORD_CHANGED = "AUTH.SUCCESS.PASSWORD_CHANGED";
        }

        public static class ERRORS
        {
            public const string REQUIRED_FIELD = "ERRORS.REQUIRED_FIELD";
            public const string INVALID_EMAIL_FORMAT = "ERRORS.INVALID_EMAIL_FORMAT";
            public const string INVALID_COLOR_HEX = "ERRORS.INVALID_COLOR_HEX";
            public const string MAX_LENGTH_50 = "ERRORS.MAX_LENGTH_50";
            public const string PWD_MIN_LENGTH = "ERRORS.PWD_MIN_LENGTH";
        }

        public static class CATEGORIES
        {
            public const string NOT_FOUND = "CATEGORIES.ERRORS.NOT_FOUND";
            public const string CANNOT_DELETE_DEFAULT = "CATEGORIES.ERRORS.CANNOT_DELETE_DEFAULT";
        }

        public static class ITEMS
        {
            public const string NOT_FOUND = "ITEMS.ERRORS.NOT_FOUND";
            public const string DELETE_SUCCESS = "ITEMS.SUCCESS.DELETE";
        }
    }
}