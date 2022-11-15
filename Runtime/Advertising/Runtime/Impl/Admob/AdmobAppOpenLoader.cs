#if PANCAKE_ADMOB_ENABLE
using System;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobAppOpenLoader : AdLoader<AdUnit>
    {
#if PANCAKE_ADMOB_ENABLE
        private AppOpenAd _appOpenAd;

        /// <summary>
        /// Ad references in the app open beta will time out after four hours.
        /// Ads rendered more than four hours after request time will no longer be valid and may not earn revenue.
        /// This time limit is being carefully considered and may change in future beta versions of the app open format.
        /// </summary>
        private DateTime _loadTime;

        public event Action<AdmobAppOpenLoader> OnCompleted = delegate { };
        public event Action<AdmobAppOpenLoader, object, AdValueEventArgs> OnPaidEvent = delegate { };
        public event Action<AdmobAppOpenLoader, object, EventArgs> OnOpeningEvent = delegate { };
        public event Action<AdmobAppOpenLoader> OnLoadedEvent = delegate { };
        public event Action<AdmobAppOpenLoader, object, AdErrorEventArgs> OnFailToShowEvent = delegate { };
        public event Action<AdmobAppOpenLoader, AdFailedToLoadEventArgs> OnFailToLoadEvent = delegate { };
        public event Action<AdmobAppOpenLoader, object, EventArgs> OnRecordImpressionEvent = delegate { };
        public event Action<AdmobAppOpenLoader, object, EventArgs> OnClosedEvent = delegate { };

        internal override bool IsReady()
        {
            return _appOpenAd != null && (DateTime.UtcNow - _loadTime).TotalHours < 4;
            ;
        }

        public AdmobAppOpenLoader() { unit = Settings.AdmobSettings.AppOpenAdUnit; }

        internal override void Load() { AppOpenAd.LoadAd(unit.Id, ((AdmobAppOpenUnit) unit).orientation, Admob.CreateRequest(), OnAdLoadCallback); }

        private void OnAdLoadCallback(AppOpenAd appOpenAd, AdFailedToLoadEventArgs error)
        {
            if (error != null)
            {
                OnAdFaildToLoad(error);
                return;
            }

            _appOpenAd = appOpenAd;
            _loadTime = DateTime.UtcNow;
            OnAdLoaded();
            _appOpenAd.OnAdDidDismissFullScreenContent += OnAdClosed;
            _appOpenAd.OnAdDidRecordImpression += OnAdDidRecordImpression;
            _appOpenAd.OnAdDidPresentFullScreenContent += OnAdOpening;
            _appOpenAd.OnAdFailedToPresentFullScreenContent += OnAdFailedToShow;
            _appOpenAd.OnPaidEvent += OnPaidHandleEvent;
        }

        private void OnPaidHandleEvent(object sender, AdValueEventArgs e)
        {
            OnPaidEvent.Invoke(this, sender, e);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(e, unit.Id);  
#endif
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e) { OnFailToShowEvent.Invoke(this, sender, e); }

        private void OnAdOpening(object sender, EventArgs e)
        {
            R.isShowingAd = true;
            OnOpeningEvent.Invoke(this, sender, e);
        }

        private void OnAdDidRecordImpression(object sender, EventArgs e) { OnRecordImpressionEvent.Invoke(this, sender, e); }

        private void OnAdClosed(object sender, EventArgs e)
        {
            R.isShowingAd = false;
            OnClosedEvent.Invoke(this, sender, e);
            OnCompleted.Invoke(this);
            Destroy();
        }

        private void OnAdLoaded() { OnLoadedEvent.Invoke(this); }

        private void OnAdFaildToLoad(AdFailedToLoadEventArgs e)
        {
            OnFailToLoadEvent.Invoke(this, e);
            Destroy();
        }

        internal override void Show() { _appOpenAd?.Show(); }

        internal override void Destroy()
        {
            _appOpenAd?.Destroy();
            _appOpenAd = null;
        }
#endif
    }
}