using Pancake.Localization;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    using Pancake;

    public class LanguageInitialization : Initialize
    {
        [SerializeField] private BoolVariable isInitialized;

        public override void Init()
        {
            Locale.LocaleChangedEvent += OnLocaleChanged;
            if (!isInitialized.Value) UserData.LoadLanguageSetting(LocaleSettings.DetectDeviceLanguage);
        }

        private void OnDestroy() { Locale.LocaleChangedEvent -= OnLocaleChanged; }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e) { UserData.SetCurrentLanguage(Locale.CurrentLanguage); }
    }
}