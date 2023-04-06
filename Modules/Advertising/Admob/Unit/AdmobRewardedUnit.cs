namespace Pancake.Monetization
{
    [System.Serializable]
    public class AdmobRewardedUnit : RewardedAdUnit
    {
        public AdmobRewardedUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}