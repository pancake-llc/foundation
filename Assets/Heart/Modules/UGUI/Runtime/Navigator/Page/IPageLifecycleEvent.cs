#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    public interface IPageLifecycleEvent
    {
#if PANCAKE_UNITASK
        UniTask Initialize();
        UniTask WillPushEnter();
        UniTask WillPushExit();
        UniTask WillPopEnter();
        UniTask WillPopExit();
        UniTask Cleanup();
#endif
        void DidPushEnter();
        void DidPushExit();
        void DidPopEnter();
        void DidPopExit();
    }
}