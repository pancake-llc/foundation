using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class InterstitialAdUnit : AdUnit
    {
        public InterstitialAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}