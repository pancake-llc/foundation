
using UnityEngine;
#if PANCAKE_ADMOB_ENABLE
using System;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobInterstitialLoader : AdLoader<AdUnit>
    {
#if PANCAKE_ADMOB_ENABLE
        private InterstitialAd _interstitialAd;
        public event Action<AdmobInterstitialLoader> OnCompleted = delegate { };
        public event Action<AdmobInterstitialLoader, object, EventArgs> OnClosedEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, AdFailedToLoadEventArgs> OnFailToLoadEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, AdErrorEventArgs> OnFailToShowEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, EventArgs> OnLoadedEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, EventArgs> OnOpeningEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, EventArgs> OnRecordImpressionEvent = delegate { };
        public event Action<AdmobInterstitialLoader, object, AdValueEventArgs> OnPaidEvent = delegate { };

        public AdmobInterstitialLoader() { unit = Settings.AdmobSettings.InterstitialAdUnit; }

        internal override void Load()
        {
            _interstitialAd = new InterstitialAd(unit.Id);
            _interstitialAd.OnAdClosed += OnAdClosed;
            _interstitialAd.OnAdFailedToLoad += OnAdFailedToLoad;
            _interstitialAd.OnAdFailedToShow += OnAdFailedToShow;
            _interstitialAd.OnAdLoaded += OnAdLoaded;
            _interstitialAd.OnAdOpening += OnAdOpening;
            _interstitialAd.OnAdDidRecordImpression += OnAdDidRecordImpression;
            _interstitialAd.OnPaidEvent += OnPaidHandleEvent;
            _interstitialAd.LoadAd(Admob.CreateRequest());
        }

        private void OnPaidHandleEvent(object sender, AdValueEventArgs e)
        {
            OnPaidEvent.Invoke(this, sender, e); 
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(e, unit.Id);  
#endif
        }

        private void OnAdDidRecordImpression(object sender, EventArgs e) { OnRecordImpressionEvent.Invoke(this, sender, e); }

        private void OnAdOpening(object sender, EventArgs e)
        {
            R.isShowingAd = true;
            OnOpeningEvent.Invoke(this, sender, e);
        }

        private void OnAdLoaded(object sender, EventArgs e) { OnLoadedEvent.Invoke(this, sender, e); }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e) { OnFailToShowEvent.Invoke(this, sender, e); }

        private void OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e) { OnFailToLoadEvent.Invoke(this, sender, e); }

        private void OnAdClosed(object sender, EventArgs e)
        {
            R.isShowingAd = false;
            OnClosedEvent.Invoke(this, sender, e);
            OnCompleted.Invoke(this);
            Destroy();
        }


        internal override void Show() { _interstitialAd?.Show(); }

        internal override bool IsReady() { return _interstitialAd != null && _interstitialAd.IsLoaded(); }

        internal override void Destroy()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;
        }
#endif
    }
}