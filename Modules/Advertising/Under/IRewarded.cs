namespace Pancake.Monetization
{
    public interface IRewarded
    {
        void Register(string key, System.Action action);
    }
}