using System;
#if PANCAKE_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.ControllerTree
{
    public interface IControllerBase : IDisposable, IControllerState
    {
#if PANCAKE_UNITASK
        UniTask Execute(CancellationToken token);
        UniTask Stop(bool isForced = false);
#endif
    }
}