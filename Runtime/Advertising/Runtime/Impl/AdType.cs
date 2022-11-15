namespace Pancake.Monetization
{
    public enum AdType
    {
        Banner = 0,
        Interstitial = 1,
        Rewarded = 2,
        RewardedInterstitial = 3,
        AppOpen = 4
    }

    public enum AdmobAdType
    {
        Banner = AdType.Banner,
        Interstitial = AdType.Interstitial,
        Rewarded = AdType.Rewarded,
        RewardedInterstitial = AdType.RewardedInterstitial,
        AppOpen = AdType.AppOpen
    }
}