using UnityEngine;

namespace Pancake.Sound
{
    public static class SoundConstant
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

        public static float DecibelVoulumeFullScale => MAX_DECIBEL_VOLUME - MIN_DECIBEL_VOLUME;
    }
}