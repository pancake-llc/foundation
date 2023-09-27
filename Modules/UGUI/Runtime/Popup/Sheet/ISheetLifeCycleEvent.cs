using System.Threading.Tasks;

namespace Pancake.UI
{
    public interface ISheetLifeCycleEvent
    {
        Task Initialize();
        Task WillEnter();
        Task WillExit();
        Task Cleanup();
        void DidEnter();
        void DidExit();
    }
}