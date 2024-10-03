#if PANCAKE_DEBUG_UI
using DebugUI;
#endif

namespace Pancake.DebugView
{
    public abstract class DebugPageBase
    {
#if PANCAKE_DEBUG_UI
        public abstract void Configure(DebugUIBuilder builder);
#endif

        public virtual void Dispose() { }
    }
}