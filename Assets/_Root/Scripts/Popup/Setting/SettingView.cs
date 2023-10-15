using Coffee.UIEffects;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class SettingView : View
    {
        [SerializeField] private TextMeshProUGUI textVersion;
        [SerializeField] private Button buttonMusic;
        [SerializeField] private Button buttonSound;
        [SerializeField] private Button buttonVibrate;
        [SerializeField] private Button buttonUpdate;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonCredit;

        [SerializeField] private UIEffect musicUIEffect;
        [SerializeField] private UIEffect soundUIEffect;
        [SerializeField] private UIEffect vibrateUIEffect;

        [SerializeField] private FloatVariable musicVolume;
        [SerializeField] private FloatVariable sfxVolume;
        [SerializeField] private ScriptableEventAudioHandle eventPauseMusic;
        [SerializeField] private ScriptableEventAudioHandle eventResumeMusic;
        [SerializeField] private ScriptableEventNoParam eventStopAllSfx;
        [SerializeField, PopupPickup] private string popupCredit;
#if UNITY_IOS
        [SerializeField] private Button buttonRestore;
        [SerializeField] private Pancake.IAP.ScriptableEventIAPNoParam restorePurchaseEvent;
#endif

        protected override UniTask Initialize()
        {
            buttonMusic.onClick.AddListener(OnButtonMusicPressed);
            buttonSound.onClick.AddListener(OnButtonSoundPressed);
            buttonVibrate.onClick.AddListener(OnButtonVibratePressed);
            buttonUpdate.onClick.AddListener(OnButtonUpdatePressed);
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonCredit.onClick.AddListener(OnButtonCreditPressed);
#if UNITY_IOS
            buttonRestore.onClick.AddListener(OnButtonRestorePressed);
#endif
            Refresh();
            return UniTask.CompletedTask;
        }

        private void OnButtonCreditPressed()
        {
            PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<CreditPopup>(popupCredit, true);
        }

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
            textVersion.text = $"Version {Application.version}";
            bool vibrateState = Vibration.EnableVibration;

            RefreshMusicState(musicVolume.Value.Approximately(1));
            RefreshSoundState(sfxVolume.Value.Approximately(1));
            RefreshVibrateState(vibrateState);
        }

        private void RefreshVibrateState(bool vibrateState) { vibrateUIEffect.effectMode = vibrateState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshSoundState(bool soundState) { soundUIEffect.effectMode = soundState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshMusicState(bool musicState) { musicUIEffect.effectMode = musicState ? EffectMode.None : EffectMode.Grayscale; }
    }
}