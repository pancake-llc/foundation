using System;

namespace Pancake.Monetization
{
    public class IronSourceBannerLoader
    {
        private readonly IronSourceAdClient _client;

        public IronSourceBannerLoader(IronSourceAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_IRONSOURCE_ENABLE
            IronSourceEvents.onBannerAdLoadedEvent += OnAdLoaded;
            IronSourceEvents.onBannerAdLoadFailedEvent += OnAdLoadFailed;
            IronSourceEvents.onBannerAdClickedEvent += OnAdClicked;
            IronSourceEvents.onBannerAdScreenPresentedEvent += OnAdScreenPresented;
            IronSourceEvents.onBannerAdScreenDismissedEvent += OnAdScreenDismissed;
            IronSourceEvents.onBannerAdLeftApplicationEvent += OnAdLeftApplication;
#endif
        }

#if PANCAKE_IRONSOURCE_ENABLE
        private void OnAdLeftApplication() { _client.InvokeBannerAdLeftApplication(); }

        private void OnAdScreenDismissed() { _client.InvokeBannerAdScreenDismissed(); }

        private void OnAdScreenPresented() { _client.InvokeBannerAdScreenPresented(); }

        private void OnAdClicked() { _client.InvokeBannerAdClicked(); }

        private void OnAdLoadFailed(IronSourceError error) { _client.InvokeBannerAdLoadFailed(error); }

        private void OnAdLoaded() { _client.InvokeBannerAdLoaded(); }
#endif
    }
}