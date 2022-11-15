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
        public static NoneAdClient Instance => client ?? (client = new NoneAdClient());

        protected override void InternalInit() { }

        protected override void InternalShowBannerAd() { }

        protected override void InternalHideBannerAd() { }

        protected override void InternalDestroyBannerAd() { }

        protected override void InternalLoadInterstitialAd() { }

        protected override bool InternalIsInterstitialAdReady() { return false; }

        protected override void InternalShowInterstitialAd() { }

        protected override void InternalLoadRewardedAd() { }

        protected override bool InternalIsRewardedAdReady() { return false; }

        protected override void InternalShowRewardedAd() { }
    }
}