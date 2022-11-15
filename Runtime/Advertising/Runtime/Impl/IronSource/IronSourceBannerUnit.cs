using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class IronSourceBannerUnit
    {
        public EBannerSize size;
        public EBannerPosition position;

        public IronSourceBannerUnit()
        {
            position = EBannerPosition.Bottom;
            size = EBannerSize.Banner;
        }


#if PANCAKE_IRONSOURCE_ENABLE
        public IronSourceBannerSize ConvertSize()
        {
            switch (size)
            {
                case EBannerSize.Banner: return IronSourceBannerSize.BANNER;
                case EBannerSize.FullBanner:
                case EBannerSize.Leaderboard:
                case EBannerSize.LargeBanner: return IronSourceBannerSize.LARGE;
                case EBannerSize.MediumRectangle: return IronSourceBannerSize.RECTANGLE;
                case EBannerSize.SmartBanner: return IronSourceBannerSize.SMART;
                default: return IronSourceBannerSize.SMART;
            }
        }

        public IronSourceBannerPosition ConvertPosition()
        {
            switch (position)
            {
                case EBannerPosition.Top:
                case EBannerPosition.TopLeft:
                case EBannerPosition.TopRight: return IronSourceBannerPosition.TOP;
                case EBannerPosition.Bottom:
                case EBannerPosition.BottomLeft:
                case EBannerPosition.BottomRight: return IronSourceBannerPosition.BOTTOM;
                default: return IronSourceBannerPosition.BOTTOM;
            }
        }
#endif
    }
}