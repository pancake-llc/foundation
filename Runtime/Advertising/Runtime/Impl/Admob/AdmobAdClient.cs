#if PANCAKE_ADS
#if PANCAKE_ADMOB_ENABLE
using System;
using GoogleMobileAds.Api;
#endif


namespace Pancake.Monetization
{
    public class AdmobAdClient : AdClient
    {
        private const string NO_SDK_MESSAGE = "SDK missing. Please import the AdMob (Google Mobile Ads) plugin.";
        private AdmobBannerLoader _banner = new AdmobBannerLoader();
        private AdmobInterstitialLoader _interstitial = new AdmobInterstitialLoader();
        private AdmobRewardedLoader _rewarded = new AdmobRewardedLoader();
        private AdmobRewardedInterstitialLoader _rewardedInterstitial = new AdmobRewardedInterstitialLoader();
        private AdmobAppOpenLoader _appOpen = new AdmobAppOpenLoader();
        private static AdmobAdClient client;
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
#if PANCAKE_ADMOB_ENABLE
                return true;
#else
                return false;
#endif
            }
        }

#if PANCAKE_ADMOB_ENABLE
        public override float GetAdaptiveBannerHeight => _banner.GetAdaptiveBannerHeight();
#endif

        protected override string NoSdkMessage => NO_SDK_MESSAGE;

        public AdmobBannerLoader Banner => _banner;

        public AdmobInterstitialLoader Interstitial => _interstitial;

        public AdmobRewardedLoader Rewarded => _rewarded;

        public AdmobRewardedInterstitialLoader RewardedInterstitial => _rewardedInterstitial;
        public AdmobAppOpenLoader AppOpen => _appOpen;

        protected override void InternalInit()
        {
#if PANCAKE_ADMOB_ENABLE
            MobileAds.Initialize(_ =>
            {
                Runtime.RunOnMainThread(() =>
                {
                    if (AdSettings.AdmobSettings.EnableTestMode) Admob.SetupDeviceTest();
                    if (AdSettings.AdCommonSettings.EnableGDPR) ShowConsentForm();
                    isInitialized = true;
                });
            });

            _banner.OnClosedEvent += HandleBannerClosed;
            _banner.OnFailToLoadEvent += InvokeBannerAdFailedToLoad;
            _banner.OnLoadedEvent += InvokeBannerAdLoaded;
            _banner.OnOpeningEvent += HandleBannerOpening;
            _banner.OnPaidEvent += InvokeBannerAdPaid;
            
            _interstitial.OnFailToLoadEvent += InvokeInterstitialAdFailedToLoad;
            _interstitial.OnFailToShowEvent += InvokeInterstitialAdFailedToShow;
            _interstitial.OnLoadedEvent += InvokeInterstitialAdLoaded;
            _interstitial.OnOpeningEvent += HandleInterstitialDisplayed;
            _interstitial.OnPaidEvent += InvokeInterstitialAdPaid;
            _interstitial.OnCompleted += HandleInterstitialCompleted;

            _rewarded.OnClosedEvent += HandleRewardedClosed;
            _rewarded.OnFailToLoadEvent += InvokeRewardedAdFailedToLoad;
            _rewarded.OnFailToShowEvent += InvokeRewardedAdFailedToShow;
            _rewarded.OnLoadedEvent += InvokeRewardedAdLoaded;
            _rewarded.OnOpeningEvent += HandleRewardedDisplayed;
            _rewarded.OnPaidEvent += InvokeRewardedAdPaid;
            _rewarded.OnRewardEvent += InvokeRewardedAdRewared;
            _rewarded.OnCompleted += HandleRewaredCompleted;
            _rewarded.OnSkipped += HandleRewardedSkipped;

            _rewardedInterstitial.OnClosedEvent += HandleRewardedInterstitialClosed;
            _rewardedInterstitial.OnFailToLoadEvent += InvokeRewardedInterstitialAdFailedToLoad;
            _rewardedInterstitial.OnFailToShowEvent += InvokeRewardedInterstitialAdFailedToShow;
            _rewardedInterstitial.OnLoadedEvent += InvokeRewardedInterstitialAdLoaded;
            _rewardedInterstitial.OnOpeningEvent += HandleRewardedInterstitialDisplayed;
            _rewardedInterstitial.OnPaidEvent += InvokeRewardedInterstitialAdPaid;
            _rewardedInterstitial.OnRewardEvent += InvokeRewardedInterstitialAdRewared;
            _rewardedInterstitial.OnCompleted += HandleRewardedInterstitialCompleted;
            _rewardedInterstitial.OnSkipped += HandleRewardedInterstitialSkipped;
            
            _appOpen.OnFailToLoadEvent += InvokeAppOpenAdFailedToLoad;
            _appOpen.OnFailToShowEvent += InvokeAppOpenAdFailedToShow;
            _appOpen.OnLoadedEvent += InvokeAppOpenAdLoaded;
            _appOpen.OnOpeningEvent += HandleAppOpenDisplayed;
            _appOpen.OnPaidEvent += InvokeAppOpenAdPaid;
            _appOpen.OnCompleted += HandleAppOpenCompleted;
#endif
        }

#if PANCAKE_ADMOB_ENABLE
        public event EventHandler<AdFailedToLoadEventArgs> OnBannerAdFailedToLoad;
        public event EventHandler<EventArgs> OnBannerAdLoaded;
        public event EventHandler<AdValueEventArgs> OnBannerAdPaid;

        public event EventHandler<AdFailedToLoadEventArgs> OnInterstitialAdFailedToLoad;
        public event EventHandler<AdErrorEventArgs> OnInterstitialAdFailedToShow;
        public event EventHandler<EventArgs> OnInterstitialAdLoaded;
        public event EventHandler<AdValueEventArgs> OnInterstititalAdPaid;

        public event EventHandler<AdValueEventArgs> OnRewardedAdPaid;
        public event EventHandler<EventArgs> OnRewardedAdLoaded;
        public event EventHandler<AdErrorEventArgs> OnRewardedAdFailedToShow;
        public event EventHandler<AdFailedToLoadEventArgs> OnRewardedAdFailedToLoad;
        public event EventHandler<Reward> OnRewardedAdRewarded;

        public event EventHandler<AdValueEventArgs> OnRewardedInterstitialAdPaid;
        public event EventHandler<EventArgs> OnRewardedInterstitialAdLoaded;
        public event EventHandler<AdErrorEventArgs> OnRewardedInterstitialAdFailedToShow;
        public event EventHandler<AdFailedToLoadEventArgs> OnRewardedInterstitialAdFailedToLoad;
        public event EventHandler<Reward> OnRewardedInterstitialAdRewarded;

        public event EventHandler<AdValueEventArgs> OnAppOpenAdPaid;
        public event EventHandler<EventArgs> OnAppOpenAdLoaded;
        public event EventHandler<AdErrorEventArgs> OnRAppOpenAdFailedToShow;
        public event EventHandler<AdFailedToLoadEventArgs> OnAppOpenAdFailedToLoad;


        private void InvokeBannerAdLoaded(AdLoader<AdUnit> instance, object sender, EventArgs args) { OnBannerAdLoaded?.Invoke(sender, args); }
        private void HandleBannerClosed(AdLoader<AdUnit> instance, object sender, EventArgs args) { InternalBannerCompleted(instance); }
        private void InvokeBannerAdFailedToLoad(AdLoader<AdUnit> instance, object sender, AdFailedToLoadEventArgs args) { OnBannerAdFailedToLoad?.Invoke(sender, args); }
        private void HandleBannerOpening(AdLoader<AdUnit> instance, object sender, EventArgs args) { InternalBannerDisplayed(instance); }
        private void InvokeBannerAdPaid(AdLoader<AdUnit> instance, object sender, AdValueEventArgs args) { OnBannerAdPaid?.Invoke(sender, args); }

        private void InvokeInterstitialAdFailedToLoad(AdLoader<AdUnit> instance, object sender, AdFailedToLoadEventArgs args)
        {
            OnInterstitialAdFailedToLoad?.Invoke(sender, args);
        }

        private void InvokeInterstitialAdFailedToShow(AdLoader<AdUnit> instance, object sender, AdErrorEventArgs args)
        {
            OnInterstitialAdFailedToShow?.Invoke(sender, args);
        }

        private void InvokeInterstitialAdLoaded(AdLoader<AdUnit> instance, object sender, EventArgs args) { OnInterstitialAdLoaded?.Invoke(sender, args); }
        private void InvokeInterstitialAdPaid(AdLoader<AdUnit> instance, object sender, AdValueEventArgs args) { OnInterstititalAdPaid?.Invoke(sender, args); }
        private void InvokeRewardedAdPaid(AdLoader<AdUnit> instance, object sender, AdValueEventArgs args) { OnRewardedAdPaid?.Invoke(sender, args); }
        private void InvokeRewardedAdLoaded(AdLoader<AdUnit> instance, object sender, EventArgs args) { OnRewardedAdLoaded?.Invoke(sender, args); }
        private void InvokeRewardedAdFailedToShow(AdLoader<AdUnit> instance, object sender, AdErrorEventArgs args) { OnRewardedAdFailedToShow?.Invoke(sender, args); }

        private void InvokeRewardedAdFailedToLoad(AdLoader<AdUnit> instance, object sender, AdFailedToLoadEventArgs args)
        {
            OnRewardedAdFailedToLoad?.Invoke(sender, args);
        }

        private void InvokeRewardedAdRewared(AdLoader<AdUnit> instance, object sender, Reward args) { OnRewardedAdRewarded?.Invoke(sender, args); }

        private void HandleInterstitialCompleted(AdmobInterstitialLoader instance) { InternalInterstitialCompleted(instance); }
        private void HandleInterstitialDisplayed(AdmobInterstitialLoader instance, object sender, EventArgs args) { InternalInterstitialDispalyed(instance); }

        private void HandleInterstitialAdOpening(AdmobInterstitialLoader instance, object sender, EventArgs arg3) { InternalInterstitialDisplayed(instance); }

        private void HandleRewaredCompleted(AdmobRewardedLoader instance) { InternalRewaredCompleted(instance); }

        private void HandleRewardedSkipped(AdmobRewardedLoader instance) { InternalRewardSkipped(instance); }
        private void HandleRewardedClosed(AdmobRewardedLoader instance, object sender, EventArgs args) { InternalRewaredClosed(instance); }
        private void HandleRewardedDisplayed(AdmobRewardedLoader instance, object sender, EventArgs args) { InternalRewaredDisplayed(instance); }

        private void HandleRewardedAdOpening(AdmobRewardedLoader instance, object sender, EventArgs arg3) { InternalRewardedDisplayed(instance); }

        private void HandleRewardedInterstitialSkipped(AdmobRewardedInterstitialLoader instance) { InternalRewardedInterstitialSkipped(instance); }

        private void HandleRewardedInterstitialCompleted(AdmobRewardedInterstitialLoader instance) { InternalRewaredInterstitialCompleted(instance); }

        private void HandleRewardedInterstitialClosed(AdmobRewardedInterstitialLoader instance, object sender, EventArgs args)
        {
            InternalRewaredInterstitialClosed(instance);
        }

        private void HandleRewardedInterstitialDisplayed(AdmobRewardedInterstitialLoader instance, object sender, EventArgs args)
        {
            InternalRewaredInterstitialDisplayed(instance);
        }

        private void InvokeRewardedInterstitialAdRewared(AdmobRewardedInterstitialLoader instance, Reward args) { OnRewardedInterstitialAdRewarded?.Invoke(null, args); }

        private void InvokeRewardedInterstitialAdPaid(AdmobRewardedInterstitialLoader instance, object sender, AdValueEventArgs args)
        {
            OnRewardedInterstitialAdPaid?.Invoke(sender, args);
        }

        private void InvokeRewardedInterstitialAdLoaded(AdmobRewardedInterstitialLoader instance) { OnRewardedInterstitialAdLoaded?.Invoke(null, null); }

        private void InvokeRewardedInterstitialAdFailedToShow(AdmobRewardedInterstitialLoader instance, object sender, AdErrorEventArgs args)
        {
            OnRewardedInterstitialAdFailedToShow?.Invoke(sender, args);
        }

        private void InvokeRewardedInterstitialAdFailedToLoad(AdmobRewardedInterstitialLoader instance, AdFailedToLoadEventArgs args)
        {
            OnRewardedInterstitialAdFailedToLoad?.Invoke(null, args);
        }
        
        private void HandleAppOpenCompleted(AdmobAppOpenLoader instance) { InternalAppOpenCompleted(instance); }
        private void HandleAppOpenDisplayed(AdmobAppOpenLoader instance, object sender, EventArgs args) { InternalAppOpenDisplayed(instance); }

        private void InvokeAppOpenAdPaid(AdmobAppOpenLoader instance, object sender, AdValueEventArgs args) { OnAppOpenAdPaid?.Invoke(sender, args); }


        private void InvokeAppOpenAdLoaded(AdmobAppOpenLoader instance) { OnAppOpenAdLoaded?.Invoke(null, null); }

        private void InvokeAppOpenAdFailedToShow(AdmobAppOpenLoader instance, object sender, AdErrorEventArgs args) { OnRAppOpenAdFailedToShow?.Invoke(sender, args); }

        private void InvokeAppOpenAdFailedToLoad(AdmobAppOpenLoader instance, AdFailedToLoadEventArgs args) { OnAppOpenAdFailedToLoad?.Invoke(null, args); }

#endif

        protected virtual void InternalBannerDisplayed(AdLoader<AdUnit> _) { CallBannerAdDisplayed(); }
        protected virtual void InternalBannerCompleted(AdLoader<AdUnit> _) { CallBannerAdCompleted(); }
        protected virtual void InternalInterstitialCompleted(AdLoader<AdUnit> _) { CallInterstitialAdCompleted(); }
        protected virtual void InternalInterstitialDispalyed(AdLoader<AdUnit> _) { CallInterstitialAdDisplayed(); }

        protected virtual void InternalRewardSkipped(AdLoader<AdUnit> _) { CallRewardedAdSkipped(); }

        protected virtual void InternalRewaredCompleted(AdLoader<AdUnit> _) { CallRewardedAdCompleted(); }
        protected virtual void InternalRewaredClosed(AdLoader<AdUnit> _) { CallRewardedAdClosed(); }
        protected virtual void InternalRewaredDisplayed(AdLoader<AdUnit> _) { CallRewardedAdDisplayed(); }

        protected virtual void InternalRewardedInterstitialSkipped(AdLoader<AdUnit> _) { CallRewardedInterstitialAdSkipped(); }
        protected virtual void InternalRewaredInterstitialCompleted(AdLoader<AdUnit> _) { CallRewardedInterstitialAdCompleted(); }
        protected virtual void InternalRewaredInterstitialClosed(AdLoader<AdUnit> _) { CallRewardedInterstitialAdClosed(); }
        protected virtual void InternalRewaredInterstitialDisplayed(AdLoader<AdUnit> _) { CallRewardedInterstitialAdDisplayed(); }

        protected virtual void InternalAppOpenCompleted(AdLoader<AdUnit> _) { CallAppOpenAdCompleted(); }
        protected virtual void InternalAppOpenDisplayed(AdLoader<AdUnit> _) { CallAppOpenAdDisplayed(); }

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
#if PANCAKE_ADMOB_ENABLE
            if (AdsUtil.IsInEEA())
            {
                var prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("GDPR");
                if (prefab != null)
                {
                    UnityEngine.GameObject.Instantiate(prefab);
                    UnityEngine.Time.timeScale = 0;
                }
            }
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
#endif