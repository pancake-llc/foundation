using Pancake.Apex;
using UnityEngine;

namespace Pancake.Monetization
{
    [HideMonoScript]
    public abstract class AdClient : ScriptableObject
    {
        [SerializeField, ReadOnly] protected AdSettings adSettings;
        protected bool statusAppOpenFirstIgnore;

        public void SetupSetting(AdSettings adSettings) { this.adSettings = adSettings; }

        public abstract void Init();
        public abstract void LoadInterstitial();
        public abstract bool IsInterstitialReady();
        public abstract void LoadRewarded();
        public abstract bool IsRewardedReady();
        public abstract void LoadRewardedInterstitial();
        public abstract bool IsRewardedInterstitialReady();
        public abstract void LoadAppOpen();
        public abstract bool IsAppOpenReady();
    }
}