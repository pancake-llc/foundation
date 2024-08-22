using System;
using Pancake.Common;
using Pancake.Localization;
using static Pancake.Game.Constant.User.DailyReward;
using static Pancake.Game.Constant.User;

namespace Pancake.Game
{
    public static class UserData
    {
        public static string GetCurrentLanguage() => Data.Load(KEY_LANGUAGE, "");
        public static void SetCurrentLanguage(Language language) => Data.Save(KEY_LANGUAGE, language.Code);
        public static void SetCurrentLanguage(string languageCode) => Data.Save(KEY_LANGUAGE, languageCode);

        public static void LoadLanguageSetting(bool detectDeviceLanguage)
        {
            var list = LocaleSettings.AvailableLanguages;
            string lang = GetCurrentLanguage();
            // for first time when user not choose lang to display
            // use system language, if you don't use detect system language use first language in list available laguages
            if (string.IsNullOrEmpty(lang))
            {
                var index = 0;
                if (detectDeviceLanguage)
                {
                    var nameSystemLang = UnityEngine.Application.systemLanguage.ToString();
                    index = list.FindIndex(x => x.Name == nameSystemLang);
                    if (index < 0) index = 0;
                }

                lang = list[index].Code;
                SetCurrentLanguage(lang);
            }

            int i = list.FindIndex(x => x.Code == lang);
            Locale.CurrentLanguage = list[i];
        }

        public static EQuality GetCurrentQuality() => Data.Load(KEY_QUALITY, EQuality.Low);
        public static void SetCurrentQuality(EQuality quality) => Data.Save(KEY_QUALITY, quality);

        public static bool GetMusic() => Data.Load(KEY_MUSIC, true);
        public static void SetMusic(bool status) => Data.Save(KEY_MUSIC, status);

        public static bool GetSfx() => Data.Load(KEY_SFX, true);
        public static void SetSfx(bool status) => Data.Save(KEY_SFX, status);

        public static bool GetVibrate() => Data.Load(KEY_VIBRATE, true);
        public static void SetVibrate(bool status) => Data.Save(KEY_VIBRATE, status);

        public static bool GetFirstOpen() => Data.Load(KEY_FIRST_OPEN, false);
        internal static void SetFirstOpen(bool status) => Data.Save(KEY_FIRST_OPEN, status);

        public static string UserId => Data.Load(KEY_ID, Guid.NewGuid().ToString("N")[..16]);

        public static int GetDailyRewardWeek() => Data.Load(CURRENT_WEEK, 1);
        public static void SetDailyRewardWeek(int value) => Data.Save(CURRENT_WEEK, value);
        public static void NextWeekDailyReward() => Data.Save(CURRENT_WEEK, GetDailyRewardWeek() + 1);
        public static bool GetStatusAllDayClaimedInWeek() => Data.Load(ALL_DAY_CLAIMED_IN_WEEK, false);
        public static void SetStatusAllDayClaimedInWeek(bool value) => Data.Save(ALL_DAY_CLAIMED_IN_WEEK, value);


        public static int GetDailyRewardDay() => Data.Load(CURRENT_DAY, 1);
        public static void SetDailyRewardDay(int day) => Data.Save(CURRENT_DAY, day.Min(7));
        public static void NextDayDailyReward() => Data.Save(CURRENT_DAY, (GetDailyRewardDay() + 1).Min(7));

        public static bool IsDailyRewardNewDay()
        {
            string currentShortDate = DateTime.Now.ToShortDateString();
            DateTime.TryParse(currentShortDate, out var shortDate);
            var oneDay = new TimeSpan(24, 0, 0);
            string lastTimeUpdated = Data.Load(LAST_TIME_UPDATE, (DateTime.Now - oneDay).ToShortDateString());
            DateTime.TryParse(lastTimeUpdated, out var lastTime);
            var comparison = shortDate - lastTime;
            return comparison >= oneDay;
        }

        public static void SetDailyRewardLastTimeUpdate() { Data.Save(LAST_TIME_UPDATE, DateTime.Now.ToShortDateString()); }

        public static bool GetFirstPurchase() => Data.Load(Shop.FIRST_PURCHASE, false);
        internal static void SetFirstPurchase(bool status) => Data.Save(Shop.FIRST_PURCHASE, status);
    }
}