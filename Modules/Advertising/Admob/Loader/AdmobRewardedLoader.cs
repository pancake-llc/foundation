using UnityEngine;
using System;
#if PANCAKE_ADMOB
using System.Collections.Generic;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobRewardedLoader : AdLoader<AdUnit>, IRewarded
    {
#if PANCAKE_ADMOB
        private RewardedAd _rewardedAd;
        public bool IsEarnRewarded { get; private set; }
        public event Action<AdmobRewardedLoader> OnCompleted = delegate { };
        public event Action<AdmobRewardedLoader> OnSkipped = delegate { };
        public event Action<AdmobRewardedLoader, object, AdValueEventArgs> OnPaidEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, EventArgs> OnOpeningEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, EventArgs> OnLoadedEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, AdErrorEventArgs> OnFailToShowEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, AdFailedToLoadEventArgs> OnFailToLoadEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, EventArgs> OnRecordImpressionEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, EventArgs> OnClosedEvent = delegate { };
        public event Action<AdmobRewardedLoader, object, Reward> OnRewardEvent = delegate { };

        private Action _completedCallback;
        private Action _skippedCallback;
        private Action _closedCallback;
        private Action _displayCallback;

        public AdmobRewardedLoader() { unit = AdSettings.AdmobSettings.RewardedAdUnit; }

        internal override bool IsReady() { return _rewardedAd != null && _rewardedAd.IsLoaded(); }

        internal override void Load()
        {
            _rewardedAd = new RewardedAd(unit.Id);
            _rewardedAd.OnAdClosed += OnAdClosed;
            _rewardedAd.OnAdFailedToLoad += OnAdFailedToLoad;
            _rewardedAd.OnAdFailedToShow += OnAdFailedToShow;
            _rewardedAd.OnAdLoaded += OnAdLoaded;
            _rewardedAd.OnAdOpening += OnAdOpening;
            _rewardedAd.OnAdDidRecordImpression += OnAdDidRecordImpression;
            _rewardedAd.OnPaidEvent += OnPaidHandleEvent;
            _rewardedAd.OnUserEarnedReward += OnRewardHandleEvent;
            _rewardedAd.LoadAd(Admob.CreateRequest());
        }

        private void OnRewardHandleEvent(object sender, Reward e)
        {
            IsEarnRewarded = true;
            OnRewardEvent.Invoke(this, sender, e);
        }

        private void OnPaidHandleEvent(object sender, AdValueEventArgs e)
        {
            OnPaidEvent.Invoke(this, sender, e);
#if PANCAKE_ANALYTIC
            AppTracking.TrackingRevenue(e, unit.Id);
#endif
        }

        private void OnAdDidRecordImpression(object sender, EventArgs e) { OnRecordImpressionEvent.Invoke(this, sender, e); }

        private void OnAdOpening(object sender, EventArgs e)
        {
            R.isShowingAd = true;
            OnOpeningEvent.Invoke(this, sender, e);
            C.CallCacheCleanAction(ref _displayCallback);
        }

        private void OnAdLoaded(object sender, EventArgs e) { OnLoadedEvent.Invoke(this, sender, e); }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e) { OnFailToShowEvent.Invoke(this, sender, e); }

        private void OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            OnFailToLoadEvent.Invoke(this, sender, e);
            Destroy();
        }

        private void OnAdClosed(object sender, EventArgs e)
        {
            R.isShowingAd = false;
            OnClosedEvent.Invoke(this, sender, e);
            C.CallCacheCleanAction(ref _closedCallback);
            if (IsEarnRewarded)
            {
                OnCompleted.Invoke(this);
                C.CallCacheCleanAction(ref _completedCallback);
            }
            else
            {
                OnSkipped.Invoke(this);
                C.CallCacheCleanAction(ref _skippedCallback);
            }

            Destroy();
        }

        internal override void Show() { _rewardedAd?.Show(); }

        internal override void Destroy()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
        }
#endif
        public void Register(string key, Action action)
        {
#if PANCAKE_ADMOB
            switch (key)
            {
                case "OnDisplayed":
                    _displayCallback = action;
                    break;
                case "OnCompleted":
                    _completedCallback = action;
                    break;
                case "OnClosed":
                    _closedCallback = action;
                    break;
                case  "OnSkipped":
                    _skippedCallback = action;
                    break;
            }
#endif
        }
    }
}