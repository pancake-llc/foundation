using DebugUI;
using UnityEngine.UIElements;

namespace Pancake.DebugView
{
    public abstract class DebugPageBase
    {
        protected abstract string Label { get; }
        public abstract void Configure(DebugUIBuilder builder);
    }
}