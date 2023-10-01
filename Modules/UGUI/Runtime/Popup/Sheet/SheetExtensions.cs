using System;
using System.Threading.Tasks;


namespace Pancake.UI
{
    public static class SheetExtensions
    {
        public static void AddLifecycleEvent(
            this Sheet self,
            Func<Task> initialize = null,
            Func<Task> onWillEnter = null,
            Action onDidEnter = null,
            Func<Task> onWillExit = null,
            Action onDidExit = null,
            Func<Task> onCleanup = null,
            int priority = 0)
        {
            var lifecycleEvent = new AnonymousSheetLifecycleEvent(initialize,
                onWillEnter,
                onDidEnter,
                onWillExit,
                onDidExit,
                onCleanup);
            self.AddLifecycleEvent(lifecycleEvent, priority);
        }
    }
}