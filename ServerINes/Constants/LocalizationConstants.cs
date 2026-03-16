namespace INest.Constants
{
    public static class LocalizationConstants
    {
        public static class SYSTEM
        {
            public const string DEFAULT_ERROR = "SYSTEM.DEFAULT_ERROR";
            public const string VALIDATION_FAILED = "SYSTEM.VALIDATION_FAILED";
            public const string EMAIL_SEND_FAILED = "SYSTEM.EMAIL_SEND_FAILED";
            public const string CONFIG_ERROR = "SYSTEM.CONFIG_ERROR";
        }

        public static class ERRORS
        {
            public const string REQUIRED_FIELD = "ERRORS.REQUIRED_FIELD";
            public const string INVALID_EMAIL_FORMAT = "ERRORS.INVALID_EMAIL_FORMAT";
            public const string INVALID_COLOR_HEX = "ERRORS.INVALID_COLOR_HEX";
            public const string MAX_LENGTH_50 = "ERRORS.MAX_LENGTH_50";
            public const string PWD_MIN_LENGTH = "ERRORS.PWD_MIN_LENGTH";
            public const string PWD_UPPER = "ERRORS.PWD_UPPER";
            public const string PWD_DIGIT = "ERRORS.PWD_DIGIT";
            public const string PWD_SPEC = "ERRORS.PWD_SPEC";
            public const string PWD_LATIN = "ERRORS.PWD_LATIN";
            public const string USERNAME_LATIN_ONLY = "ERRORS.USERNAME_LATIN_ONLY";
            public const string NEGATIVE_NUMBER = "ERRORS.NEGATIVE_NUMBER";
            public const string FUTURE_DATE = "ERRORS.FUTURE_DATE";
            public const string FILE_TOO_LARGE = "ERRORS.FILE_TOO_LARGE";
            public const string IMAGE_PROCESSING_FAILED = "ERRORS.IMAGE_PROCESSING_FAILED";
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
            public const string EMAIL_ALREADY_EXISTS = "AUTH.ERRORS.EMAIL_ALREADY_EXISTS";
            public const string INVALID_CREDENTIALS = "AUTH.ERRORS.INVALID_CREDENTIALS";
            public const string EMAIL_UNCONFIRMED = "AUTH.ERRORS.EMAIL_UNCONFIRMED";
            public const string INVALID_OR_EXPIRED_CODE = "AUTH.ERRORS.INVALID_OR_EXPIRED_CODE";
            public const string USER_NOT_FOUND = "AUTH.ERRORS.USER_NOT_FOUND";
            public const string GOOGLE_AUTH_FAILED = "AUTH.ERRORS.GOOGLE_AUTH_FAILED";
            public const string TOKEN_MISSING = "AUTH.ERRORS.TOKEN_MISSING";
            public const string OTP_SENT = "AUTH.SUCCESS.OTP_SENT";
            public const string RESET_EMAIL_SENT = "AUTH.SUCCESS.RESET_EMAIL_SENT";
            public const string PASSWORD_CHANGED = "AUTH.SUCCESS.PASSWORD_CHANGED";
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

        public static class LOCATIONS
        {
            public const string NOT_FOUND = "LOCATIONS.ERRORS.NOT_FOUND";
            public const string SELF_NESTING = "LOCATIONS.ERRORS.SELF_NESTING";
            public const string CIRCULAR_DEPENDENCY = "LOCATIONS.ERRORS.CIRCULAR_DEPENDENCY";
        }

        public static class PLATFORMS
        {
            public const string NOT_FOUND = "PLATFORMS.ERRORS.NOT_FOUND";
        }

        public static class SALES
        {
            public const string ITEM_NOT_FOUND = "SALES.ERRORS.ITEM_NOT_FOUND";
            public const string ALREADY_SOLD = "SALES.ERRORS.ALREADY_SOLD";
            public const string NOT_FOUND = "SALES.ERRORS.NOT_FOUND";
        }
    }
}