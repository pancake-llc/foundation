namespace Pancake.Monetization
{
    public abstract class AdClient
    {
        protected bool statusAppOpenFirstIgnore;

        public abstract void Init();
        public abstract void LoadBanner();
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