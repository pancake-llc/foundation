namespace Pancake.Monetization
{
    public interface IInterstitial
    {
        void Register(string key, System.Action action);
    }
}