namespace Pancake.Monetization
{
    public interface IRewardedInterstitial
    {
        void Register(string key, System.Action action);
    }
}