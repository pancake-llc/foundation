namespace Pancake.Monetization
{
    public class IronSourceInterstitialLoader
    {
        private readonly IronSourceAdClient _client;

        public IronSourceInterstitialLoader(IronSourceAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_IRONSOURCE_ENABLE
            IronSourceEvents.onInterstitialAdClickedEvent += OnAdClicked;
            IronSourceEvents.onInterstitialAdClosedEvent += OnAdClosed;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += OnAdLoadFailed;
            IronSourceEvents.onInterstitialAdOpenedEvent += OnAdOpened;
            IronSourceEvents.onInterstitialAdReadyEvent += OnAdReady;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += OnAdShowSucceeded;
            IronSourceEvents.onInterstitialAdShowFailedEvent += OnAdShowFailed;
#endif
        }

#if PANCAKE_IRONSOURCE_ENABLE
        private void OnAdClicked() { _client.InvokeInterstitialAdClicked(); }

        private void OnAdClosed()
        {
            R.isShowingAd = false;
            _client.InvokeInterstitialAdClosed();
        }

        private void OnAdLoadFailed(IronSourceError error) { _client.InvokeInterstitialAdLoadFailed(error); }

        private void OnAdOpened() { _client.InvokeInterstitialAdOpened(); }

        private void OnAdReady() { _client.InvokeInterstitialAdReady(); }

        private void OnAdShowSucceeded() { _client.InvokeInterstitialAdShowSucceeded(); }

        private void OnAdShowFailed(IronSourceError error) { _client.InvokeInterstitialAdShowFailed(error); }
#endif
    }
}