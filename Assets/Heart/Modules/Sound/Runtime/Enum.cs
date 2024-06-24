using UnityEngine;

namespace Pancake.Sound
{
    public enum EAudioTrackType
    {
        Generic,
        Dominator,
    }

    [System.Flags]
    public enum EAudioType
    {
        None = 0,

        Music = 1 << 0,
        UI = 1 << 1,
        Ambience = 1 << 2,
        Sfx = 1 << 3,
        VoiceOver = 1 << 4,

        All = Music | UI | Ambience | Sfx | VoiceOver,
    }

    [System.Flags]
    public enum EEffectType
    {
        None = 0,

        Volume = 1 << 0,
        LowPass = 1 << 1,
        HighPass = 1 << 2,
        Custom = 1 << 3,

        All = Volume | LowPass | HighPass | Custom,
    }

    public enum EAudioPlayMode
    {
        Single, // Always play the first clip
        Sequence, // Play clips sequentially
        Random, // Play clips randomly with the given weight
    }

    public enum EFilterSlope
    {
#if UNITY_2019_2_OR_NEWER
        [InspectorName("12dB \u2215 Oct")]
#endif
        TwoPole,
#if UNITY_2019_2_OR_NEWER
        [InspectorName("24dB \u2215 Oct")]
#endif
        FourPole,
    }

    public enum EMonoConversionMode
    {
        Downmixing = 0,
        Left = 1,
        Right = 2,
    }

    public enum EPitchShiftingSetting
    {
        AudioMixer,
        AudioSource,
    }

    [System.Flags]
    public enum ERandomFlags
    {
        None = 0,
        Pitch = 1 << 0,
        Volume = 1 << 1,
    }

    public enum ESetEffectMode
    {
        Add,
        Remove,
        Override,
    }

    public enum EAudioStopMode
    {
        /// <summary>
        /// Stop the current playback, and it will restart if played again.
        /// </summary>
        Stop,

        /// <summary>
        /// Pause the current playback, and it will resume from where it was paused if played again.
        /// </summary>
        Pause,

        /// <summary>
        /// Mute the current playback, it will keep playing in the background until it's played(Unmuted) again. 
        /// </summary>
        Mute,
    }

    public enum EAudioTransition
    {
        /// <summary>
        /// Follow the clip's setting in LibraryManager.
        /// </summary>
        Default,

        /// <summary>
        /// Ignore the clip's setting in LibraryManger.Stop immediately and play immediately.
        /// </summary>
        Immediate,

        /// <summary>
        /// Stop immediately, and play with the clip's FadeIn setting in LibraryManger.
        /// </summary>
        OnlyFadeIn,

        /// <summary>
        /// Stop with the clip's FadeOut setting in LibraryManger, and play immediately.
        /// </summary>
        OnlyFadeOut,

        /// <summary>
        /// Stop previous and play new one at the same time , and do both fade in and fade out.   
        /// </summary>
        CrossFade
    }

    public enum ETransportType
    {
        Start = 1 << 0,
        End = 1 << 1,
        Delay = 1 << 2,
        FadeIn = 1 << 3,
        FadeOut = 1 << 4,
    }

    [System.Flags]
    public enum EVolumeSliderOptions
    {
        Slider = 0,
        Label = 1 << 0,
        Field = 1 << 1,
        VuMeter = 1 << 2,
        SnapFullVolume = 1 << 3,
    }

    public enum ESpatialPropertyType
    {
        StereoPan,
        DopplerLevel,
        MinDistance,
        MaxDistance,
        SpatialBlend,
        ReverbZoneMix,
        Spread,
        CustomRolloff,
        RolloffMode,
    }
}