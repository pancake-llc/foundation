using System;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobBannerVariable : AdUnitVariable
    {
        public EBannerSize size;
        public EBannerPosition position;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private BannerView _bannerView;
#endif

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            Destroy();
            _bannerView = new BannerView(Id, ConvertSize(), ConvertPosition());
            _bannerView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerView.OnAdPaid += OnAdPaided;
            _bannerView.LoadAd(new AdRequest());
#endif
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
            _bannerView.Show();
#endif
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_bannerView == null) return;
            _bannerView.Destroy();
            _bannerView = null;
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

        private void OnAdPaided(AdValue value) { paidedCallback?.Invoke(value.Value / 1000000f, Id, EAdNetwork.Admob.ToString()); }

        private void OnAdOpening() { C.CallActionClean(ref displayedCallback); }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref faildedToLoadCallback); }

        private void OnAdClosed() { C.CallActionClean(ref closedCallback); }
#endif
    }
}