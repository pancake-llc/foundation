using Pancake.Apex;
using UnityEngine;

namespace Pancake.Monetization
{
    public abstract class AdClient : ScriptableObject
    {
        public abstract EAdNetwork ClientType { get; }
        [SerializeField, ReadOnly] protected AdSettings adSettings;

        public void SetupSetting(AdSettings adSettings) { this.adSettings = adSettings; }

        public abstract void Init();
        public abstract AdUnitVariable ShowBanner();
        public abstract void DestroyBanner();
        public abstract AdUnitVariable ShowInterstitial();
        public abstract void LoadInterstitial();
        public abstract bool IsInterstitialReady();
        public abstract AdUnitVariable ShowRewarded();
        public abstract void LoadRewarded();
        public abstract bool IsRewardedReady();
        public abstract AdUnitVariable ShowRewardedInterstitial();
        public abstract void LoadRewardedInterstitial();
        public abstract bool IsRewardedInterstitialReady();
        public abstract AdUnitVariable ShowAppOpen();
        public abstract void LoadAppOpen();
        public abstract bool IsAppOpenReady();
    }
}