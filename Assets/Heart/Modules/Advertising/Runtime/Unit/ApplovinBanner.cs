using System;
using Pancake.Common;
using UnityEngine;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinBanner : AdUnit, IBannerHide
    {
        public EBannerSize size;
        public EBannerPosition position;

#pragma warning disable 0414
        private bool _isBannerDestroyed = true;
        private bool _registerCallback;
        private bool _isBannerShowing;
        private bool _previousBannerShowStatus;
#pragma warning restore 0414

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (Advertising.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            if (!_registerCallback)
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdExpanded;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnAdCollapsed;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                // The latest MAX Unity plugin (versions 4.3.1 and above) enables adaptive banners automatically
                if (size != EBannerSize.Adaptive) MaxSdk.SetBannerExtraParameter(Id, "adaptive_banner", "false");
                
                _registerCallback = true;
            }

            if (_isBannerDestroyed)
            {
                MaxSdk.CreateBanner(Id, ConvertPosition());
                _isBannerDestroyed = false;
            }
#endif
        }

        private void OnWaitAppOpenClosed()
        {
            if (_previousBannerShowStatus)
            {
                _previousBannerShowStatus = false;
                Show();
            }
        }

        private void OnWaitAppOpenDisplayed()
        {
            _previousBannerShowStatus = _isBannerShowing;
            if (_isBannerShowing) Hide();
        }

        public override bool IsReady() { return !string.IsNullOrEmpty(Id); }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            _isBannerShowing = true;
            Advertising.waitAppOpenClosedAction = OnWaitAppOpenClosed;
            Advertising.waitAppOpenDisplayedAction = OnWaitAppOpenDisplayed;
            Load(); // load banner again if destroyed banner before
            MaxSdk.ShowBanner(Id);
#endif
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (string.IsNullOrEmpty(Id)) return;
            _isBannerShowing = false;
            _isBannerDestroyed = true;
            Advertising.waitAppOpenClosedAction = null;
            Advertising.waitAppOpenDisplayedAction = null;
            MaxSdk.DestroyBanner(Id);
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        public MaxSdkBase.BannerPosition ConvertPosition()
        {
            switch (position)
            {
                case EBannerPosition.Top: return MaxSdkBase.BannerPosition.TopCenter;
                case EBannerPosition.Bottom: return MaxSdkBase.BannerPosition.BottomCenter;
                case EBannerPosition.TopLeft: return MaxSdkBase.BannerPosition.TopLeft;
                case EBannerPosition.TopRight: return MaxSdkBase.BannerPosition.TopRight;
                case EBannerPosition.BottomLeft: return MaxSdkBase.BannerPosition.BottomLeft;
                case EBannerPosition.BottomRight: return MaxSdkBase.BannerPosition.BottomRight;
                default:
                    return MaxSdkBase.BannerPosition.BottomCenter;
            }
        }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            paidedCallback?.Invoke(info.Revenue,
                info.NetworkName,
                unit,
                info.AdFormat,
                EAdNetwork.Applovin.ToString());
        }

        private void OnAdCollapsed(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref closedCallback); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo info) { C.CallActionClean(ref failedToLoadCallback); }

        private void OnAdExpanded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref displayedCallback); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

#endif
        public void Hide()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            _isBannerShowing = false;
            if (string.IsNullOrEmpty(Id)) return;
            MaxSdk.HideBanner(Id);
#endif
        }
    }
}