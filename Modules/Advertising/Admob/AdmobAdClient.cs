#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using UnityEngine;
using System;
using System.Collections;
using GoogleMobileAds.Api;


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
        internal Action rewardedInterstitialCompletedChain;
        internal Action rewardedInterstitialDisplayChain;
        internal Action rewardedInterstitialSkippedChain;
        internal Action rewardedInterstitialClosedChain;
        private readonly WaitForSeconds _waitBannerReload = new WaitForSeconds(5f);
        public static AdmobAdClient Instance => client ??= new AdmobAdClient();

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

        protected override void InternalInit()
        {
#if PANCAKE_ADMOB
            MobileAds.Initialize(_ =>
            {
                Runtime.RunOnMainThread(() =>
                {
                    if (AdSettings.AdmobSettings.EnableTestMode) Admob.SetupDeviceTest();
                    if (AdSettings.AdCommonSettings.EnableGdpr) ShowConsentForm();
                });
            });

            _banner = new AdmobBannerLoader(this);
            _interstitial = new AdmobInterstitialLoader(this);
            _rewarded = new AdmobRewardedLoader(this);
            _rewardedInterstitial = new AdmobRewardedInterstitialLoader(this);
            _appOpen = new AdmobAppOpenLoader(this);
            
            isInitialized = true;
            LoadInterstitialAd();
            LoadRewardedAd();
            LoadRewardedInterstitialAd();
            LoadAppOpenAd();
#endif
        }


        #region banner

        public event Action OnBannerAdLoaded;
        public event Action<LoadAdError> OnBannerAdFaildedToLoad;
        public event Action<AdValue> OnBannerAdPaided;


        internal void InvokeBannerAdLoaded() { OnBannerAdLoaded?.Invoke(); }

        internal void InvokeBannerAdFailedToLoad(LoadAdError error)
        {
            OnBannerAdFaildedToLoad?.Invoke(error);
            Runtime.RunCoroutine(DelayBannerReload());
        }

        internal void InvokeBannerAdDisplayed() { CallBannerAdDisplayed(); }
        internal void InvokeBannerAdCompleted() { CallBannerAdCompleted(); }
        internal void InvokeBannerAdPaided(AdValue value) { OnBannerAdPaided?.Invoke(value); }

        private IEnumerator DelayBannerReload()
        {
            yield return _waitBannerReload;
            _banner?.Load();
        }

        #endregion


        #region interstitial

        public event Action<AdError> OnInterAdFailedToShow;
        public event Action<LoadAdError> OnInterAdFailedToLoad;
        public event Action OnInterAdLoaded;
        public event Action<AdValue> OnInterAdPaided;
        public event Action OnInterAdImpressionRecorded;


        internal void InvokeInterAdFailedToShow(AdError error) { OnInterAdFailedToShow?.Invoke(error); }
        internal void InvokeInterAdFailedToLoad(LoadAdError error) { OnInterAdFailedToLoad?.Invoke(error); }
        internal void InvokeInterAdPaided(AdValue value) { OnInterAdPaided?.Invoke(value); }
        internal void InvokeInterAdImpressionRecorded() { OnInterAdImpressionRecorded?.Invoke(); }
        internal void InvokeInterAdLoaded() { OnInterAdLoaded?.Invoke(); }
        internal void InvokeInterAdDisplayed() { CallInterstitialAdDisplayed(); }
        internal void InvokeInterAdCompleted() { CallInterstitialAdCompleted(); }

        protected override void CallInterstitialAdDisplayed()
        {
            R.isShowingAd = true;
            C.CallActionClean(ref interstitialDisplayChain);
            base.CallInterstitialAdDisplayed();
        }

        protected override void CallInterstitialAdCompleted()
        {
            R.isShowingAd = false;
            C.CallActionClean(ref interstitialCompletedChain);
            base.CallInterstitialAdCompleted();
            _interstitial.Destroy();
        }

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

        internal void InvokeRewardAdCompleted()
        {
            R.isShowingAd = false;
            CallRewardedAdClosed();
            if (_rewarded.IsEarnRewarded)
            {
                CallRewardedAdCompleted();
                return;
            }

            CallRewardedAdSkipped();
        }

        protected override void CallRewardedAdDisplayed()
        {
            R.isShowingAd = true;
            C.CallActionClean(ref rewardedDisplayChain);
            base.CallRewardedAdDisplayed();
        }

        protected override void CallRewardedAdSkipped()
        {
            C.CallActionClean(ref rewardedSkippedChain);
            base.CallRewardedAdSkipped();
            _rewarded.Destroy();
        }

        protected override void CallRewardedAdCompleted()
        {
            C.CallActionClean(ref rewardedCompletedChain);
            base.CallRewardedAdCompleted();
            _rewarded.Destroy();
        }

        protected override void CallRewardedAdClosed()
        {
            C.CallActionClean(ref rewardedClosedChain);
            base.CallRewardedAdClosed();
        }

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

        internal void InvokeRewardedInterAdCompleted()
        {
            R.isShowingAd = false;
            CallRewardedInterstitialAdClosed();
            if (_rewardedInterstitial.IsEarnRewarded)
            {
                CallRewardedInterstitialAdCompleted();
                return;
            }

            CallRewardedInterstitialAdSkipped();
        }

        protected override void CallRewardedInterstitialAdDisplayed()
        {
            R.isShowingAd = true;
            C.CallActionClean(ref rewardedInterstitialDisplayChain);
            base.CallRewardedInterstitialAdDisplayed();
        }

        protected override void CallRewardedInterstitialAdSkipped()
        {
            C.CallActionClean(ref rewardedInterstitialSkippedChain);
            base.CallRewardedInterstitialAdSkipped();
            _rewardedInterstitial.Destroy();
        }

        protected override void CallRewardedInterstitialAdCompleted()
        {
            C.CallActionClean(ref rewardedInterstitialCompletedChain);
            base.CallRewardedInterstitialAdCompleted();
            _rewardedInterstitial.Destroy();
        }

        protected override void CallRewardedInterstitialAdClosed()
        {
            C.CallActionClean(ref rewardedInterstitialClosedChain);
            base.CallRewardedInterstitialAdClosed();
        }

        #endregion


        #region open ad

        public event Action<AdError> OnAppOpenAdFailedToShow;
        public event Action<LoadAdError> OnAppOpenAdFailedToLoad;
        public event Action OnAppOpenAdLoaded;
        public event Action<AdValue> OnAppOpenAdPaided;
        public event Action OnAppOpenAdImpressionRecorded;


        internal void InvokeAppOpenAdFailedToShow(AdError error) { OnAppOpenAdFailedToShow?.Invoke(error); }
        public void InvokeAppOpenAdFailedToLoad(LoadAdError error) { OnAppOpenAdFailedToLoad?.Invoke(error); }
        internal void InvokeAppOpenAdPaided(AdValue value) { OnAppOpenAdPaided?.Invoke(value); }
        internal void InvokeAppOpenAdImpressionRecorded() { OnAppOpenAdImpressionRecorded?.Invoke(); }
        internal void InvokeAppOpenAdLoaded() { OnAppOpenAdLoaded?.Invoke(); }
        internal void InvokeAppOpenAdDisplayed() { CallAppOpenAdDisplayed(); }
        internal void InvokeAppOpenAdCompleted() { CallAppOpenAdCompleted(); }

        protected override void CallAppOpenAdDisplayed()
        {
            R.isShowingAd = true;
            base.CallAppOpenAdDisplayed();
        }

        protected override void CallAppOpenAdCompleted()
        {
            R.isShowingAd = false;
            base.CallAppOpenAdCompleted();
        }

        #endregion


        protected override void InternalShowBannerAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.BannerAdUnit.Id)) return;
            _banner.Load();
            _banner.Show();
        }

        protected override void InternalDestroyBannerAd() { _banner.Destroy(); }

        protected override void InternalLoadInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.InterstitialAdUnit.Id)) return;
            _interstitial.Load();
        }

        protected override bool InternalIsInterstitialAdReady() { return _interstitial.IsReady; }

        protected override IInterstitial InternalShowInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.InterstitialAdUnit.Id)) return null;
            _interstitial.Show();
            return _interstitial;
        }

        protected override void InternalLoadRewardedAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.RewardedAdUnit.Id)) return;
            _rewarded.Load();
        }

        protected override bool InternalIsRewardedAdReady() { return _rewarded.IsReady; }

        protected override IRewarded InternalShowRewardedAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.RewardedAdUnit.Id)) return null;
            _rewarded.Show();
            return _rewarded;
        }

        protected override void InternalLoadRewardedInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.RewardedInterstitialAdUnit.Id)) return;
            _rewardedInterstitial.Load();
        }

        protected override bool InternalIsRewardedInterstitialAdReady() { return _rewardedInterstitial.IsReady; }

        protected override IRewardedInterstitial InternalShowRewardedInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.RewardedInterstitialAdUnit.Id)) return null;
            _rewardedInterstitial.Show();
            return _rewardedInterstitial;
        }

        protected override void InternalLoadAppOpenAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.AppOpenAdUnit.Id)) return;
            _appOpen.Load();
        }

        protected override void InternalShowAppOpenAd()
        {
            if (string.IsNullOrEmpty(AdSettings.AdmobSettings.AppOpenAdUnit.Id)) return;
            _appOpen.Show();
        }

        protected override bool InternalIsAppOpenAdReady() { return _appOpen.IsReady; }

        public override void ShowConsentForm()
        {
#if UNITY_ANDROID
            // if (AdsUtil.IsInEEA())
            // {
            //     var prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("GDPR");
            //     if (prefab != null)
            //     {
            //         UnityEngine.GameObject.Instantiate(prefab);
            //         UnityEngine.Time.timeScale = 0;
            //     }
            // }
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
#endif