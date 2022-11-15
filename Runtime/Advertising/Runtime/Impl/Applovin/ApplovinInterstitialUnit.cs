using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinInterstitialUnit : InterstitialAdUnit
    {
        public ApplovinInterstitialUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}