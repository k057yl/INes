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

            public static class ERRORS
            {
                public const string VALIDATION_FAILED = "SYSTEM.ERRORS.VALIDATION_FAILED";
            }
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
            public const string MAX_NESTING_REACHED = "ERRORS.MAX_NESTING_REACHED";
        }

        public static class EMAILS
        {
            public const string CONFIRM_SUBJECT = "EMAILS.CONFIRM_SUBJECT";
            public const string CONFIRM_BODY = "EMAILS.CONFIRM_BODY";
            public const string RESET_SUBJECT = "EMAILS.RESET_SUBJECT";
            public const string RESET_BODY = "EMAILS.RESET_BODY";
        }

        public static class AUTH
        {
            public static class ERRORS
            {
                public const string EMAIL_ALREADY_EXISTS = "AUTH.ERRORS.EMAIL_ALREADY_EXISTS";
                public const string INVALID_CREDENTIALS = "AUTH.ERRORS.INVALID_CREDENTIALS";
                public const string EMAIL_UNCONFIRMED = "AUTH.ERRORS.EMAIL_UNCONFIRMED";
                public const string INVALID_OR_EXPIRED_CODE = "AUTH.ERRORS.INVALID_OR_EXPIRED_CODE";
                public const string USER_NOT_FOUND = "AUTH.ERRORS.USER_NOT_FOUND";
                public const string GOOGLE_AUTH_FAILED = "AUTH.ERRORS.GOOGLE_AUTH_FAILED";
                public const string TOKEN_MISSING = "AUTH.ERRORS.TOKEN_MISSING";
                public const string INVALID_TOKEN = "AUTH.ERRORS.INVALID_TOKEN";
                public const string REFRESH_TOKEN_EXPIRED = "AUTH.ERRORS.REFRESH_TOKEN_EXPIRED";
                public const string INVALID_USERNAME = "AUTH.ERRORS.INVALID_USERNAME";
                public const string REGISTRATION_FAILED = "AUTH.ERRORS.REGISTRATION_FAILED";
            }

            public static class SUCCESS
            {
                public const string OTP_SENT = "AUTH.SUCCESS.OTP_SENT";
                public const string RESET_EMAIL_SENT = "AUTH.SUCCESS.RESET_EMAIL_SENT";
                public const string PASSWORD_CHANGED = "AUTH.SUCCESS.PASSWORD_CHANGED";
            }
        }

        public static class CATEGORIES
        {
            public static class ERRORS
            {
                public const string NOT_FOUND = "CATEGORIES.ERRORS.NOT_FOUND";
                public const string CANNOT_DELETE_DEFAULT = "CATEGORIES.ERRORS.CANNOT_DELETE_DEFAULT";
                public const string INVALID_NAME = "CATEGORIES.ERRORS.INVALID_NAME";
            }
        }

        public static class ITEMS
        {
            public static class ERRORS
            {
                public const string NOT_FOUND = "ITEMS.ERRORS.NOT_FOUND";
                public const string ONLY_ACTIVE_CAN_BE_EDITED = "ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED";
            }

            public static class SUCCESS
            {
                public const string DELETE = "ITEMS.SUCCESS.DELETE";
            }
        }

        public static class LOCATIONS
        {
            public static class ERRORS
            {
                public const string NOT_FOUND = "LOCATIONS.ERRORS.NOT_FOUND";
                public const string SELF_NESTING = "LOCATIONS.ERRORS.SELF_NESTING";
                public const string CIRCULAR_DEPENDENCY = "LOCATIONS.ERRORS.CIRCULAR_DEPENDENCY";
                public const string INVALID_NAME = "LOCATIONS.ERRORS.INVALID_NAME";
            }
        }

        public static class PLATFORMS
        {
            public static class ERRORS
            {
                public const string NOT_FOUND = "PLATFORMS.ERRORS.NOT_FOUND";
                public const string INVALID_NAME = "PLATFORMS.ERRORS.INVALID_NAME";
            }
        }

        public static class SALES
        {
            public static class ERRORS
            {
                public const string ITEM_NOT_FOUND = "SALES.ERRORS.ITEM_NOT_FOUND";
                public const string ALREADY_SOLD = "SALES.ERRORS.ALREADY_SOLD";
                public const string NOT_FOUND = "SALES.ERRORS.NOT_FOUND";
            }
        }

        public static class STATUS
        {
            public const string ACTIVE = "STATUS.ACTIVE";
            public const string LENT = "STATUS.LENT";
            public const string LOST = "STATUS.LOST";
            public const string BROKEN = "STATUS.BROKEN";
            public const string SOLD = "STATUS.SOLD";
            public const string GIFTED = "STATUS.GIFTED";
            public const string LISTED = "STATUS.LISTED";
            public const string BORROWED = "STATUS.BORROWED";
        }

        public static class HISTORY
        {
            public const string CREATED = "HISTORY.CREATED";
            public const string MOVED = "HISTORY.MOVED";
            public const string STATUS_CHANGED = "HISTORY.STATUS_CHANGED";
            public const string REPAIRED = "HISTORY.REPAIRED";
            public const string LENT = "HISTORY.LENT";
            public const string RETURNED = "HISTORY.RETURNED";
            public const string VALUE_UPDATED = "HISTORY.VALUE_UPDATED";
            public const string PHOTOS_ADDED_COUNT = "HISTORY.PHOTOS_ADDED_COUNT";

            public static class REMINDER
            {
                public const string COMPLETED = "HISTORY.REMINDER.COMPLETED";
                public const string SCHEDULED = "HISTORY.REMINDER.SCHEDULED";
            }
        }

        public static class REMINDERS
        {
            public const string CUSTOM = "REMINDERS.CUSTOM";
            public const string WARRANTY = "REMINDERS.WARRANTY";
            public const string MAINTENANCE = "REMINDERS.MAINTENANCE";
            public const string RETURN_ITEM = "REMINDERS.RETURN_ITEM";
            public const string INSURANCE = "REMINDERS.INSURANCE";
            public const string MEDICAL = "REMINDERS.MEDICAL";
            public const string TAX = "REMINDERS.TAX";
            public const string SUBSCRIPTION = "REMINDERS.SUBSCRIPTION";
        }

        public static class LENDING
        {
            public static class ERRORS
            {
                public const string NOT_FOUND = "LENDING.ERRORS.NOT_FOUND";
                public const string ALREADY_LENT = "LENDING.ERRORS.ALREADY_LENT";
                public const string NOT_LENT = "LENDING.ERRORS.NOT_LENT";
            }
        }
    }
}