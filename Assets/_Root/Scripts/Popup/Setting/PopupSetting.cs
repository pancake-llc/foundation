using Coffee.UIEffects;
using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class PopupSetting : GameComponent
    {
        [SerializeField, Foldout("Setting", Style = "Group")] private TextMeshProUGUI textVersion;
        [SerializeField, Foldout("Setting", Style = "Group")] private Button buttonMusic;
        [SerializeField, Foldout("Setting", Style = "Group")] private Button buttonSound;
        [SerializeField, Foldout("Setting", Style = "Group")] private Button buttonVibrate;
        [SerializeField, Foldout("Setting", Style = "Group")] private Button buttonUpdate;

        [SerializeField, Foldout("Setting", Style = "Group")] private ScriptableEventAudioHandle eventPauseMusic;
        [SerializeField, Foldout("Setting", Style = "Group")] private ScriptableEventAudioHandle eventResumeMusic;
        [SerializeField, Foldout("Setting", Style = "Group")] private FloatVariable musicVolume;
        [SerializeField, Foldout("Setting", Style = "Group")] private ScriptableEventNoParam eventStopAllSfx;
        [SerializeField, Foldout("Setting", Style = "Group")] private FloatVariable sfxVolume;
#if UNITY_IOS
        [SerializeField, Foldout("Setting", Style = "Group")] private Button buttonRestore;
        [SerializeField, Foldout("Setting", Style = "Group")] private Pancake.IAP.ScriptableEventIAPNoParam restorePurchaseEvent;
#endif

        private bool _initialized;


        private UIEffect _musicUIEffect;
        private UIEffect _soundUIEffect;
        private UIEffect _vibrateUIEffect;

//         public override void Init()
//         {
//             if (!_initialized)
//             {
//                 _initialized = true;
//                 buttonMusic.onClick.AddListener(OnButtonMusicPressed);
//                 buttonSound.onClick.AddListener(OnButtonSoundPressed);
//                 buttonVibrate.onClick.AddListener(OnButtonVibratePressed);
//                 buttonUpdate.onClick.AddListener(OnButtonUpdatePressed);
// #if UNITY_IOS
//                 buttonRestore.onClick.AddListener(OnButtonRestorePressed);
// #endif
//                 buttonMusic.TryGetComponent(out _musicUIEffect);
//                 buttonSound.TryGetComponent(out _soundUIEffect);
//                 buttonVibrate.TryGetComponent(out _vibrateUIEffect);
//             }
//
//             Refresh();
//         }
//
//         private void OnButtonMusicPressed()
//         {
//             bool state = musicVolume.Value.Approximately(1);
//             state = !state;
//             if (state)
//             {
//                 musicVolume.Value = 1;
//                 eventResumeMusic.Raise(AudioHandle.invalid);
//             }
//             else
//             {
//                 musicVolume.Value = 0;
//                 eventPauseMusic.Raise(AudioHandle.invalid);
//             }
//
//             RefreshMusicState(state);
//         }
//
//         private void OnButtonSoundPressed()
//         {
//             bool state = sfxVolume.Value.Approximately(1);
//             state = !state;
//             if (state)
//             {
//                 sfxVolume.Value = 1;
//             }
//             else
//             {
//                 sfxVolume.Value = 0;
//                 eventStopAllSfx.Raise();
//             }
//
//             RefreshSoundState(state);
//         }
//
//         private void OnButtonVibratePressed()
//         {
//             bool state = Vibration.EnableVibration;
//             state = !state;
//             Vibration.EnableVibration = state;
//             RefreshVibrateState(state);
//         }
//
//         private void OnButtonUpdatePressed() { C.GotoStore(); }
//
// #if UNITY_IOS
//         private void OnButtonRestorePressed()
//         {
//             restorePurchaseEvent?.Raise();
//         }
// #endif
//
//         private void Refresh()
//         {
//             textVersion.text = $"Version {Application.version}";
//             bool vibrateState = Vibration.EnableVibration;
//
//             RefreshMusicState(musicVolume.Value.Approximately(1));
//             RefreshSoundState(sfxVolume.Value.Approximately(1));
//             RefreshVibrateState(vibrateState);
//         }
//
//         private void RefreshVibrateState(bool vibrateState) { _vibrateUIEffect.effectMode = vibrateState ? EffectMode.None : EffectMode.Grayscale; }
//
//         private void RefreshSoundState(bool soundState) { _soundUIEffect.effectMode = soundState ? EffectMode.None : EffectMode.Grayscale; }
//
//         private void RefreshMusicState(bool musicState) { _musicUIEffect.effectMode = musicState ? EffectMode.None : EffectMode.Grayscale; }
    }
}