using System;
using JetBrains.Annotations;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    public class ApplovinAdClient : AdClient
    {
        private const string NO_SDK_MESSAGE = "SDK missing. Please import AppLovin plugin.";
        private ApplovinBannerLoader _banner;
        private ApplovinInterstitialLoader _interstitial;
        private ApplovinRewardedLoader _rewarded;
        private ApplovinRewardedInterstitialLoader _rewardedInterstitial;
        private ApplovinAppOpenLoader _appOpen;
        private static ApplovinAdClient client;
        private bool _isBannerDestroyed;
        private bool _isEarnRewarded;
        private bool _isEarnRewardedInterstitial;
        internal Action rewardedCompletedChain;
        internal Action rewardedDisplayChain;
        internal Action rewardedSkippedChain;
        internal Action rewardedClosedChain;
        internal Action interstitialDisplayChain;
        internal Action interstitialCompletedChain;
        internal Action rewardedInterstitialCompletedChain;
        internal Action rewardedInterstitialDisplayChain;
        internal Action rewardedInterstitialSkippedChain;
        internal Action rewardedInterstitialClosedChain;
        public static ApplovinAdClient Instance => client ??= new ApplovinAdClient();


        #region banner

        public event Action OnBannerAdLoaded;
        public event Action OnBannerAdFaildToLoad;
        public event Action OnBannerAdClicked;
        public event Action<MaxSdkBase.AdInfo> OnBannerAdRevenuePaid;


        internal void InvokeBannerAdLoaded() { OnBannerAdLoaded?.Invoke(); }
        internal void InvokeBannerAdFaildToLoad() { OnBannerAdFaildToLoad?.Invoke(); }
        internal void InvokeBannerAdClicked() { OnBannerAdClicked?.Invoke(); }
        internal void InvokeBannerAdExpanded() { CallBannerAdDisplayed(); }
        internal void InvokeBannerAdCollapsed() { CallBannerAdCompleted(); }
        internal void InvokeBannerAdRevenuePaid(MaxSdkBase.AdInfo info) { OnBannerAdRevenuePaid?.Invoke(info); }

        #endregion


        #region interstitial

        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdLoaded;
        public event Action OnInterstitialAdFaildToLoad;
        public event Action OnInterstitialAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnInterstitialAdRevenuePaid;

        internal void InvokeInterstitialAdLoaded() { OnInterstitialAdLoaded?.Invoke(); }
        internal void InvokeInterstitialAdFaildToLoad() { OnInterstitialAdFaildToLoad?.Invoke(); }
        internal void InvokeInterstitialAdFaildToDisplay() { OnInterstitialAdFaildToDisplay?.Invoke(); }
        internal void InvokeInterstitialAdClicked() { OnInterstitialAdClicked?.Invoke(); }
        internal void InvokeInterstitialAdDisplay() { CallInterstitialAdDisplayed(); }

        internal void InvokeInterstitialAdHidden()
        {
            R.isShowingAd = false;
            CallInterstitialAdCompleted();
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) LoadInterstitialAd();
        }

        internal void InvokeInterstitialAdRevenuePaid(MaxSdkBase.AdInfo info) { OnInterstitialAdRevenuePaid?.Invoke(info); }

        protected override void CallInterstitialAdDisplayed()
        {
            C.CallActionClean(ref interstitialDisplayChain);
            base.CallInterstitialAdDisplayed();
        }

        protected override void CallInterstitialAdCompleted()
        {
            C.CallActionClean(ref interstitialCompletedChain);
            base.CallInterstitialAdCompleted();
        }

        #endregion


        #region rewarded

        public event Action OnRewardedAdClicked;
        public event Action OnRewardedAdLoaded;
        public event Action OnRewardedAdFaildToLoad;
        public event Action OnRewardedAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnRewardedAdRevenuePaid;
        public event Action<MaxSdkBase.Reward> OnRewardedAdReceivedReward;


        internal void InvokeRewardedAdLoaded() { OnRewardedAdLoaded?.Invoke(); }
        internal void InvokeRewardedAdFaildToLoad() { OnRewardedAdFaildToLoad?.Invoke(); }
        internal void InvokeRewardedAdFaildToDisplay() { OnRewardedAdFaildToDisplay?.Invoke(); }
        internal void InvokeRewardedAdClicked() { OnRewardedAdClicked?.Invoke(); }

        internal void InvokeRewardedAdDisplay()
        {
            CallRewardedAdDisplayed();
            C.CallActionClean(ref rewardedDisplayChain);
        }

        internal void InvokeRewardedAdHidden()
        {
            R.isShowingAd = false;
            CallRewardedAdClosed();
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) LoadRewardedAd();
            if (_isEarnRewarded)
            {
                CallRewardedAdCompleted();
                return;
            }

            CallRewardedAdSkipped();
        }

        internal void InvokeRewardedAdRevenuePaid(MaxSdkBase.AdInfo info) { OnRewardedAdRevenuePaid?.Invoke(info); }

        internal void InvokeRewardedAdReceivedReward(MaxSdkBase.Reward reward)
        {
            OnRewardedAdReceivedReward?.Invoke(reward);
            _isEarnRewarded = true;
        }

        protected override void CallRewardedAdClosed()
        {
            C.CallActionClean(ref rewardedClosedChain);
            base.CallRewardedAdClosed();
        }

        protected override void CallRewardedAdCompleted()
        {
            C.CallActionClean(ref rewardedCompletedChain);
            base.CallRewardedAdCompleted();
        }

        protected override void CallRewardedAdSkipped()
        {
            C.CallActionClean(ref rewardedSkippedChain);
            base.CallRewardedAdSkipped();
        }

        #endregion


        #region rewarded interstitial

        public event Action OnRewardedInterstitialAdClicked;
        public event Action OnRewardedInterstitialAdLoaded;
        public event Action OnRewardedInterstitialAdFaildToLoad;
        public event Action OnRewardedInterstitialAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnRewardedInterstitialAdRevenuePaid;
        public event Action<MaxSdkBase.Reward> OnRewardedInterstitialAdReceivedReward;


        internal void InvokeRewardedInterstitialAdLoaded() { OnRewardedInterstitialAdLoaded?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToLoad() { OnRewardedInterstitialAdFaildToLoad?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToDisplay() { OnRewardedInterstitialAdFaildToDisplay?.Invoke(); }
        internal void InvokeRewardedInterstitialAdClicked() { OnRewardedInterstitialAdClicked?.Invoke(); }
        internal void InvokeRewardedInterstitialAdDisplay() { CallRewardedInterstitialAdDisplayed(); }

        internal void InvokeRewardedInterstitialAdHidden()
        {
            R.isShowingAd = false;
            CallRewardedInterstitialAdClosed();
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) LoadRewardedInterstitialAd();
            if (_isEarnRewardedInterstitial)
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
            _isEarnRewardedInterstitial = true;
        }

        protected override void CallRewardedInterstitialAdClosed()
        {
            C.CallActionClean(ref rewardedInterstitialClosedChain);
            base.CallRewardedInterstitialAdClosed();
        }

        protected override void CallRewardedInterstitialAdCompleted()
        {
            C.CallActionClean(ref rewardedInterstitialCompletedChain);
            base.CallRewardedInterstitialAdCompleted();
        }

        protected override void CallRewardedInterstitialAdSkipped()
        {
            C.CallActionClean(ref rewardedInterstitialSkippedChain);
            base.CallRewardedInterstitialAdSkipped();
        }

        protected override void CallRewardedInterstitialAdDisplayed()
        {
            C.CallActionClean(ref rewardedInterstitialDisplayChain);
            base.CallRewardedInterstitialAdDisplayed();
        }

        #endregion


        #region app open

        public event Action OnAppOpenAdClicked;
        public event Action OnAppOpenAdLoaded;
        public event Action OnAppOpenAdFaildToLoaded;
        public event Action OnAppOpenAdFaildToDisplay;
        public event Action<MaxSdkBase.AdInfo> OnAppOpenAdPaid;

        internal void InvokeAppOpenAdRevenuePaid(MaxSdkBase.AdInfo info) { OnAppOpenAdPaid?.Invoke(info); }
        internal void InvokeAppOpenAdClicked() { OnAppOpenAdClicked?.Invoke(); }
        internal void InvokeAppOpenAdLoaded() { OnAppOpenAdLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToLoaded() { OnAppOpenAdFaildToLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToDisplay() { OnAppOpenAdFaildToDisplay?.Invoke(); }
        internal void InvokeAppOpenAdDisplay() { CallAppOpenAdDisplayed(); }
        internal void InvokeAppOpenAdCompleted() { CallAppOpenAdCompleted(); }

        #endregion


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
#if PANCAKE_APPLOVIN
                return true;
#else
                return false;
#endif
            }
        }

        protected override string NoSdkMessage => NO_SDK_MESSAGE;

        protected override void InternalInit()
        {
#if PANCAKE_APPLOVIN
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

        protected override void InternalShowBannerAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.BannerAdUnit.Id)) return;
            if (_isBannerDestroyed)
            {
                MaxSdk.CreateBanner(AdSettings.MaxSettings.BannerAdUnit.Id, AdSettings.MaxSettings.BannerAdUnit.ConvertPosition());
                _isBannerDestroyed = false;
            }

            MaxSdk.ShowBanner(AdSettings.MaxSettings.BannerAdUnit.Id);
        }

        protected override void InternalDestroyBannerAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.BannerAdUnit.Id)) return;
            _isBannerDestroyed = true;
            MaxSdk.DestroyBanner(AdSettings.MaxSettings.BannerAdUnit.Id);
        }

        protected override void InternalLoadInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return;
            MaxSdk.LoadInterstitial(AdSettings.MaxSettings.InterstitialAdUnit.Id);
        }

        protected override IInterstitial InternalShowInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return null;
            R.isShowingAd = true;
            MaxSdk.ShowInterstitial(AdSettings.MaxSettings.InterstitialAdUnit.Id);

            return _interstitial;
        }

        protected override bool InternalIsInterstitialAdReady()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.InterstitialAdUnit.Id)) return false;
            return MaxSdk.IsInterstitialReady(AdSettings.MaxSettings.InterstitialAdUnit.Id);
        }

        protected override void InternalLoadRewardedAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return;
            MaxSdk.LoadRewardedAd(AdSettings.MaxSettings.RewardedAdUnit.Id);
        }

        [CanBeNull]
        protected override IRewarded InternalShowRewardedAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return null;
            _isEarnRewarded = false;
            R.isShowingAd = true;
            MaxSdk.ShowRewardedAd(AdSettings.MaxSettings.RewardedAdUnit.Id);
            return _rewarded;
        }

        protected override bool InternalIsRewardedAdReady()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedAdUnit.Id)) return false;
            return MaxSdk.IsRewardedAdReady(AdSettings.MaxSettings.RewardedAdUnit.Id);
        }

        protected override void InternalLoadRewardedInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return;
            MaxSdk.LoadRewardedInterstitialAd(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);
        }

        protected override IRewardedInterstitial InternalShowRewardedInterstitialAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return null;
            _isEarnRewardedInterstitial = false;
            R.isShowingAd = true;
            MaxSdk.ShowRewardedInterstitialAd(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);

            return _rewardedInterstitial;
        }

        protected override bool InternalIsRewardedInterstitialAdReady()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id)) return false;
            return MaxSdk.IsRewardedInterstitialAdReady(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);
        }

        protected override void InternalLoadAppOpenAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return;
            MaxSdk.LoadAppOpenAd(AdSettings.MaxSettings.AppOpenAdUnit.Id);
        }

        protected override void InternalShowAppOpenAd()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return;
            R.isShowingAd = true;
            MaxSdk.ShowAppOpenAd(AdSettings.MaxSettings.AppOpenAdUnit.Id);
        }

        protected override bool InternalIsAppOpenAdReady()
        {
            if (string.IsNullOrEmpty(AdSettings.MaxSettings.AppOpenAdUnit.Id)) return false;
            return MaxSdk.IsAppOpenAdReady(AdSettings.MaxSettings.AppOpenAdUnit.Id);
        }

        public override void ShowConsentForm()
        {
#if UNITY_ANDROID
#if PANCAKE_APPLOVIN
            // if (AdsUtil.IsInEEA())
            // {
            //     MaxSdk.UserService.ShowConsentDialog();
            // }
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