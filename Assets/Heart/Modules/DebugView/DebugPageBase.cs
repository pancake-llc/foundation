using DebugUI;

namespace Pancake.DebugView
{
    public abstract class DebugPageBase
    {
        public abstract void Configure(DebugUIBuilder builder);

        public virtual void Dispose() { }
    }
}