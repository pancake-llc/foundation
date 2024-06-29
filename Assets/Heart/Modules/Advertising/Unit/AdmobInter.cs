using System;
using Pancake.Common;
using UnityEngine;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobInter : AdUnit
    {
        [NonSerialized] internal Action completedCallback;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private InterstitialAd _interstitialAd;
#endif
        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (Advertising.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

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

        public override AdUnit Show()
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || string.IsNullOrEmpty(Id) || Advertising.IsRemoveAd || !IsReady()) return this;
            ShowImpl();
            return this;
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _interstitialAd.Show();
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
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
            OnAdLoaded();
        }

        private void OnAdOpening()
        {
            Advertising.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdFailedToShow(AdError error) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdClosed()
        {
            Advertising.isShowingAd = false;
            C.CallActionClean(ref completedCallback);
            Destroy();
        }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "InterstitialAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref failedToLoadCallback); }
#endif
    }
}