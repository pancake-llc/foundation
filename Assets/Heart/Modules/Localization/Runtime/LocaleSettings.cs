using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("scriptable_setting")]
    public sealed class LocaleSettings : ScriptableSettings<LocaleSettings>
    {
        [SerializeField] private List<Language> availableLanguages = new List<Language>(1) {Language.English};
        [SerializeField] private string importLocation = "Assets";
        [SerializeField] private string googleCredential;
        
        public string ImportLocation => importLocation;
        public List<Language> AvailableLanguages => availableLanguages;
        public string GoogleCredential => googleCredential;

        public List<Language> AllLanguages
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