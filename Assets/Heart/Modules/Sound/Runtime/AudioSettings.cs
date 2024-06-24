using LitMotion;

namespace Pancake.Sound
{
    [EditorIcon("so_blue_setting")]
    public sealed class AudioSettings : ScriptableSettings<AudioSettings>
    {
        public float combFilteringPreventionInSeconds = 0.04f;
        public bool logCombFilteringWarning = true;
        public bool logAccessRecycledPlayerWarning;
        public Ease defaultFadeInEase = Ease.InCubic;
        public Ease defaultFadeOutEase = Ease.OutSine;
        public Ease seamlessFadeInEase = Ease.OutCubic;
        public Ease seamlessFadeOutEase = Ease.OutSine;
        public EFilterSlope audioFilterSlope = EFilterSlope.FourPole;
        public int defaultAudioPlayerPoolSize = 5;
        public EPitchShiftingSetting pitchSetting = EPitchShiftingSetting.AudioSource;
        public bool alwaysPlayMusicAsBGM = true;
        public EAudioTransition defaultBGMTransition = EAudioTransition.CrossFade;
        public float defaultBGMTransitionTime = 2;

#if UNITY_EDITOR
        public static void ResetToFactorySettings()
        {
            Instance.combFilteringPreventionInSeconds = 0.04f;
            Instance.logCombFilteringWarning = true;
            Instance.defaultFadeInEase = Ease.InCubic;
            Instance.defaultFadeOutEase = Ease.OutSine;
            Instance.seamlessFadeInEase = Ease.OutCubic;
            Instance.seamlessFadeOutEase = Ease.OutSine;
            Instance.audioFilterSlope = EFilterSlope.FourPole;
            Instance.defaultAudioPlayerPoolSize = 5;
            Instance.pitchSetting = EPitchShiftingSetting.AudioSource;
            Instance.alwaysPlayMusicAsBGM = true;
            Instance.defaultBGMTransition = EAudioTransition.CrossFade;
            Instance.defaultBGMTransitionTime = 2;
        }
#endif

        public static float CombFilteringPreventionInSeconds
        {
            get => Instance.combFilteringPreventionInSeconds;
            set => Instance.combFilteringPreventionInSeconds = value;
        }

        public static bool LogCombFilteringWarning { get => Instance.logCombFilteringWarning; set => Instance.logCombFilteringWarning = value; }
        public static bool LogAccessRecycledPlayerWarning { get => Instance.logAccessRecycledPlayerWarning; set => Instance.logAccessRecycledPlayerWarning = value; }
        public static Ease DefaultFadeInEase { get => Instance.defaultFadeInEase; set => Instance.defaultFadeInEase = value; }
        public static Ease DefaultFadeOutEase { get => Instance.defaultFadeOutEase; set => Instance.defaultFadeOutEase = value; }
        public static Ease SeamlessFadeInEase { get => Instance.seamlessFadeInEase; set => Instance.seamlessFadeInEase = value; }
        public static Ease SeamlessFadeOutEase { get => Instance.seamlessFadeOutEase; set => Instance.seamlessFadeOutEase = value; }
        public static EFilterSlope AudioFilterSlope { get => Instance.audioFilterSlope; set => Instance.audioFilterSlope = value; }
        public static int DefaultAudioPlayerPoolSize { get => Instance.defaultAudioPlayerPoolSize; set => Instance.defaultAudioPlayerPoolSize = value; }
        public static EPitchShiftingSetting PitchSetting { get => Instance.pitchSetting; set => Instance.pitchSetting = value; }
        public static bool AlwaysPlayMusicAsBGM { get => Instance.alwaysPlayMusicAsBGM; set => Instance.alwaysPlayMusicAsBGM = value; }
        public static EAudioTransition DefaultBGMTransition { get => Instance.defaultBGMTransition; set => Instance.defaultBGMTransition = value; }
        public static float DefaultBGMTransitionTime { get => Instance.defaultBGMTransitionTime; set => Instance.defaultBGMTransitionTime = value; }
    }
}