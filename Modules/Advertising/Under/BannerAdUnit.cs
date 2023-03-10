namespace Pancake.Monetization
{
    [System.Serializable]
    public class BannerAdUnit : AdUnit
    {
        public EBannerSize size;
        public EBannerPosition position;

        public BannerAdUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
            position = EBannerPosition.Bottom;
            size = EBannerSize.Adaptive;
        }
    }
}