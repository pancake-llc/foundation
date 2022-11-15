namespace Pancake.Monetization
{
    public class IronSourceRewardedLoader
    {
        private readonly IronSourceAdClient _client;

        public IronSourceRewardedLoader(IronSourceAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_IRONSOURCE_ENABLE
            IronSourceEvents.onRewardedVideoAdOpenedEvent += OnAdOpened;
            IronSourceEvents.onRewardedVideoAdClickedEvent += OnAdClicked;
            IronSourceEvents.onRewardedVideoAdClosedEvent += OnAdClosed;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnAvailabilityChanged;
            IronSourceEvents.onRewardedVideoAdStartedEvent += OnAdStarted;
            IronSourceEvents.onRewardedVideoAdEndedEvent += OnAdEnded;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += OnAdRewarded;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += OnAdShowFailed;
#endif
        }

#if PANCAKE_IRONSOURCE_ENABLE
        private void OnAdOpened() { ; }

        private void OnAdClicked(IronSourcePlacement placement) { }

        private void OnAdClosed() { R.isShowingAd = false; }

        private void OnAvailabilityChanged(bool status) { }

        private void OnAdStarted() { }

        private void OnAdEnded() { }

        private void OnAdRewarded(IronSourcePlacement placement) { }

        private void OnAdShowFailed(IronSourceError error) { }
#endif
    }
}