#if PANCAKE_ADS
using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobIntestitialUnit : InterstitialAdUnit
    {
        public AdmobIntestitialUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}
#endif