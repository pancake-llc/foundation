using Pancake.Localization;

namespace Pancake.SceneFlow
{
    using Pancake;

    public class LanguageInitialization : Initialize
    {
        public override void Init() { Locale.LocaleChangedEvent += OnLocaleChanged; }

        private void OnDestroy() { Locale.LocaleChangedEvent -= OnLocaleChanged; }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e) { UserData.SetCurrentLanguage(Locale.CurrentLanguage); }
    }
}