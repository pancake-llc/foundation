namespace Pancake.Monetization
{
    [System.Serializable]
    public class ApplovinInterstitialUnit : InterstitialAdUnit
    {
        public ApplovinInterstitialUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}