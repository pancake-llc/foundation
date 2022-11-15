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
                return AndroidId;
#elif UNITY_IOS
                return IosId;
#else
                return string.Empty;
#endif
            }
        }

        /// <summary>
        /// Gets the ad ID for iOS platform.
        /// </summary>
        /// <value>The ios identifier.</value>
        public virtual string IosId => iOSId;

        /// <summary>
        /// Gets the ad ID for Android platform.
        /// </summary>
        /// <value>The android identifier.</value>
        public virtual string AndroidId => androidId;

        public AdUnit(string iOSId, string androidId)
        {
            this.iOSId = iOSId;
            this.androidId = androidId;
        }

        public override string ToString() { return Id; }

        public override bool Equals(object obj)
        {
            var item = obj as AdUnit;
            if (item == null) return false;
            return Id.Equals(item.Id);
        }

        public override int GetHashCode() { return Id.GetHashCode(); }

        public static bool operator ==(AdUnit a, AdUnit b)
        {
            if (ReferenceEquals(a, null)) return ReferenceEquals(b, null);

            return a.Equals(b);
        }

        public static bool operator !=(AdUnit a, AdUnit b) { return !(a == b); }
    }
}