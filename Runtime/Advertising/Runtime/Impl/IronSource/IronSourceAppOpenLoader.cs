#if PANCAKE_IRONSOURCE_ENABLE && PANCAKE_ADMOB_ENABLE
using System;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class IronSourceAppOpenLoader : AdLoader<AdUnit>
    {
        private readonly IronSourceAdClient _client;
#if PANCAKE_IRONSOURCE_ENABLE && PANCAKE_ADMOB_ENABLE
        private AppOpenAd _appOpenAd;

        /// <summary>
        /// Ad references in the app open beta will time out after four hours.
        /// Ads rendered more than four hours after request time will no longer be valid and may not earn revenue.
        /// This time limit is being carefully considered and may change in future beta versions of the app open format.
        /// </summary>
        private DateTime _loadTime;

        internal override bool IsReady() { return _appOpenAd != null && (DateTime.UtcNow - _loadTime).TotalHours < 4; }
#endif
        public IronSourceAppOpenLoader(IronSourceAdClient client)
        {
            _client = client;
#if PANCAKE_IRONSOURCE_ENABLE && PANCAKE_ADMOB_ENABLE
            unit = Settings.IronSourceSettings.AppOpenAdUnit;
#endif
        }
#if PANCAKE_IRONSOURCE_ENABLE && PANCAKE_ADMOB_ENABLE
        internal override void Load() { AppOpenAd.LoadAd(unit.Id, ((IronSourceAppOpenUnit) unit).orientation, Admob.CreateRequest(), OnAdLoadCallback); }

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

        private void OnAdDidRecordImpression(object sender, EventArgs e) { _client.InvokeAppOpenAdDidRecordImpression(); }

        private void OnPaidHandleEvent(object sender, AdValueEventArgs e)
        {
            _client.InvokeAppOpenAdRevenuePaid(e);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(e, unit.Id);  
#endif
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e) { _client.InvokeAppOpenAdFailedToShow(); }

        private void OnAdOpening(object sender, EventArgs e)
        {
            R.isShowingAd = true;
            _client.InvokeAppOpenAdOpening();
        }

        private void OnAdClosed(object sender, EventArgs e)
        {
            R.isShowingAd = false;
            _client.InvokeAppOpenAdClosed();
            _client.InternalAppOpenAdCompleted(this);
            Destroy();
        }

        private void OnAdLoaded() { _client.InvokeAppOpenAdLoaded(); }

        private void OnAdFaildToLoad(AdFailedToLoadEventArgs e)
        {
            _client.InvokeAppOpenAdFailedToLoad();
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