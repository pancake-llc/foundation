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

        void ShowBannerAd();
        void HideBannerAd();
        void DestroyBannerAd();

        #endregion

        #region interstitial ad

        event Action<IAdClient> OnInterstitialAdCompleted;
        void LoadInterstitialAd();
        bool IsInterstitialAdReady();
        void ShowInterstitialAd();

        #endregion

        #region rewarded ad

        event Action<IAdClient> OnRewardedAdSkipped;
        event Action<IAdClient> OnRewardedAdCompleted;
        void LoadRewardedAd();
        bool IsRewardedAdReady();
        void ShowRewardedAd();

        #endregion

        #region rewarded interstitital ad

        event Action<IAdClient> OnRewardedInterstitialAdSkipped;
        event Action<IAdClient> OnRewardedInterstitialAdCompleted;
        void LoadRewardedInterstitialAd();
        bool IsRewardedInterstitialAdReady();
        void ShowRewardedInterstitialAd();

        #endregion

        #region app open ad

        event Action<IAdClient> OnAppOpenAdCompleted;
        void LoadAppOpenAd();
        bool IsAppOpenAdReady();
        void ShowAppOpenAd();

        #endregion

        #region privacy

        void ShowConsentForm();

        #endregion
    }
}