using System;

namespace Pancake.UI
{
    public interface IPresenter : IDisposable
    {
        bool IsDisposed { get; }
        bool IsInitialized { get; }
        void InitPresenter();
    }
}