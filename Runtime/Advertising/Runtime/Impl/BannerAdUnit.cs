using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class BannerAdUnit : AdUnit
    {
        public EBannerPosition position = EBannerPosition.Bottom;

        public BannerAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}