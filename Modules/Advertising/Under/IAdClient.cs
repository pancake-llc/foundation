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
        void ShowBannerAd();
        void DestroyBannerAd();

        #endregion

        #region interstitial ad

        event Action<IAdClient> OnInterstitialAdCompleted;
        event Action<IAdClient> OnInterstitialAdDisplayed;
        void LoadInterstitialAd();
        bool IsInterstitialAdReady();
        IInterstitial ShowInterstitialAd();

        #endregion

        #region rewarded ad

        event Action<IAdClient> OnRewardedAdSkipped;
        event Action<IAdClient> OnRewardedAdCompleted;
        event Action<IAdClient> OnRewardedAdClosed;
        event Action<IAdClient> OnRewardedAdDisplayed;
        void LoadRewardedAd();
        bool IsRewardedAdReady();
        IRewarded ShowRewardedAd();

        #endregion

        #region rewarded interstitital ad

        event Action<IAdClient> OnRewardedInterstitialAdSkipped;
        event Action<IAdClient> OnRewardedInterstitialAdCompleted;
        event Action<IAdClient> OnRewardedInterstitialAdClosed;
        event Action<IAdClient> OnRewardedInterstitialAdDisplayed;
        void LoadRewardedInterstitialAd();
        bool IsRewardedInterstitialAdReady();
        IRewardedInterstitial ShowRewardedInterstitialAd();

        #endregion

        #region app open ad

        event Action<IAdClient> OnAppOpenAdCompleted;
        event Action<IAdClient> OnAppOpenAdDisplayed;
        void LoadAppOpenAd();
        bool IsAppOpenAdReady();
        void ShowAppOpenAd();

        #endregion

        #region privacy

        void ShowConsentForm();

        #endregion
    }
}