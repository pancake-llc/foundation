using System;
using System.Threading;
using UnityEngine;
using Pancake.Common;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobBanner : AdUnit, IBannerHide
    {
        public EBannerSize size = EBannerSize.Adaptive;
        public EBannerPosition position = EBannerPosition.Bottom;
        public EBannerCollapsiblePosition collapsiblePosition = EBannerCollapsiblePosition.None;
        private bool _isBannerShowing;
        private bool _previousBannerShowStatus;
        private CancellationTokenSource _tokenSource;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private BannerView _bannerView;
#endif

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (Advertising.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

            Destroy();
            _bannerView = new BannerView(Id, ConvertSize(), ConvertPosition());
            _bannerView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerView.OnAdPaid += OnAdPaided;
            var adRequest = new AdRequest();
            if (collapsiblePosition != EBannerCollapsiblePosition.None) adRequest.Extras.Add("collapsible", collapsiblePosition.ToString().ToLower());
            _bannerView.LoadAd(adRequest);
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

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return _bannerView != null;
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _isBannerShowing = true;
            Advertising.waitAppOpenClosedAction = OnWaitAppOpenClosed;
            Advertising.waitAppOpenDisplayedAction = OnWaitAppOpenDisplayed;
            _bannerView.Show();
#endif
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_bannerView == null) return;
            _isBannerShowing = false;
            Advertising.waitAppOpenClosedAction = null;
            Advertising.waitAppOpenDisplayedAction = null;
            _bannerView.Destroy();
            _bannerView = null;
#endif
        }

        public void Hide()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _isBannerShowing = false;
            _bannerView?.Hide();
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        public AdSize ConvertSize()
        {
            switch (size)
            {
                case EBannerSize.Adaptive: return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                default: return AdSize.Banner;
            }
        }

        public AdPosition ConvertPosition()
        {
            switch (position)
            {
                case EBannerPosition.Top: return AdPosition.Top;
                case EBannerPosition.Bottom: return AdPosition.Bottom;
                case EBannerPosition.TopLeft: return AdPosition.TopLeft;
                case EBannerPosition.TopRight: return AdPosition.TopRight;
                case EBannerPosition.BottomLeft: return AdPosition.BottomLeft;
                case EBannerPosition.BottomRight: return AdPosition.BottomRight;
                default: return AdPosition.Bottom;
            }
        }

        public bool IsCollapsible() { return collapsiblePosition != EBannerCollapsiblePosition.None && _bannerView.IsCollapsible(); }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "BannerAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdOpening() { C.CallActionClean(ref displayedCallback); }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            C.CallActionClean(ref failedToLoadCallback);

            StopReload();
            _tokenSource = new CancellationTokenSource();
            DelayBannerReload();
        }

        private void OnAdClosed() { C.CallActionClean(ref closedCallback); }

        private void StopReload()
        {
            if (_tokenSource == null) return;

            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        private async void DelayBannerReload()
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(5f, _tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Load();
        }
#endif
    }
}