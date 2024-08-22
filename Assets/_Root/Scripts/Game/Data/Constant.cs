namespace Pancake.Game
{
    public static class Constant
    {
        public static class User
        {
            public const string KEY_LANGUAGE = "user_lang";
            public const string KEY_QUALITY = "user_quality";
            public const string KEY_MUSIC = "user_music";
            public const string KEY_SFX = "user_sfx";
            public const string KEY_VIBRATE = "user_vibrate";
            public const string KEY_FIRST_OPEN = "user_first_open";
            public const string KEY_ID = "user_id";
            public const string AGREE_PRIVACY = "user_agree_privacy";

            public static class DailyReward
            {
                public const string CURRENT_WEEK = "user_current_week_dr";
                public const string CURRENT_DAY = "user_current_day_dr";
                public const string LAST_TIME_UPDATE = "user_last_time_update_dr";
                public const string ALL_DAY_CLAIMED_IN_WEEK = "user_all_day_claimed_dr";
            }

            public static class Shop
            {
                public const string FIRST_PURCHASE = "user_first_purchase";
            }
        }

        public static class Scene
        {
            public const string LAUNCHER = "launcher";
            public const string MENU = "menu";
            public const string GAMEPLAY = "gameplay";
            public const string PERSISTENT = "persistent";
        }

        public const string PERSISTENT_POPUP_CONTAINER = "persistent_popup_container";
    }
}