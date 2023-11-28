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

        [MenuItem(MENU_NAME + "/Chinese")]
        private static void ChangeToChinese() { SetLanguage(Language.Chinese); }

        [MenuItem(MENU_NAME + "/English")]
        private static void ChangeToEnglish() { SetLanguage(Language.English); }

        [MenuItem(MENU_NAME + "/French")]
        private static void ChangeToFrench() { SetLanguage(Language.French); }

        [MenuItem(MENU_NAME + "/German")]
        private static void ChangeToGerman() { SetLanguage(Language.German); }

        [MenuItem(MENU_NAME + "/Italian")]
        private static void ChangeToItalian() { SetLanguage(Language.Italian); }

        [MenuItem(MENU_NAME + "/Japanese")]
        private static void ChangeToJapanese() { SetLanguage(Language.Japanese); }

        [MenuItem(MENU_NAME + "/Korean")]
        private static void ChangeToKorean() { SetLanguage(Language.Korean); }

        [MenuItem(MENU_NAME + "/Portuguese")]
        private static void ChangeToPortuguese() { SetLanguage(Language.Portuguese); }

        [MenuItem(MENU_NAME + "/Russian")]
        private static void ChangeToRussian() { SetLanguage(Language.Russian); }

        [MenuItem(MENU_NAME + "/Spanish")]
        private static void ChangeToSpanish() { SetLanguage(Language.Spanish); }

        [MenuItem(MENU_NAME + "/Vietnamese")]
        private static void ChangeToVietnamese() { SetLanguage(Language.Vietnamese); }

        [MenuItem(MENU_NAME + "/Unknown")]
        private static void ChangeToUnknown() { SetLanguage(Language.Unknown); }

        private static string GetMenuName(Language language) { return $"{MENU_NAME}{language}"; }
    }
}