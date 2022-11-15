#if PANCAKE_ADMOB_ENABLE
using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
#endif

namespace Pancake.Monetization
{
    public class AdmobBannerLoader : AdLoader<AdUnit>
    {
#if PANCAKE_ADMOB_ENABLE
        private BannerView _bannerView;
        private bool _isLoaded;
        public event Action<AdmobBannerLoader, object, EventArgs> OnClosedEvent = delegate { };
        public event Action<AdmobBannerLoader, object, AdFailedToLoadEventArgs> OnFailToLoadEvent = delegate { };
        public event Action<AdmobBannerLoader, object, EventArgs> OnLoadedEvent = delegate { };
        public event Action<AdmobBannerLoader, object, EventArgs> OnOpeningEvent = delegate { };
        public event Action<AdmobBannerLoader, object, AdValueEventArgs> OnPaidEvent = delegate { };


        public AdmobBannerLoader() { unit = Settings.AdmobSettings.BannerAdUnit; }

        internal override void Load()
        {
            var admobBannerUnit = (AdmobBannerUnit)unit;
            _bannerView = new BannerView(unit.Id, admobBannerUnit.ConvertSize(), admobBannerUnit.ConvertPosition());
            _bannerView.OnAdClosed += OnAdClosed;
            _bannerView.OnAdFailedToLoad += OnAdFailedToLoad;
            _bannerView.OnAdLoaded += OnAdLoaded;
            _bannerView.OnAdOpening += OnAdOpening;
            _bannerView.OnPaidEvent += OnPaidHandleEvent;
            _bannerView.LoadAd(Admob.CreateRequest());
        }

        private void OnPaidHandleEvent(object sender, AdValueEventArgs e)
        {
            OnPaidEvent.Invoke(this, sender, e);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(e, unit.Id);  
#endif
        }

        private void OnAdOpening(object sender, EventArgs e) { OnOpeningEvent.Invoke(this, sender, e); }

        private void OnAdLoaded(object sender, EventArgs e)
        {
            _isLoaded = true;
            OnLoadedEvent.Invoke(this, sender, e);
        }

        private void OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            OnFailToLoadEvent.Invoke(this, sender, e);
            RuntimeHelper.RunCoroutine(DelayReload(10f));
        }

        private void OnAdClosed(object sender, EventArgs e) { OnClosedEvent.Invoke(this, sender, e); }

        internal override void Show() { _bannerView?.Show(); }

        internal override void Destroy()
        {
            _bannerView?.Destroy();
            _bannerView = null;
            _isLoaded = false;
        }
#endif


        internal void Hide()
        {
#if PANCAKE_ADMOB_ENABLE
            _bannerView?.Hide();
#endif
        }
#if PANCAKE_ADMOB_ENABLE
        internal override bool IsReady() { return _isLoaded; }

        private IEnumerator DelayReload(float delay)
        {
            yield return new WaitForSeconds(delay);
            Load();
        }

        internal float GetAdaptiveBannerHeight()
        {
            return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth).Height;
        }
#endif
    }
}