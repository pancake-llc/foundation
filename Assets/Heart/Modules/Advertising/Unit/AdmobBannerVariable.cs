using System;
using UnityEngine;
using System.Collections;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobBannerVariable : AdUnitVariable, IBannerHide
    {
        public EBannerSize size = EBannerSize.Adaptive;
        public EBannerPosition position = EBannerPosition.Bottom;
        private EventBinding<WaitAppOpenDisplayed> _waitAppOpenDisplayed;
        private EventBinding<WaitAppOpenClosed> _waitAppOpenClosed;
        private bool _isBannerShowing;
        private bool _previousBannerShowStatus;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private BannerView _bannerView;
#endif

        private readonly WaitForSeconds _waitBannerReload = new WaitForSeconds(5f);
        private AsyncProcessHandle _reload;

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

            Destroy();
            _bannerView = new BannerView(Id, ConvertSize(), ConvertPosition());
            _bannerView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerView.OnAdPaid += OnAdPaided;
            _bannerView.LoadAd(new AdRequest());
            _waitAppOpenDisplayed ??= new EventBinding<WaitAppOpenDisplayed>(OnWaitAppOpenDisplayed);
            _waitAppOpenClosed ??= new EventBinding<WaitAppOpenClosed>(OnWaitAppOpenClosed);
            _waitAppOpenDisplayed.Listen = false;
            _waitAppOpenClosed.Listen = false;
#endif
        }

        private void OnWaitAppOpenClosed(WaitAppOpenClosed _)
        {
            if (_previousBannerShowStatus)
            {
                _previousBannerShowStatus = false;
                Show();
            }
        }

        private void OnWaitAppOpenDisplayed(WaitAppOpenDisplayed _)
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
            _bannerView.Show();
#endif
        }

        public override void Destroy()
        {
            _isBannerShowing = false;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_bannerView == null) return;
            _bannerView.Destroy();
            _bannerView = null;
#endif
        }

        public void Hide()
        {
            _isBannerShowing = false;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
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

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "BannerAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdOpening()
        {
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            C.CallActionClean(ref failedToLoadCallback);

            if (_reload is {IsTerminated: false}) App.StopCoroutine(_reload);
            _reload = App.StartCoroutine(DelayBannerReload());
        }

        private void OnAdClosed()
        {
            C.CallActionClean(ref closedCallback);
        }

        private IEnumerator DelayBannerReload()
        {
            yield return _waitBannerReload;
            Load();
        }
#endif

#if UNITY_EDITOR
        [UnityEngine.ContextMenu("Copy Default Test Id")]
        protected void FillDefaultTestId()
        {
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/6300978111".CopyToClipboard();
#elif UNITY_IOS
            "ca-app-pub-3940256099942544/2934735716".CopyToClipboard();
#endif
            DebugEditor.Toast("[Admob] Copy Banner Test Unit Id Success!");
        }
#endif
    }
}