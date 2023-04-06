using System;

namespace Pancake.Monetization
{
    public interface IAdClient
    {
        EAdNetwork Network { get; }
        bool IsBannerAdSupported { get; }
        bool IsInsterstitialAdSupport { get; }
        bool IsRewardedAdSupport { get; }
        bool IsRewardedInterstitialAdSupport { get; }
        bool IsAppOpenAdSupport { get; }
        bool IsSdkAvaiable { get; }
        bool IsInitialized { get; }
        void Initialize();
        void RegisterAppStateChange();

        #region banner ad

        event Action<IAdClient> OnBannerAdDisplayed;
        event Action<IAdClient> OnBannerAdCompleted;
        event Action<double, string, string, string, EAdNetwork> OnBannerAdPaid;
        void ShowBannerAd();
        void DestroyBannerAd();

        #endregion

        #region interstitial ad

        event Action<IAdClient> OnInterstitialAdCompleted;
        event Action<IAdClient> OnInterstitialAdDisplayed;
        event Action<double, string, string, string, EAdNetwork> OnInterstitialAdPaid;
        void LoadInterstitialAd();
        bool IsInterstitialAdReady();
        IInterstitial ShowInterstitialAd();

        #endregion

        #region rewarded ad

        event Action<IAdClient> OnRewardedAdSkipped;
        event Action<IAdClient> OnRewardedAdCompleted;
        event Action<IAdClient> OnRewardedAdClosed;
        event Action<IAdClient> OnRewardedAdDisplayed;
        event Action<double, string, string, string, EAdNetwork> OnRewardedAdPaid;
        void LoadRewardedAd();
        bool IsRewardedAdReady();
        IRewarded ShowRewardedAd();

        #endregion

        #region rewarded interstitital ad

        event Action<IAdClient> OnRewardedInterAdSkipped;
        event Action<IAdClient> OnRewardedInterAdCompleted;
        event Action<IAdClient> OnRewardedInterAdClosed;
        event Action<IAdClient> OnRewardedInterAdDisplayed;
        event Action<double, string, string, string, EAdNetwork> OnRewardedInterAdPaid;
        void LoadRewardedInterAd();
        bool IsRewardedInterAdReady();
        IRewardedInterstitial ShowRewardedInterAd();

        #endregion

        #region app open ad

        event Action<IAdClient> OnAppOpenAdCompleted;
        event Action<IAdClient> OnAppOpenAdDisplayed;
        event Action<double, string, string, string, EAdNetwork> OnAppOpenAdPaid;
        void LoadAppOpenAd();
        bool IsAppOpenAdReady();
        void ShowAppOpenAd();

        #endregion
    }
}