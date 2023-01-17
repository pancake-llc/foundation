#if PANCAKE_ADS
using System;
using JetBrains.Annotations;

#pragma warning disable CS0414
// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable NotAccessedField.Local
namespace Pancake.Monetization
{
    public class ApplovinAdClient : AdClient
    {
        private const string NO_SDK_MESSAGE = "SDK missing. Please import applovin max plugin.";
        private ApplovinBannerLoader _banner;
        private ApplovinInterstitialLoader _interstitial;
        private ApplovinRewardedLoader _rewarded;
        private ApplovinRewardedInterstitialLoader _rewardedInterstitial;
        private ApplovinAppOpenLoader _appOpen;
        private static ApplovinAdClient client;
        private bool _isBannerDestroyed;
        private bool _isRewardedCompleted;
        private bool _isRewardedInterstitialCompleted;
        internal Action rewardedCompletedChain;
        internal Action rewardedDisplayChain;
        internal Action rewardedSkippedChain;
        internal Action rewardedClosedChain;
        public static ApplovinAdClient Instance => client ?? (client = new ApplovinAdClient());

#if PANCAKE_MAX_ENABLE
        public event Action OnBannerAdLoaded;
        public event Action OnBannerAdFaildToLoad;
        public event Action OnBannerAdClicked;
        public event Action<MaxSdkBase.AdInfo> OnBannerAdRevenuePaid;
        
        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdLoaded;
        public event Action OnInterstitialAdFaildToLoad;
        public event Action OnInterstitialAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnInterstitialAdRevenuePaid;
        
        public event Action OnRewardedAdClicked;
        public event Action OnRewardedAdLoaded;
        public event Action OnRewardedAdFaildToLoad;
        public event Action OnRewardedAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnRewardedAdRevenuePaid;
        public event Action<MaxSdkBase.Reward> OnRewardedAdReceivedReward;
        
        public event Action OnRewardedInterstitialAdClicked;
        public event Action OnRewardedInterstitialAdLoaded;
        public event Action OnRewardedInterstitialAdFaildToLoad;
        public event Action OnRewardedInterstitialAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnRewardedInterstitialAdRevenuePaid;
        public event Action<MaxSdkBase.Reward> OnRewardedInterstitialAdReceivedReward;

        public event Action OnAppOpenAdClicked;
        public event Action OnAppOpenAdLoaded;
        public event Action OnAppOpenAdFaildToLoaded;
        public event Action OnAppOpenAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnAppOpenAdPaid;
#endif

        public override EAdNetwork Network => EAdNetwork.Applovin;
        public override bool IsBannerAdSupported => true;
        public override bool IsInsterstitialAdSupport => true;
        public override bool IsRewardedAdSupport => true;
        public override bool IsRewardedInterstitialAdSupport => true;
        public override bool IsAppOpenAdSupport => true;

        public override bool IsSdkAvaiable
        {
            get
            {
#if PANCAKE_MAX_ENABLE
                return true;
#else
                return false;
#endif
            }
        }

#if PANCAKE_MAX_ENABLE
        public override float GetAdaptiveBannerHeight => _banner.GetAdaptiveBannerHeight();
#endif

        protected override string NoSdkMessage => NO_SDK_MESSAGE;

        #region internal

#if PANCAKE_MAX_ENABLE
        internal void InvokeBannerAdLoaded() { OnBannerAdLoaded?.Invoke(); }
        internal void InvokeBannerAdFaildToLoad() { OnBannerAdFaildToLoad?.Invoke(); }
        internal void InvokeBannerAdClicked() { OnBannerAdClicked?.Invoke(); }
        internal void InvokeBannerAdExpanded() { CallBannerAdDisplayed(); }
        internal void InvokeBannerAdCollapsed() { CallBannerAdCompleted(); }
        internal void InvokeBannerAdRevenuePaid(MaxSdkBase.AdInfo info) { OnBannerAdRevenuePaid?.Invoke(info); }
        internal void InvokeInterstitialAdLoaded() { OnInterstitialAdLoaded?.Invoke(); }
        internal void InvokeInterstitialAdFaildToLoad() { OnInterstitialAdFaildToLoad?.Invoke(); }
        internal void InvokeInterstitialAdFaildToDisplay() { OnInterstitialAdFaildToDisplay?.Invoke(); }
        internal void InvokeInterstitialAdClicked() { OnInterstitialAdClicked?.Invoke(); }
        internal void InvokeInterstitialAdDisplay() { CallInterstitialAdDisplayed(); }
        internal void InvokeInterstitialAdHidden() { CallInterstitialAdCompleted(); }
        internal void InvokeInterstitialAdRevenuePaid(MaxSdkBase.AdInfo info) { OnInterstitialAdRevenuePaid?.Invoke(info); }
        internal void InvokeRewardedAdLoaded() { OnRewardedAdLoaded?.Invoke(); }
        internal void InvokeRewardedAdFaildToLoad() { OnRewardedAdFaildToLoad?.Invoke(); }
        internal void InvokeRewardedAdFaildToDisplay() { OnRewardedAdFaildToDisplay?.Invoke(); }
        internal void InvokeRewardedAdClicked() { OnRewardedAdClicked?.Invoke(); }

        internal void InvokeRewardedAdDisplay()
        {
            CallRewardedAdDisplayed();
            C.CallCacheCleanAction(ref rewardedDisplayChain);
        }

        internal void InvokeRewardedAdHidden()
        {
            CallRewardedAdClosed();
            C.CallCacheCleanAction(ref rewardedClosedChain);
            if (_isRewardedCompleted)
            {
                CallRewardedAdCompleted();
                C.CallCacheCleanAction(ref rewardedCompletedChain);
                return;
            }

            CallRewardedAdSkipped();
            C.CallCacheCleanAction(ref rewardedSkippedChain);
        }
        
        internal void InvokeRewardedAdRevenuePaid(MaxSdkBase.AdInfo info) { OnRewardedAdRevenuePaid?.Invoke(info); }

        internal void InvokeRewardedAdReceivedReward(MaxSdkBase.Reward reward)
        {
            OnRewardedAdReceivedReward?.Invoke(reward);
            _isRewardedCompleted = true;
        }

        internal void InvokeRewardedInterstitialAdLoaded() { OnRewardedInterstitialAdLoaded?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToLoad() { OnRewardedInterstitialAdFaildToLoad?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToDisplay() { OnRewardedInterstitialAdFaildToDisplay?.Invoke(); }
        internal void InvokeRewardedInterstitialAdClicked() { OnRewardedInterstitialAdClicked?.Invoke(); }
        internal void InvokeRewardedInterstitialAdDisplay() { CallRewardedInterstitialAdDisplayed(); }

        internal void InvokeRewardedInterstitialAdHidden()
        {
            CallRewardedInterstitialAdClosed();
            if (_isRewardedInterstitialCompleted)
            {
                CallRewardedInterstitialAdCompleted();
                return;
            }

            CallRewardedInterstitialAdSkipped();
        }
        
        internal void InvokeRewardedInterstitialAdRevenuePaid(MaxSdkBase.AdInfo info) { OnRewardedInterstitialAdRevenuePaid?.Invoke(info); }

        internal void InvokeRewardedInterstitialAdReceivedReward(MaxSdkBase.Reward reward)
        {
            OnRewardedInterstitialAdReceivedReward?.Invoke(reward);
            _isRewardedInterstitialCompleted = true;
        }

        internal void InvokeAppOpenAdRevenuePaid(MaxSdkBase.AdInfo info) { OnAppOpenAdPaid?.Invoke(info); }
        internal void InvokeAppOpenAdClicked() { OnAppOpenAdClicked?.Invoke(); }
        internal void InvokeAppOpenAdLoaded() { OnAppOpenAdLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToLoaded() { OnAppOpenAdFaildToLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToDisplay() { OnAppOpenAdFaildToDisplay?.Invoke(); }
        internal void InvokeAppOpenAdDisplay() { CallAppOpenAdDisplayed(); }
        internal void InternalAppOpenAdCompleted() { CallAppOpenAdCompleted(); }
#endif

        #endregion

        protected override void InternalInit()
        {
#if PANCAKE_MAX_ENABLE
            MaxSdk.SetSdkKey(AdSettings.MaxSettings.SdkKey);
            if (AdSettings.AdCommonSettings.EnableGDPR) MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;

            MaxSdk.InitializeSdk();
            MaxSdk.SetIsAgeRestrictedUser(AdSettings.MaxSettings.EnableAgeRestrictedUser);
#endif

            _banner = new ApplovinBannerLoader(this);
            _interstitial = new ApplovinInterstitialLoader(this);
            _rewarded = new ApplovinRewardedLoader(this);
            _rewardedInterstitial = new ApplovinRewardedInterstitialLoader(this);
            _appOpen = new ApplovinAppOpenLoader(this);

            LoadInterstitialAd();
            LoadRewardedAd();
            LoadRewardedInterstitialAd();
            isInitialized = true;
            _isBannerDestroyed = false;
        }

#if PANCAKE_MAX_ENABLE
        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration configuration)
        {
            if (configuration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
            {
                ShowConsentForm();
            }
            else if (configuration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
            {
                // No need to show consent dialog, proceed with initialization
            }
            else
            {
                // Consent dialog state is unknown. Proceed with initialization, but check if the consent
                // dialog should be shown on the next application initialization
            }
        }
#endif

        protected override void InternalShowBannerAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.BannerAdUnit.Id)) return;
            if (_isBannerDestroyed)
            {
                MaxSdk.CreateBanner(AdSettings.MaxSettings.BannerAdUnit.Id, AdSettings.MaxSettings.BannerAdUnit.ConvertPosition());
                _isBannerDestroyed = false;
            }

            MaxSdk.ShowBanner(AdSettings.MaxSettings.BannerAdUnit.Id);
#endif
        }

        protected override void InternalHideBannerAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.BannerAdUnit.Id)) return;
            MaxSdk.HideBanner(AdSettings.MaxSettings.BannerAdUnit.Id);
#endif
        }

        protected override void InternalDestroyBannerAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.BannerAdUnit.Id)) return;
            _isBannerDestroyed = true;
            MaxSdk.DestroyBanner(AdSettings.MaxSettings.BannerAdUnit.Id);
#endif
        }

        protected override void InternalLoadInterstitialAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return;
            MaxSdk.LoadInterstitial(AdSettings.MaxSettings.InterstitialAdUnit.Id);
#endif
        }

        protected override void InternalShowInterstitialAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return;
            R.isShowingAd = true;
            MaxSdk.ShowInterstitial(AdSettings.MaxSettings.InterstitialAdUnit.Id);
#endif
        }

        protected override bool InternalIsInterstitialAdReady()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return false;
            return MaxSdk.IsInterstitialReady(AdSettings.MaxSettings.InterstitialAdUnit.Id);
#else
            return false;
#endif
        }

        protected override void InternalLoadRewardedAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return;
            MaxSdk.LoadRewardedAd(AdSettings.MaxSettings.RewardedAdUnit.Id);
#endif
        }

        [CanBeNull]
        protected override IRewarded InternalShowRewardedAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return null;
            _isRewardedCompleted = false;
            R.isShowingAd = true;
            MaxSdk.ShowRewardedAd(AdSettings.MaxSettings.RewardedAdUnit.Id);
#endif
            return _rewarded;
        }

        protected override bool InternalIsRewardedAdReady()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return false;
            return MaxSdk.IsRewardedAdReady(AdSettings.MaxSettings.RewardedAdUnit.Id);
#else
            return false;
#endif
        }

        protected override void InternalLoadRewardedInterstitialAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return;
            MaxSdk.LoadRewardedInterstitialAd(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);
#endif
        }

        protected override void InternalShowRewardedInterstitialAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return;
            _isRewardedInterstitialCompleted = false;
            R.isShowingAd = true;
            MaxSdk.ShowRewardedInterstitialAd(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);
#endif
        }

        protected override bool InternalIsRewardedInterstitialAdReady()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return false;
            return MaxSdk.IsRewardedInterstitialAdReady(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);
#else
            return false;
#endif
        }

        protected override void InternalLoadAppOpenAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return;
            MaxSdk.LoadAppOpenAd(AdSettings.MaxSettings.AppOpenAdUnit.Id);
#endif
        }

        protected override void InternalShowAppOpenAd()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return;
            R.isShowingAd = true;
            MaxSdk.ShowAppOpenAd(AdSettings.MaxSettings.AppOpenAdUnit.Id);
#endif
        }

        protected override bool InternalIsAppOpenAdReady()
        {
#if PANCAKE_MAX_ENABLE
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return false;
            return MaxSdk.IsAppOpenAdReady(AdSettings.MaxSettings.AppOpenAdUnit.Id);
#else
            return false;
#endif
        }

        public override void ShowConsentForm()
        {
#if UNITY_ANDROID
#if PANCAKE_MAX_ENABLE
            if (AdsUtil.IsInEEA())
            {
                MaxSdk.UserService.ShowConsentDialog();
            }
#endif
#elif UNITY_IOS
            if (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        }
    }
}
#endif