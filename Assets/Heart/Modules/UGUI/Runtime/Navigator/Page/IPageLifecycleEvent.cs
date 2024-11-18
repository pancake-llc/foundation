using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public interface IPageLifecycleEvent
    {
        UniTask Initialize();
        UniTask WillPushEnter();
        UniTask WillPushExit();
        UniTask WillPopEnter();
        UniTask WillPopExit();
        UniTask Cleanup();
        void DidPushEnter();
        void DidPushExit();
        void DidPopEnter();
        void DidPopExit();
    }
}