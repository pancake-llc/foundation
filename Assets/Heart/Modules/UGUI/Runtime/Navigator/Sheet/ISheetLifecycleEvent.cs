using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public interface ISheetLifecycleEvent
    {
        UniTask Initialize();
        UniTask WillEnter();
        UniTask WillExit();
        UniTask Cleanup();
        void DidEnter();
        void DidExit();
    }
}