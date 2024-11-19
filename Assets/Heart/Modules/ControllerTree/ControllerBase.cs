using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pancake.ControllerTree
{
    public abstract class ControllerBase : IControllerBase
    {
        public void Dispose() { throw new System.NotImplementedException(); }

        public EControllerState State { get; }
        public UniTask Execute(CancellationToken token) { throw new System.NotImplementedException(); }

        public UniTask Stop(bool isForced = false) { throw new System.NotImplementedException(); }
        
        
        
        
    }
}