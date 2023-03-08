namespace Pancake.Monetization
{
    [System.Serializable]
    public class RewardedAdUnit : AdUnit
    {
        public RewardedAdUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}