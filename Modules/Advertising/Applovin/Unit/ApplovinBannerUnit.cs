namespace Pancake.Monetization
{
    [System.Serializable]
    public class ApplovinBannerUnit : BannerAdUnit
    {
        public ApplovinBannerUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
            position = EBannerPosition.Bottom;
            size = EBannerSize.Adaptive;
        }

#if PANCAKE_APPLOVIN
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