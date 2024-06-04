using System;
using Coffee.UIEffects;
using Pancake.Localization;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    public sealed class SettingModalContext : Modal
    {
        [Serializable]
        public class UIView
        {
            [field: Header("Music")] [field: SerializeField] public Button ButtonMusic { get; set; }
            [field: SerializeField] public Button ButtonSound { get; set; }
            [field: SerializeField] public Button ButtonVibrate { get; set; }
            [field: SerializeField] public UIEffect MusicUIEffect { get; set; }
            [field: SerializeField] public UIEffect SoundUIEffect { get; set; }
            [field: SerializeField] public UIEffect VibrateUIEffect { get; set; }

            [field: Header("Language")] [field: SerializeField] public GameObject LanguageElementPrefab { get; set; }
            [field: SerializeField] public RectTransform LanguagePopup { get; set; }
            [field: SerializeField] public UIButton ButtonSelectLanguage { get; set; }
            [field: SerializeField] public TextMeshProUGUI TextNameLanguageSelected { get; set; }

            [field: Header("")] [field: SerializeField] public LocaleTextComponent LocaleTextVersion { get; set; }
            [field: SerializeField] public Button ButtonUpdate { get; set; }
            [field: SerializeField] public Button ButtonClose { get; set; }
            [field: SerializeField] public Button ButtonCredit { get; set; }
            [field: SerializeField] public Button ButtonBackupData { get; set; }
            [field: SerializeField] public Button ButtonRestoreData { get; set; }
        }

        [Serializable]
        public class UIModel
        {
            
        }

        [field: SerializeField] public UIView View { get; private set; }

        public UIModel Model => Container.Resolve<UIModel>();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(View);
            builder.RegisterEntryPoint<SettingModalPresenter>();
        }
    }
}