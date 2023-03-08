namespace Pancake.Monetization
{
    [System.Serializable]
    public class AppOpenAdUnit : AdUnit
    {
        public AppOpenAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}