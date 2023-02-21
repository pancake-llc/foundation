#if PANCAKE_ADS
using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class BannerAdUnit : AdUnit
    {
        public EBannerPosition position;

        public BannerAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
            position = EBannerPosition.Bottom; // default set banner display in bottom
        }
    }
}
#endif