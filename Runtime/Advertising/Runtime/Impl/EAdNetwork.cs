#if PANCAKE_ADS
namespace Pancake.Monetization
{
    public enum EAdNetwork
    {
        None = 0,
        Admob = 1,
        Applovin = 2
    }

    public enum EBannerAdNetwork
    {
        None = EAdNetwork.None,
        Admob = EAdNetwork.Admob,
        Applovin = EAdNetwork.Applovin,
    }

    public enum EInterstitialAdNetwork
    {
        None = EAdNetwork.None,
        Admob = EAdNetwork.Admob,
        Applovin = EAdNetwork.Applovin
    }

    public enum ERewardedAdNetwork
    {
        None = EAdNetwork.None,
        Admob = EAdNetwork.Admob,
        Applovin = EAdNetwork.Applovin
    }

    public enum ERewardedInterstitialAdNetwork
    {
        None = EAdNetwork.None,
        Admob = EAdNetwork.Admob,
        Applovin = EAdNetwork.Applovin
    }

    public enum EAppOpenAdNetwork
    {
        None = EAdNetwork.None,
        Admob = EAdNetwork.Admob,
        Applovin = EAdNetwork.Applovin
    }
}
#endif