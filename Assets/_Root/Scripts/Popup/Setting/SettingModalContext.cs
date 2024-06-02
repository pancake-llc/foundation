using System;
using Coffee.UIEffects;
using Pancake.Localization;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Pancake.SceneFlow
{
    public sealed class SettingModalContext : Modal
    {
        [Serializable]
        public class UIView
        {
            [field: Header("Music")] [field: SerializeField] private Button ButtonMusic { get; set; }
            [field: SerializeField] private Button ButtonSound { get; set; }
            [field: SerializeField] private Button ButtonVibrate { get; set; }
            [field: SerializeField] private UIEffect MusicUIEffect { get; set; }
            [field: SerializeField] private UIEffect SoundUIEffect { get; set; }
            [field: SerializeField] private UIEffect VibrateUIEffect { get; set; }

            [field: Header("Language")] [field: SerializeField] private GameObject LanguageElementPrefab { get; set; }
            [field: SerializeField] private RectTransform LanguagePopup { get; set; }
            [field: SerializeField] private UIButton ButtonSelectLanguage { get; set; }
            [field: SerializeField] private TextMeshProUGUI TextNameLanguageSelected { get; set; }

            [field: Header("")] [field: SerializeField] private LocaleTextComponent LocaleTextVersion { get; set; }
            [field: SerializeField] private Button ButtonUpdate { get; set; }
            [field: SerializeField] private Button ButtonClose { get; set; }
            [field: SerializeField] private Button ButtonCredit { get; set; }
            [field: SerializeField] private Button ButtonBackupData { get; set; }
            [field: SerializeField] private Button ButtonRestoreData { get; set; }
        }

        [Serializable]
        public class UIModel
        {
            
        }

        [field: SerializeField] public UIView View { get; private set; }

        public UIModel Model => Container.Resolve<UIModel>();

        protected override void Configure(IContainerBuilder builder) { }
    }
}