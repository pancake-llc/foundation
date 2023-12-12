using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("scriptable_setting")]
    public sealed class LocaleSettings : ScriptableSettings<LocaleSettings>
    {
        [SerializeField] private List<Language> availableLanguages = new List<Language>(1) {Language.English};
        [SerializeField] private bool detectDeviceLanguage;
        [SerializeField] private string importLocation = "Assets";
        [SerializeField] private string googleCredential;

        public static bool DetectDeviceLanguage => Instance.detectDeviceLanguage;
        public static string ImportLocation => Instance.importLocation;
        public static List<Language> AvailableLanguages => Instance.availableLanguages;
        public static string GoogleCredential => Instance.googleCredential;

        public static List<Language> AllLanguages
        {
            get
            {
                var languages = new List<Language>();
                languages.AddRange(Language.BuiltInLanguages);
                languages.AddRange(AvailableLanguages.Filter(l => l.Custom));
                return languages;
            }
        }
    }
}