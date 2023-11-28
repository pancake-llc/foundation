using Pancake.Localization;
using UnityEditor;
using UnityEngine;

namespace Pancake.LocalizationEditor
{
    public static class EditorMenu
    {
        private const string MENU_NAME = "Tools/Pancake/Localization/Change Locale/";

        private static void SetLanguage(Language currentLanguage)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Setting language only available when application is playing.");
                return;
            }

            var previousLanguage = Locale.CurrentLanguage;
            Locale.CurrentLanguage = currentLanguage;

            Menu.SetChecked(GetMenuName(previousLanguage), false);
            Menu.SetChecked(GetMenuName(currentLanguage), true);
        }

        [MenuItem(MENU_NAME + "/Chinese", priority = 10001)]
        private static void ChangeToChinese() { SetLanguage(Language.Chinese); }

        [MenuItem(MENU_NAME + "/English", priority = 10002)]
        private static void ChangeToEnglish() { SetLanguage(Language.English); }

        [MenuItem(MENU_NAME + "/French", priority = 10003)]
        private static void ChangeToFrench() { SetLanguage(Language.French); }

        [MenuItem(MENU_NAME + "/German", priority = 10004)]
        private static void ChangeToGerman() { SetLanguage(Language.German); }

        [MenuItem(MENU_NAME + "/Italian", priority = 10005)]
        private static void ChangeToItalian() { SetLanguage(Language.Italian); }

        [MenuItem(MENU_NAME + "/Japanese", priority = 10006)]
        private static void ChangeToJapanese() { SetLanguage(Language.Japanese); }

        [MenuItem(MENU_NAME + "/Korean", priority = 10007)]
        private static void ChangeToKorean() { SetLanguage(Language.Korean); }

        [MenuItem(MENU_NAME + "/Portuguese", priority = 10008)]
        private static void ChangeToPortuguese() { SetLanguage(Language.Portuguese); }

        [MenuItem(MENU_NAME + "/Russian", priority = 10009)]
        private static void ChangeToRussian() { SetLanguage(Language.Russian); }

        [MenuItem(MENU_NAME + "/Spanish", priority = 10010)]
        private static void ChangeToSpanish() { SetLanguage(Language.Spanish); }

        [MenuItem(MENU_NAME + "/Vietnamese", priority = 10011)]
        private static void ChangeToVietnamese() { SetLanguage(Language.Vietnamese); }

        [MenuItem(MENU_NAME + "/Unknown", priority = 10012)]
        private static void ChangeToUnknown() { SetLanguage(Language.Unknown); }

        private static string GetMenuName(Language language) { return $"{MENU_NAME}{language}"; }
    }
}