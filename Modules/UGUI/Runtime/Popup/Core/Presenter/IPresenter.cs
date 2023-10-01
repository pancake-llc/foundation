using System;

namespace Pancake.UI.Popup
{
    public interface IPresenter : IDisposable
    {
        bool IsDisposed { get; }
        bool IsInitialized { get; }
        void InitPresenter();
    }
}