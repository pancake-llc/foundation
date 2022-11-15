using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinBannerUnit : BannerAdUnit
    {
        [Tooltip("if you are using adaptive banner, you will need to also integrate AdMob " +
                 "(as it is a Google ad format and the AdSize.getCurrentOrientationAnchoredAdaptiveBannerAdSize() is from the AdMob SDK).")]
        public bool useAdaptiveBanner;

        public ApplovinBannerUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }

#if PANCAKE_MAX_ENABLE
        public MaxSdkBase.BannerPosition ConvertPosition()
        {
            switch (position)
            {
                case EBannerPosition.Top: return MaxSdkBase.BannerPosition.TopCenter;
                case EBannerPosition.Bottom: return MaxSdkBase.BannerPosition.BottomCenter;
                case EBannerPosition.TopLeft: return MaxSdkBase.BannerPosition.TopLeft;
                case EBannerPosition.TopRight: return MaxSdkBase.BannerPosition.TopRight;
                case EBannerPosition.BottomLeft: return MaxSdkBase.BannerPosition.BottomLeft;
                case EBannerPosition.BottomRight: return MaxSdkBase.BannerPosition.BottomRight;
                default:
                    return MaxSdkBase.BannerPosition.BottomCenter;
            }
        }
#endif
    }
}