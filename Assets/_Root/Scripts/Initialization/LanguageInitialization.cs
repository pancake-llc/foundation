using Pancake.Localization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    using Pancake;

    public class LanguageInitialization : Initialize
    {
        [SerializeField] private bool detectDeviceLanguage;

        public override void Init()
        {
            Locale.LocaleChangedEvent += OnLocaleChanged;
            var list = LocaleSettings.Instance.AvailableLanguages;
            string lang = UserData.GetCurrentLanguage();
            // for first time when user not choose lang to display
            // use system language, if dont use detect system language use first language in list available laguages
            if (string.IsNullOrEmpty(lang))
            {
                var index = 0;
                if (detectDeviceLanguage)
                {
                    var nameSystemLang = Application.systemLanguage.ToString();
                    index = list.FindIndex(x => x.Name == nameSystemLang);
                    if (index < 0) index = 0;
                }

                lang = list[index].Code;
                UserData.SetCurrentLanguage(lang);
            }

            int i = list.FindIndex(x => x.Code == lang);
            Locale.CurrentLanguage = list[i];
        }

        private void OnDestroy() { Locale.LocaleChangedEvent -= OnLocaleChanged; }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e) { UserData.SetCurrentLanguage(Locale.CurrentLanguage); }
    }
}