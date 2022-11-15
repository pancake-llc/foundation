using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class AppOpenAdUnit : AdUnit
    {
        public AppOpenAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}