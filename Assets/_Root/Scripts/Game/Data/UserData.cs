using Pancake.Common;
using Pancake.Localization;

namespace Pancake.Game
{
    public static class UserData
    {
        public static string GetCurrentLanguage() => Data.Load(Constant.User.KEY_LANGUAGE, "");
        public static void SetCurrentLanguage(Language language) => Data.Save(Constant.User.KEY_LANGUAGE, language.Code);
        public static void SetCurrentLanguage(string languageCode) => Data.Save(Constant.User.KEY_LANGUAGE, languageCode);
        
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
    }
}