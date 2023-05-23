using System;
using UnityEngine;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobAppOpenVariable : AdUnitVariable
    {
        public ScreenOrientation orientation = ScreenOrientation.Portrait;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private AppOpenAd _appOpenAd;
#endif
        private DateTime _expireTime;

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

            Destroy();
            AppOpenAd.Load(Id, orientation, new AdRequest(), OnAdLoadCallback);
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
            _appOpenAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            OnAdLoaded();

            // App open ads can be preloaded for up to 4 hours.
            _expireTime = DateTime.Now + TimeSpan.FromHours(4);
        }

        private void OnAdImpressionRecorded() { throw new NotImplementedException(); }

        private void OnAdOpening()
        {
            AdStatic.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdFailedToShow(AdError obj) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdClosed()
        {
            AdStatic.isShowingAd = false;
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

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref faildedToLoadCallback); }
#endif
        
#if UNITY_EDITOR
        [UnityEngine.ContextMenu("Copy Default Test Id")]
        protected void FillDefaultTestId()
        {
#if UNITY_ANDROID
            androidId = "ca-app-pub-3940256099942544/3419835294";
#elif UNITY_IOS
            iOSId = "ca-app-pub-3940256099942544/5662855259";
#endif
            foreach (UnityEditor.SceneView scene in UnityEditor.SceneView.sceneViews)
            {
                scene.ShowNotification(new UnityEngine.GUIContent("[Admob] Copy App Open Test Unit Id!"), 1.0f);
                scene.Repaint();
            }
        }
#endif
    }
}