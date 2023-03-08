namespace Pancake.Monetization
{
    [System.Serializable]
    public class AdmobRewardedInterstitialUnit : RewardedInterstitialAdUnit
    {
        public AdmobRewardedInterstitialUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}