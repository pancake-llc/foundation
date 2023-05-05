using System;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [Serializable]
    public class SkuCrossPlatform
    {
        [SerializeField] private bool overrideId;
        [SerializeField, HideIf(nameof(overrideId))] private string defaultId;
        [SerializeField, ShowIf(nameof(overrideId)), Color(0.3f, 0.5f, 0.65f, 0.5f, Target = ColorTarget.Background)] private string androidId;
        [SerializeField, ShowIf(nameof(overrideId)), Color(0.64f, 0.6f, 0.3f, 0.5f, Target = ColorTarget.Background)] private string iOSId;

        public virtual string Id
        {
            get
            {
                if (!OverrideId) return DefaultId;

#if UNITY_ANDROID
                return AndroidId;
#elif UNITY_IOS
                return IosId;
#else
                return string.Empty;
#endif
            }
        }

        public virtual string IosId => iOSId;

        public virtual string AndroidId => androidId;

        public string DefaultId => defaultId;

        public bool OverrideId => overrideId;

        public SkuCrossPlatform(string androidId, string iOSId)
        {
            this.androidId = androidId;
            this.iOSId = iOSId;
        }
    }

    [Serializable]
    public class IAPData
    {
        [InlineEditor, HideLabel] public SkuCrossPlatform sku;
        public ProductType productType;
    }
}