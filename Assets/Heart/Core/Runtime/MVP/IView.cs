using System.Collections.Generic;

namespace Pancake
{
    public interface IView<TData>
    {
        void Initialize();
        void Cleanup();
        void UpdateView(TData data);
        event System.Action<TData> OnDataChanged;
    }
}