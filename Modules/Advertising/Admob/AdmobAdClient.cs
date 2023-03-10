#if PANCAKE_ADMOB
using System;
using GoogleMobileAds.Api;
#endif


namespace Pancake.Monetization
{
    public class AdmobAdClient : AdClient
    {
        private const string NO_SDK_MESSAGE = "SDK missing. Please import the AdMob (Google Mobile Ads) plugin.";
        private AdmobBannerLoader _banner;
        private AdmobInterstitialLoader _interstitial;
        private AdmobRewardedLoader _rewarded;
        private AdmobRewardedInterstitialLoader _rewardedInterstitial;
        private AdmobAppOpenLoader _appOpen;
        private static AdmobAdClient client;
        internal Action interstitialDisplayChain;
        internal Action interstitialCompletedChain;
        internal Action rewardedCompletedChain;
        internal Action rewardedDisplayChain;
        internal Action rewardedSkippedChain;
        internal Action rewardedClosedChain;
        public static AdmobAdClient Instance => client ?? (client = new AdmobAdClient());

        public override EAdNetwork Network => EAdNetwork.Admob;
        public override bool IsBannerAdSupported => true;
        public override bool IsInsterstitialAdSupport => true;
        public override bool IsRewardedAdSupport => true;
        public override bool IsRewardedInterstitialAdSupport => true;
        public override bool IsAppOpenAdSupport => true;

        public override bool IsSdkAvaiable
        {
            get
            {
#if PANCAKE_ADMOB
                return true;
#else
                return false;
#endif
            }
        }

        protected override string NoSdkMessage => NO_SDK_MESSAGE;

        public AdmobBannerLoader Banner => _banner;

        public AdmobInterstitialLoader Interstitial => _interstitial;

        public AdmobRewardedLoader Rewarded => _rewarded;

        public AdmobRewardedInterstitialLoader RewardedInterstitial => _rewardedInterstitial;
        public AdmobAppOpenLoader AppOpen => _appOpen;

        protected override void InternalInit()
        {
#if PANCAKE_ADMOB
            MobileAds.Initialize(_ =>
            {
                Runtime.RunOnMainThread(() =>
                {
                    if (AdSettings.AdmobSettings.EnableTestMode) Admob.SetupDeviceTest();
                    if (AdSettings.AdCommonSettings.EnableGDPR) ShowConsentForm();
                    isInitialized = true;
                });
            });


#endif
        }

#if PANCAKE_ADMOB


        #region banner

        public event Action OnBannerAdLoaded;
        public event Action<LoadAdError> OnBannerAdFaildedToLoad;
        public event Action<AdValue> OnBannerAdPaided;


        internal void InvokeBannerAdLoaded() { OnBannerAdLoaded?.Invoke(); }
        internal void InvokeBannerAdFailedToLoad(LoadAdError error) { OnBannerAdFaildedToLoad?.Invoke(error); }
        internal void InvokeBannerAdDisplayed() { CallBannerAdDisplayed(); }
        internal void InvokeBannerAdCompleted() { CallBannerAdCompleted(); }
        internal void InvokeBannerAdPaided(AdValue value) { OnBannerAdPaided?.Invoke(value); }

        #endregion


        #region interstitial

        public event Action<AdError> OnInterAdFailedToShow;
        public event Action<LoadAdError> OnInterAdFailedToLoad;
        public event Action OnInterAdLoaded;
        public event Action<AdValue> OnInterAdPaided;
        public event Action OnInterAdImpressionRecorded;


        internal void InvokeInterAdFailedToShow(AdError error) { OnInterAdFailedToShow?.Invoke(error); }
        public void InvokeInterAdFailedToLoad(LoadAdError error) { OnInterAdFailedToLoad?.Invoke(error); }
        internal void InvokeInterAdPaided(AdValue value) { OnInterAdPaided?.Invoke(value); }
        internal void InvokeInterAdImpressionRecorded() { OnInterAdImpressionRecorded?.Invoke(); }
        internal void InvokeInterAdLoaded() { OnInterAdLoaded?.Invoke(); }
        internal void InvokeInterAdDisplayed() { CallInterstitialAdDisplayed(); }
        internal void InvokeInterAdCompleted() { CallInterstitialAdCompleted(); }

        #endregion


        #region rewarded

        public event Action<AdError> OnRewardAdFailedToShow;
        public event Action<AdValue> OnRewardAdPaided;
        public event Action<LoadAdError> OnRewardAdFailedToLoad;
        public event Action OnRewardAdLoaded;
        public event Action OnRewardAdImpressionRecorded;

        internal void InvokeRewardAdPaided(AdValue value) { OnRewardAdPaided?.Invoke(value); }
        internal void InvokeRewardAdFailedToShow(AdError error) { OnRewardAdFailedToShow?.Invoke(error); }
        internal void InvokeRewardAdFailedToLoad(LoadAdError error) { OnRewardAdFailedToLoad?.Invoke(error); }
        internal void InvokeRewardAdLoaded() { OnRewardAdLoaded?.Invoke(); }
        internal void InvokeRewardAdImpressionRecorded() { OnRewardAdImpressionRecorded?.Invoke(); }
        internal void InvokeRewardAdDisplayed() { CallRewardedAdDisplayed(); }
        internal void InvokeRewardAdCompleted() { CallRewardedAdCompleted(); }
        internal void InvokeRewardAdSkipped() { CallRewardedAdSkipped(); }
        internal void InvokeRewardAdClosed() { CallRewardedAdClosed(); }

        #endregion

        #region rewarded interstitial

        public event Action<AdError> OnRewardedInterAdFailedToShow;
        public event Action<AdValue> OnRewardedInterAdPaided;
        public event Action<LoadAdError> OnRewardInterAdFailedToLoad;
        public event Action OnRewardInterAdLoaded;
        public event Action OnRewardedInterAdImpressionRecorded;

        internal void InvokeRewardedInterAdPaided(AdValue value) { OnRewardedInterAdPaided?.Invoke(value); }
        internal void InvokeRewardedInterAdFailedToShow(AdError error) { OnRewardedInterAdFailedToShow?.Invoke(error); }
        internal void InvokeRewardedInterAdFailedToLoad(LoadAdError error) { OnRewardInterAdFailedToLoad?.Invoke(error); }
        internal void InvokeRewardedInterAdLoaded() { OnRewardInterAdLoaded?.Invoke(); }
        internal void InvokeRewardedInterAdImpressionRecorded() { OnRewardedInterAdImpressionRecorded?.Invoke(); }
        internal void InvokeRewardedInterAdDisplayed() { CallRewardedInterstitialAdDisplayed(); }
        internal void InvokeRewardedInterAdCompleted() { CallRewardedInterstitialAdCompleted(); }
        internal void InvokeRewardedInterAdSkipped() { CallRewardedInterstitialAdSkipped(); }
        internal void InvokeRewardedInterAdClosed() { CallRewardedInterstitialAdClosed(); }

        #endregion

        #region open ad

        internal void InvokeAppOpenAdDisplayed() { CallAppOpenAdDisplayed(); }
        internal void InvokeAppOpenAdCompleted() { CallAppOpenAdCompleted(); }

        #endregion


#endif

        protected override void InternalShowBannerAd()
        {
            _banner.Load();
            _banner.Show();
        }

        protected override void InternalHideBannerAd() { _banner.Hide(); }

        protected override void InternalDestroyBannerAd() { _banner.Destroy(); }

        protected override void InternalLoadInterstitialAd() { _interstitial?.Load(); }

        protected override bool InternalIsInterstitialAdReady() { return _interstitial.IsReady(); }

        protected override IInterstitial InternalShowInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.InterstitialAdUnit.Id)) return null;
            _interstitial?.Show();
            return _interstitial;
        }

        protected override void InternalLoadRewardedAd() { _rewarded?.Load(); }

        protected override bool InternalIsRewardedAdReady() { return _rewarded.IsReady(); }

        protected override IRewarded InternalShowRewardedAd()
        {
            _rewarded?.Show();
            return _rewarded;
        }

        protected override void InternalLoadRewardedInterstitialAd() { _rewardedInterstitial.Load(); }

        protected override bool InternalIsRewardedInterstitialAdReady() { return _rewardedInterstitial.IsReady(); }

        protected override void InternalShowRewardedInterstitialAd() { _rewardedInterstitial.Show(); }

        protected override void InternalLoadAppOpenAd() { _appOpen.Load(); }

        protected override void InternalShowAppOpenAd() { _appOpen.Show(); }

        protected override bool InternalIsAppOpenAdReady() { return _appOpen.IsReady(); }

        public override void ShowConsentForm()
        {
#if UNITY_ANDROID
#if PANCAKE_ADMOB
            // if (AdsUtil.IsInEEA())
            // {
            //     var prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("GDPR");
            //     if (prefab != null)
            //     {
            //         UnityEngine.GameObject.Instantiate(prefab);
            //         UnityEngine.Time.timeScale = 0;
            //     }
            // }
#endif
#elif UNITY_IOS
            if (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            }

#endif
        }
    }
}