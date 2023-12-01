using Coffee.UIEffects;
using Pancake.Apex;
using Pancake.Localization;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class SettingView : View, IEnhancedScrollerDelegate
    {
        [SerializeField] private LocaleTextComponent localeTextVersion;
        [SerializeField] private Button buttonMusic;
        [SerializeField] private Button buttonSound;
        [SerializeField] private Button buttonVibrate;
        [SerializeField] private Button buttonUpdate;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonCredit;

        [SerializeField] private UIEffect musicUIEffect;
        [SerializeField] private UIEffect soundUIEffect;
        [SerializeField] private UIEffect vibrateUIEffect;

        [Header("Language")] [SerializeField] private LanguageElementView languageElementPrefab;
        [SerializeField] private EnhancedScroller languageScroller;
        [SerializeField] private RectTransform languagePopup;
        [SerializeField] private UIButton buttonSelectLanguage;
        [SerializeField] private TextMeshProUGUI textNameLanguageSelected;
        [SerializeField, Array] private LocaleText[] langLocales;

        [Space] [SerializeField] private FloatVariable musicVolume;
        [SerializeField] private FloatVariable sfxVolume;
        [SerializeField] private ScriptableEventAudioHandle eventPauseMusic;
        [SerializeField] private ScriptableEventAudioHandle eventResumeMusic;
        [SerializeField] private ScriptableEventNoParam eventStopAllSfx;
        [SerializeField, PopupPickup] private string popupCredit;
#if UNITY_IOS
        [SerializeField] private Button buttonRestore;
        [SerializeField] private Pancake.IAP.ScriptableEventIAPNoParam restorePurchaseEvent;
#endif

        private Language _selectedLang;
        private RectTransform _languageScrollerRT;
        private bool _firstTimeActiveLanguage;

        protected override UniTask Initialize()
        {
            _languageScrollerRT = languageScroller.GetComponent<RectTransform>();
            buttonMusic.onClick.AddListener(OnButtonMusicPressed);
            buttonSound.onClick.AddListener(OnButtonSoundPressed);
            buttonVibrate.onClick.AddListener(OnButtonVibratePressed);
            buttonUpdate.onClick.AddListener(OnButtonUpdatePressed);
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonCredit.onClick.AddListener(OnButtonCreditPressed);
            buttonSelectLanguage.onClick.AddListener(OnButtonSelectLanguagePressed);
            languageScroller.Delegate = this;
            _selectedLang = Locale.CurrentLanguage;
            textNameLanguageSelected.text = _selectedLang.Name;
#if UNITY_IOS
            buttonRestore.onClick.AddListener(OnButtonRestorePressed);
#endif
            Refresh();
            return UniTask.CompletedTask;
        }

        private void OnButtonSelectLanguagePressed()
        {
            if (buttonSelectLanguage.AffectObject.localEulerAngles.z.Equals(0))
            {
                Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 90), 0.3f);
                InternalShowSelectLanguage();

                if (!_firstTimeActiveLanguage)
                {
                    _firstTimeActiveLanguage = true;
                    languageScroller.ReloadData();
                }
            }
            else
            {
                Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
                InternalHideSelectLanguage();
            }
        }

        private void OnButtonCreditPressed() { PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<CreditPopup>(popupCredit, true); }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnButtonSoundPressed()
        {
            bool state = sfxVolume.Value.Approximately(1);
            state = !state;
            if (state)
            {
                sfxVolume.Value = 1;
            }
            else
            {
                sfxVolume.Value = 0;
                eventStopAllSfx.Raise();
            }

            RefreshSoundState(state);
        }

        private void OnButtonVibratePressed()
        {
            bool state = Vibration.EnableVibration;
            state = !state;
            Vibration.EnableVibration = state;
            RefreshVibrateState(state);
        }

        private void OnButtonMusicPressed()
        {
            bool state = musicVolume.Value.Approximately(1);
            state = !state;
            if (state)
            {
                musicVolume.Value = 1;
                eventResumeMusic.Raise(AudioHandle.invalid);
            }
            else
            {
                musicVolume.Value = 0;
                eventPauseMusic.Raise(AudioHandle.invalid);
            }

            RefreshMusicState(state);
        }

        private void OnButtonUpdatePressed() { C.GotoStore(); }

#if UNITY_IOS
        private void OnButtonRestorePressed()
        {
            restorePurchaseEvent?.Raise();
        }
#endif

        private void Refresh()
        {
            localeTextVersion.UpdateArgs($"{Application.version}");
            bool vibrateState = Vibration.EnableVibration;

            RefreshMusicState(musicVolume.Value.Approximately(1));
            RefreshSoundState(sfxVolume.Value.Approximately(1));
            RefreshVibrateState(vibrateState);
        }

        private void RefreshVibrateState(bool vibrateState) { vibrateUIEffect.effectMode = vibrateState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshSoundState(bool soundState) { soundUIEffect.effectMode = soundState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshMusicState(bool musicState) { musicUIEffect.effectMode = musicState ? EffectMode.None : EffectMode.Grayscale; }
        public int GetNumberOfCells(EnhancedScroller scroller) { return LocaleSettings.Instance.AvailableLanguages.Count; }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) { return 120f; }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var element = scroller.GetCellView(languageElementPrefab) as LanguageElementView;
            if (element != null)
            {
                var code = LocaleSettings.Instance.AvailableLanguages[dataIndex];
                element.name = "Country_" + code.Name;
                element.Init(code,
                    OnButtonElementClicked,
                    IsElementSelected,
                    GetLocaleTextByIndex,
                    InternalHideSelectLanguage);
                return element;
            }

            return null;
        }

        private LocaleText GetLocaleTextByIndex(int arg) { return langLocales[arg]; }

        private void OnButtonElementClicked(LanguageElementView view)
        {
            _selectedLang = view.Lang;
            Locale.CurrentLanguage = _selectedLang;
            languageScroller.RefreshActiveCellViews();
            textNameLanguageSelected.text = view.Lang.Name;
        }

        private bool IsElementSelected(string code) { return _selectedLang.Code.Equals(code); }

        private void InternalShowSelectLanguage()
        {
            _languageScrollerRT.pivot = new Vector2(0.5f, 1f);
            languagePopup.gameObject.SetActive(true);
            languagePopup.SetSizeDeltaY(74f);
            Tween.UISizeDelta(languagePopup, new Vector2(languagePopup.sizeDelta.x, 666f), 0.5f)
                .OnComplete(() =>
                {
                    languageScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Always;
                    _languageScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
        }

        private void InternalHideSelectLanguage()
        {
            _languageScrollerRT.pivot = new Vector2(0.5f, 1f);
            languageScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Never;

            Tween.UISizeDelta(languagePopup, new Vector2(languagePopup.sizeDelta.x, 74f), 0.5f)
                .OnComplete(() =>
                {
                    languagePopup.gameObject.SetActive(false);
                    languageScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Always;
                    _languageScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
            Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
        }
    }
}