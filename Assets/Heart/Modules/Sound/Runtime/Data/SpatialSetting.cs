using UnityEngine;

namespace Pancake.Sound
{
    public class SpatialSetting : ScriptableObject
    {
        public float stereoPan = AudioConstant.DEFAULT_PAN_STEREO;
        public float dopplerLevel = AudioConstant.DEFAULT_DOPPLER;
        public float minDistance = AudioConstant.ATTENUATION_MIN_DISTANCE;
        public float maxDistance = AudioConstant.ATTENUATION_MAX_DISTANCE;

        public AnimationCurve spatialBlend;
        public AnimationCurve reverbZoneMix;
        public AnimationCurve spread;
        public AnimationCurve customRolloff;
        public AudioRolloffMode rolloffMode = AudioConstant.DEFAULT_ROLLOFF_MODE;
    }
}