using LitMotion;
using UnityEngine;

namespace Pancake.Sound
{
    public static class AudioConstant
    {
        /// <summary>
        /// The normalized minimum volume that Unity AudioMixer can reach
        /// </summary>
        public const float MIN_VOLUME = 0.0001f;

        /// <summary>
        /// The normalized full (or default) volume
        /// </summary>
        public const float FULL_VOLUME = 1f;

        /// <summary>
        /// The normalized maximum volume that Unity AudioMixer can reach
        /// </summary>
        public const float MAX_VOLUME = 10f;

        public const float DEFAULT_DECIBEL_VOLUME_SCALE = 20f;

        /// <summary>
        /// The minimum volume that Unity AudioMixer can reach in dB.
        /// </summary>
        public const float MIN_DECIBEL_VOLUME = -80f; // DefaultDecibelVolumeScale * Log10(MinVolume)

        /// <summary>
        /// The full (or default) volume in dB.
        /// </summary>
        public const float FULL_DECIBEL_VOLUME = 0f; // DefaultDecibelVolumeScale * Log10(FullVolume)

        /// <summary>
        /// The maximum volume that Unity AudioMixer can reach in dB.
        /// </summary>
        public const float MAX_DECIBEL_VOLUME = 20f; // DefaultDecibelVolumeScale * Log10(MaxVolume)

        /// <summary>
        /// The maximum sound frequency in Hz. (base on Unity's audio mixer effect like Lowpass/Highpass)
        /// </summary>
        public const float MAX_FREQUENCY = 22000f;

        /// <summary>
        /// The minimum sound frequency in Hz. (base on Unity's audio mixer effect like Lowpass/Highpass)
        /// </summary>
        public const float MIN_FREQUENCY = 10f;

        public const int SECOND_IN_MILLISECONDS = 1000;
        public const float MILLISECOND_IN_SECONDS = 0.001f;

        // Base on AuidoSource default values
        public const float DEFAULT_DOPPLER = 1f;
        public const float ATTENUATION_MIN_DISTANCE = 1f;
        public const float ATTENUATION_MAX_DISTANCE = 500f;
        public const float SPATIAL_BLEND_3D = 1f;
        public const float SPATIAL_BLEND_2D = 0f;
        public const float DEFAULT_PITCH = 1f; // The default pitch for both AudioSource and AudioMixer.
        public const float MIN_AUDIO_SOURCE_PITCH = 0.1f; // todo: values under 0 is not supported currently. Might support in the future to achieve that reverse feature.
        public const float MAX_AUDIO_SOURCE_PITCH = 3f;
        public const float MIN_MIXER_PITCH = 0.1f;
        public const float MAX_MIXER_PITCH = 10f;
        public const int DEFAULT_PRIORITY = 128;
        public const int HIGHEST_PRIORITY = 0;
        public const float LOWEST_PRIORITY = 256;
        public const float DEFAULT_SPREAD = 0f;
        public const float DEFAULT_REVER_ZONE_MIX = 1f;
        public const float DEFAULT_PAN_STEREO = 0f;
        public const AudioRolloffMode DEFAULT_ROLLOFF_MODE = AudioRolloffMode.Logarithmic;

        public const float FADE_TIME_IMMEDIATE = 0f;
        public const float FADE_TIME_QUICK = 0.5f;
        public const float FADE_TIME_SMOOTH = 1f;

        public const float LOW_PASS_FREQUENCY = 300f;
        public const float HIGH_PASS_FREQUENCY = 2000f;

        public const Ease VOLUME_INCREASE_EASE = Ease.InCubic;
        public const Ease VOLUME_DECREASE_EASE = Ease.OutSine;

        public const Ease LOW_PASS_IN_EASE = Ease.OutCubic;
        public const Ease LOW_PASS_OUT_EASE = Ease.InCubic;
        public const Ease HIGH_PASS_IN_EASE = Ease.InCubic;
        public const Ease HIGH_PASS_OUT_EASE = Ease.OutCubic;

        public const int VIRTUAL_TRACK_COUNT = 4;

        public static float DecibelVoulumeFullScale => MAX_DECIBEL_VOLUME - MIN_DECIBEL_VOLUME;

        public const string LOG_HEADER = "<b><color=#FF77C6>[Audio] </color></b>";

        public const string TEMP_ASSET_NAME = "Temp";
        public const string AUDIO_PLAYER_PREFAB_NAME = "AudioPlayer";


        #region Audio Mixer

        public const string MIXER_NAME = "MainAudioMixer";
        public const string MASTER_TRACK_NAME = "Master";
        public const string GENERIC_TRACK_NAME = "Track";
        public const string MAIN_TRACK_NAME = "Main";
        public const string MAIN_DOMINATED_TRACK_NAME = "Main_Dominated";
        public const string EFFECT_TRACK_NAME = "Effect";
        public const string DOMINATOR_TRACK_NAME = "Dominator";

        #endregion

        #region Exposed Parameters Name

        public const string EFFECT_PARA_NAME_SUFFIX = "_Effect";
        public const string PITCH_PARA_NAME_SUFFIX = "_Pitch";
        public const string LOW_PASS_PARA_NAME_SUFFIX = "_LowPass";
        public const string HIGH_PASS_PARA_NAME_SUFFIX = "_HighPass";

        public const string LOW_PASS_PARA_NAME = EFFECT_TRACK_NAME + LOW_PASS_PARA_NAME_SUFFIX;
        public const string HIGH_PASS_PARA_NAME = EFFECT_TRACK_NAME + HIGH_PASS_PARA_NAME_SUFFIX;

        public const string DOMINATOR_LOW_PASS_PARA_NAME = MAIN_TRACK_NAME + LOW_PASS_PARA_NAME_SUFFIX;
        public const string DOMINATOR_HIGH_PASS_PARA_NAME = MAIN_TRACK_NAME + HIGH_PASS_PARA_NAME_SUFFIX;

        #endregion
    }
}