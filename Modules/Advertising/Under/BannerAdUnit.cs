namespace Pancake.Monetization
{
    [System.Serializable]
    public class BannerAdUnit : AdUnit
    {
        public EBannerPosition position;

        public BannerAdUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
            position = EBannerPosition.Bottom; // default set banner display in bottom
        }
    }
}