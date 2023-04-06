using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public abstract class AdUnit
    {
        [SerializeField] protected string androidId;
        [SerializeField] protected string iOSId;

        /// <summary>
        /// Gets the ad ID corresponding to the current platform.
        /// Returns <c>string.Empty</c> if no ID was defined for this platform.
        /// </summary>
        /// <value>The identifier.</value>
        public virtual string Id
        {
            get
            {
#if UNITY_ANDROID
                return androidId;
#elif UNITY_IOS
                return iOSId;
#else
                return string.Empty;
#endif
            }
        }

        protected AdUnit(string androidId, string iOSId)
        {
            this.androidId = androidId;
            this.iOSId = iOSId;
        }
    }
}