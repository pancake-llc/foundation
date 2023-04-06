namespace Pancake.Monetization
{
    public class NoneAdClient : AdClient
    {
        public override EAdNetwork Network => EAdNetwork.None;
        public override bool IsBannerAdSupported => false;
        public override bool IsInsterstitialAdSupport => false;
        public override bool IsRewardedAdSupport => false;
        public override bool IsRewardedInterstitialAdSupport => false;
        public override bool IsAppOpenAdSupport => false;
        public override bool IsSdkAvaiable => true;
        protected override string NoSdkMessage => "";

        private static NoneAdClient client;
        public static NoneAdClient Instance => client ??= new NoneAdClient();

        protected override void InternalInit() { }

        protected override void InternalShowBannerAd() { }

        protected override void InternalDestroyBannerAd() { }

        protected override void InternalLoadInterstitialAd() { }

        protected override bool InternalIsInterstitialAdReady() { return false; }

        protected override IInterstitial InternalShowInterstitialAd() { return null; }

        protected override void InternalLoadRewardedAd() { }

        protected override bool InternalIsRewardedAdReady() { return false; }

        protected override IRewarded InternalShowRewardedAd() { return null; }

        protected override void InternalLoadRewardedInterstitialAd() { }

        protected override IRewardedInterstitial InternalShowRewardedInterstitialAd() { return null; }

        protected override bool InternalIsRewardedInterstitialAdReady() { return false; }

        protected override void InternalLoadAppOpenAd() { }

        protected override void InternalShowAppOpenAd() { }

        protected override bool InternalIsAppOpenAdReady() { return false; }
    }
}