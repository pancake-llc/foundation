#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    public interface ISheetLifecycleEvent
    {
#if PANCAKE_UNITASK
        UniTask Initialize();
        UniTask WillEnter();
        UniTask WillExit();
        UniTask Cleanup();
#endif
        void DidEnter();
        void DidExit();
    }
}