#if PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [System.Serializable]
    public class AdmobBannerUnit : BannerAdUnit
    {
        public AdmobBannerUnit(string iOSId, string androidId) : base(iOSId, androidId) { }

        public AdmobBannerUnit() : base("", "") { }


#if PANCAKE_ADMOB
        public AdSize ConvertSize()
        {
            switch (size)
            {
                case EBannerSize.Adaptive: return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                default: return AdSize.Banner;
            }
        }

        public AdPosition ConvertPosition()
        {
            switch (position)
            {
                case EBannerPosition.Top: return AdPosition.Top;
                case EBannerPosition.Bottom: return AdPosition.Bottom;
                case EBannerPosition.TopLeft: return AdPosition.TopLeft;
                case EBannerPosition.TopRight: return AdPosition.TopRight;
                case EBannerPosition.BottomLeft: return AdPosition.BottomLeft;
                case EBannerPosition.BottomRight: return AdPosition.BottomRight;
                default: return AdPosition.Bottom;
            }
        }
#endif
    }
}
