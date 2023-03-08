namespace Pancake.Monetization
{
    [System.Serializable]
    public class AdmobIntestitialUnit : InterstitialAdUnit
    {
        public AdmobIntestitialUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}