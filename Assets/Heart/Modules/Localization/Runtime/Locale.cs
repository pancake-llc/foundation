using System;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    public sealed class Locale
    {
        private static Locale instance;
        private Language _currentLanguage = Language.English;

        /// <summary>
        /// Raised when <see cref="CurrentLanguage"/> has been changed. 
        /// </summary>
        private event EventHandler<LocaleChangedEventArgs> OnLocaleChangedEvent;

        private static Locale Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Debug.LogError("Locale only avaiable when application playing!");
                    return null;
                }

                if (instance == null)
                {
                    instance = new Locale();
                    instance.SetDefaultLanguage();
                }

                return instance;
#endif
            }
        }

        public static Language CurrentLanguage
        {
            get => Instance._currentLanguage;
            set
            {
                if (Instance._currentLanguage != value)
                {
                    Instance._currentLanguage = value;
                    var oldValue = Instance._currentLanguage;
                    Instance._currentLanguage = value;
                    Instance.OnLanguageChanged(new LocaleChangedEventArgs(oldValue, value));
                }
            }
        }

        public static EventHandler<LocaleChangedEventArgs> LocaleChangedEvent { get => Instance.OnLocaleChangedEvent; set => Instance.OnLocaleChangedEvent = value; }

        private void OnLanguageChanged(LocaleChangedEventArgs e) { OnLocaleChangedEvent?.Invoke(this, e); }

        /// <summary>
        /// Sets the <see cref="CurrentLanguage"/> as <see cref="Application.systemLanguage"/>.
        /// </summary>
        public void SetSystemLanguage() { CurrentLanguage = Application.systemLanguage; }

        /// <summary>
        /// Sets the <see cref="CurrentLanguage"/> to default language defined in <see cref="LocaleSettings"/>.
        /// </summary>
        public void SetDefaultLanguage() { CurrentLanguage = LocaleSettings.Instance.AvailableLanguages.FirstOrDefault(); }
    }
}