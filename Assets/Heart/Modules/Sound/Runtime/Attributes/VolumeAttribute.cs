using UnityEngine;

namespace Pancake.Sound
{
    public class VolumeAttribute : PropertyAttribute
    {
        public readonly bool canBoost;

        public VolumeAttribute()
        {
#if UNITY_WEBGL
			canBoost = false;
#else
            canBoost = true;
#endif
        }

        public VolumeAttribute(bool canBoost)
        {
#if UNITY_WEBGL
			Debug.LogWarning(AudioConstant.LOG_HEADER + "Volume boosting is not supported in WebGL");
			canBoost = false;
#else
            this.canBoost = canBoost;
#endif
        }
    }
}