#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
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
        private ApplovinRewardedInterLoader _rewardedInter;
        private ApplovinAppOpenLoader _appOpen;
        private static ApplovinAdClient client;
        private bool _isBannerDestroyed;
        private bool _isEarnRewarded;
        private bool _isEarnRewardedInter;
        internal Action rewardedCompletedChain;
        internal Action rewardedDisplayChain;
        internal Action rewardedSkippedChain;
        internal Action rewardedClosedChain;
        internal Action interstitialDisplayChain;
        internal Action interstitialCompletedChain;
        internal Action rewardedInterCompletedChain;
        internal Action rewardedInterDisplayChain;
        internal Action rewardedInterSkippedChain;
        internal Action rewardedInterClosedChain;
        public static ApplovinAdClient Instance => client ??= new ApplovinAdClient();


        #region banner

        public event Action OnBannerAdLoaded;
        public event Action OnBannerAdFaildToLoad;
        public event Action OnBannerAdClicked;


        internal void InvokeBannerAdLoaded() { OnBannerAdLoaded?.Invoke(); }
        internal void InvokeBannerAdFaildToLoad() { OnBannerAdFaildToLoad?.Invoke(); }
        internal void InvokeBannerAdClicked() { OnBannerAdClicked?.Invoke(); }
        internal void InvokeBannerAdExpanded() { CallBannerAdDisplayed(); }
        internal void InvokeBannerAdCollapsed() { CallBannerAdCompleted(); }

        internal void InvokeBannerAdRevenuePaid(MaxSdkBase.AdInfo info)
        {
            CallBannerAdPaid(info.Revenue,
                info.NetworkName,
                info.AdUnitIdentifier,
                info.Placement,
                EAdNetwork.Applovin);
        }

        #endregion


        #region interstitial

        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdLoaded;
        public event Action OnInterstitialAdFaildToLoad;
        public event Action OnInterstitialAdFaildToDisplay;

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

        internal void InvokeInterstitialAdRevenuePaid(MaxSdkBase.AdInfo info)
        {
            CallInterstitialAdPaid(info.Revenue,
                info.NetworkName,
                info.AdUnitIdentifier,
                info.Placement,
                EAdNetwork.Applovin);
        }

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

        internal void InvokeRewardedAdRevenuePaid(MaxSdkBase.AdInfo info)
        {
            CallRewardedAdPaid(info.Revenue,
                info.NetworkName,
                info.AdUnitIdentifier,
                info.Placement,
                EAdNetwork.Applovin);
        }

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
        public event Action<MaxSdkBase.Reward> OnRewardedInterstitialAdReceivedReward;


        internal void InvokeRewardedInterstitialAdLoaded() { OnRewardedInterstitialAdLoaded?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToLoad() { OnRewardedInterstitialAdFaildToLoad?.Invoke(); }
        internal void InvokeRewardedInterstitialAdFaildToDisplay() { OnRewardedInterstitialAdFaildToDisplay?.Invoke(); }
        internal void InvokeRewardedInterstitialAdClicked() { OnRewardedInterstitialAdClicked?.Invoke(); }
        internal void InvokeRewardedInterstitialAdDisplay() { CallRewardedInterAdDisplayed(); }

        internal void InvokeRewardedInterstitialAdHidden()
        {
            R.isShowingAd = false;
            CallRewardedInterAdClosed();
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) LoadRewardedInterAd();
            if (_isEarnRewardedInter)
            {
                CallRewardedInterAdCompleted();
                return;
            }

            CallRewardedInterAdSkipped();
        }

        internal void InvokeRewardedInterstitialAdRevenuePaid(MaxSdkBase.AdInfo info)
        {
            CallRewardedInterAdPaid(info.Revenue,
                info.NetworkName,
                info.AdUnitIdentifier,
                info.Placement,
                EAdNetwork.Applovin);
        }

        internal void InvokeRewardedInterstitialAdReceivedReward(MaxSdkBase.Reward reward)
        {
            OnRewardedInterstitialAdReceivedReward?.Invoke(reward);
            _isEarnRewardedInter = true;
        }

        protected override void CallRewardedInterAdClosed()
        {
            C.CallActionClean(ref rewardedInterClosedChain);
            base.CallRewardedInterAdClosed();
        }

        protected override void CallRewardedInterAdCompleted()
        {
            C.CallActionClean(ref rewardedInterCompletedChain);
            base.CallRewardedInterAdCompleted();
        }

        protected override void CallRewardedInterAdSkipped()
        {
            C.CallActionClean(ref rewardedInterSkippedChain);
            base.CallRewardedInterAdSkipped();
        }

        protected override void CallRewardedInterAdDisplayed()
        {
            C.CallActionClean(ref rewardedInterDisplayChain);
            base.CallRewardedInterAdDisplayed();
        }

        #endregion


        #region app open

        public event Action OnAppOpenAdClicked;
        public event Action OnAppOpenAdLoaded;
        public event Action OnAppOpenAdFaildToLoaded;
        public event Action OnAppOpenAdFaildToDisplay;

        internal void InvokeAppOpenAdRevenuePaid(MaxSdkBase.AdInfo info)
        {
            CallAppOpenAdPaid(info.Revenue,
                info.NetworkName,
                info.AdUnitIdentifier,
                info.Placement,
                EAdNetwork.Applovin);
        }

        internal void InvokeAppOpenAdClicked() { OnAppOpenAdClicked?.Invoke(); }
        internal void InvokeAppOpenAdLoaded() { OnAppOpenAdLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToLoaded() { OnAppOpenAdFaildToLoaded?.Invoke(); }
        internal void InvokeAppOpenAdFaildToDisplay() { OnAppOpenAdFaildToDisplay?.Invoke(); }
        internal void InvokeAppOpenAdDisplay() { CallAppOpenAdDisplayed(); }
        internal void InvokeAppOpenAdCompleted() { CallAppOpenAdCompleted(); }

        protected override void CallAppOpenAdCompleted()
        {
            R.isShowingAd = false;
            base.CallAppOpenAdCompleted();
        }

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
            MaxSdk.InitializeSdk();
            MaxSdk.SetIsAgeRestrictedUser(AdSettings.MaxSettings.EnableAgeRestrictedUser);
#endif

            _banner = new ApplovinBannerLoader(this);
            _interstitial = new ApplovinInterstitialLoader(this);
            _rewarded = new ApplovinRewardedLoader(this);
            _rewardedInter = new ApplovinRewardedInterLoader(this);
            _appOpen = new ApplovinAppOpenLoader(this);

            isInitialized = true;
            LoadInterstitialAd();
            LoadRewardedAd();
            LoadRewardedInterAd();
            LoadAppOpenAd();
            _isBannerDestroyed = false;
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
            _isEarnRewardedInter = false;
            R.isShowingAd = true;
            MaxSdk.ShowRewardedInterstitialAd(AdSettings.MaxSettings.RewardedInterstitialAdUnit.Id);

            return _rewardedInter;
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
    }
}
#endif