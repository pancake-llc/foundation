using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [Serializable]
    public class SkuCrossPlatform
    {
        [SerializeField] private bool overrideId;
        [SerializeField, HideIf(nameof(overrideId))] private string defaultId;
        [SerializeField, ShowIf(nameof(overrideId))] private string androidId;
        [SerializeField, ShowIf(nameof(overrideId))] private string iOSId;

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
        [InlineProperty, HideLabel] public SkuCrossPlatform sku;
        public ProductType productType;
    }

    [Serializable]
    public class InformationPurchaseResult
    {
        public string device;
        public string iapType;
        public string transactionId;
        public string productId;
        public int purchaseState;
        public long purchaseTime;
        public string purchaseToken;
        public string signature;
        public string deviceId;

        public InformationPurchaseResult(
            string device,
            string iapType,
            string transactionId,
            string productId,
            int purchaseState,
            long purchaseTime,
            string purchaseToken,
            string signature,
            string deviceId)
        {
            this.device = device;
            this.iapType = iapType;
            this.transactionId = transactionId;
            this.productId = productId;
            this.purchaseState = purchaseState;
            this.purchaseTime = purchaseTime;
            this.purchaseToken = purchaseToken;
            this.signature = signature;
            this.deviceId = deviceId;
        }
    }
}