namespace Pancake.Monetization
{
    public class AdLoader<T> where T : AdUnit
    {
        protected T unit;
        internal virtual bool IsReady() { return false; }
        internal virtual void Show() { }
        internal virtual void Load() { }
        internal virtual void Destroy() { }
    }
}