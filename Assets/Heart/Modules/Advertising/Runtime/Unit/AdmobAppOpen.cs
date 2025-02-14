using System;
using Pancake.Common;
using UnityEngine;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobAppOpen : AdUnit
    {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private AppOpenAd _appOpenAd;
#endif
        private DateTime _expireTime;

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (Advertising.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

            Destroy();
            AppOpenAd.Load(Id, new AdRequest(), OnAdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return _appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime;
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
            _appOpenAd.Show();
#endif
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_appOpenAd == null) return;
            _appOpenAd.Destroy();
            _appOpenAd = null;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private void OnAdLoadCallback(AppOpenAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _appOpenAd = ad;
            _appOpenAd.OnAdPaid += OnAdPaided;
            _appOpenAd.OnAdFullScreenContentClosed += OnAdClosed;
            _appOpenAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _appOpenAd.OnAdFullScreenContentOpened += OnAdOpening;
            OnAdLoaded();

            // App open ads can be preloaded for up to 4 hours.
            _expireTime = DateTime.Now + TimeSpan.FromHours(4);
        }

        private void OnAdOpening()
        {
            Advertising.waitAppOpenDisplayedAction?.Invoke();
            Advertising.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdFailedToShow(AdError obj) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdClosed()
        {
            Advertising.waitAppOpenClosedAction?.Invoke();
            Advertising.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            Destroy();
        }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "AppOpenAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref failedToLoadCallback); }
#endif
    }
}