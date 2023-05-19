using System;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobInterVariable : AdUnitVariable
    {
        [NonSerialized] public Action completedCallback;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private InterstitialAd _interstitialAd;
#endif
        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            Destroy();
            InterstitialAd.Load(Id, new AdRequest(), AdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _interstitialAd.Show();
#endif
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_interstitialAd == null) return;
            _interstitialAd.Destroy();
            _interstitialAd = null;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private void AdLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _interstitialAd = ad;
            _interstitialAd.OnAdPaid += OnAdPaided;
            _interstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _interstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _interstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _interstitialAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            OnAdLoaded();
        }

        private void OnAdImpressionRecorded() { throw new NotImplementedException(); }

        private void OnAdOpening()
        {
            AdSettings.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdFailedToShow(AdError error) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdClosed()
        {
            AdSettings.isShowingAd = false;
            C.CallActionClean(ref completedCallback);
            Destroy();
        }

        private void OnAdPaided(AdValue value) { paidedCallback?.Invoke(value.Value / 1000000f, Id, EAdNetwork.Admob.ToString()); }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref faildedToLoadCallback); }
#endif
    }
}