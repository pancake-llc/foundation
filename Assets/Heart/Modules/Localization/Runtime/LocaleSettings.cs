using System.Collections.Generic;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("so_blue_setting")]
    public sealed class LocaleSettings : ScriptableSettings<LocaleSettings>
    {
        [SerializeField] private List<Language> availableLanguages = new(1) {Language.English};
        [SerializeField] private bool detectDeviceLanguage;
        [SerializeField] private string importLocation = "Assets";
        [SerializeField] private string googleTranslateApiKey;
        [SerializeField] private string spreadsheetKey;
        [SerializeField, TextArea] private string serviceAccountCredential;

        public static bool DetectDeviceLanguage => Instance.detectDeviceLanguage;
        public static string ImportLocation => Instance.importLocation;
        public static List<Language> AvailableLanguages => Instance.availableLanguages;
        public static string GoogleTranslateApiKey => Instance.googleTranslateApiKey;
        public static string SpreadsheetKey => Instance.spreadsheetKey;
        public static string ServiceAccountCredential => Instance.serviceAccountCredential;

        public static List<Language> AllLanguages
        {
            get
            {
                var languages = new List<Language>();
                languages.Adds(Language.BuiltInLanguages);
                languages.Adds(AvailableLanguages.Filter(l => l.Custom));
                return languages;
            }
        }
    }
}