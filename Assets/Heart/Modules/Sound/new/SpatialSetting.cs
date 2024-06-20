using UnityEngine;

namespace Pancake.Sound
{
    public class SpatialSetting : ScriptableObject
    {
        public float stereoPan = SoundConstant.DEFAULT_PAN_STEREO;
        public float dopplerLevel = SoundConstant.DEFAULT_DOPPLER;
        public float minDistance = SoundConstant.ATTENUATION_MIN_DISTANCE;
        public float maxDistance = SoundConstant.ATTENUATION_MAX_DISTANCE;

        public AnimationCurve spatialBlend;
        public AnimationCurve reverbZoneMix;
        public AnimationCurve spread;
        public AnimationCurve customRolloff;
        public AudioRolloffMode rolloffMode = SoundConstant.DEFAULT_ROLLOFF_MODE;
    }
}