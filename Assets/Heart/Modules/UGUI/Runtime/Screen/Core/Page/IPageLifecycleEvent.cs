using System.Threading.Tasks;

namespace Pancake.UI
{
    public interface IPageLifecycleEvent
    {
        Task Initialize();
        Task WillPushEnter();
        Task WillPushExit();
        Task WillPopEnter();
        Task WillPopExit();
        Task Cleanup();
        void DidPushEnter();
        void DidPushExit();
        void DidPopEnter();
        void DidPopExit();
    }
}