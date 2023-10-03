using System;
using System.Threading.Tasks;

namespace Pancake.UI
{
    public static class PageExtensions
    {
        public static void AddLifecycleEvent(
            this Page self,
            Func<Task> initialize = null,
            Func<Task> onWillPushEnter = null,
            Action onDidPushEnter = null,
            Func<Task> onWillPushExit = null,
            Action onDidPushExit = null,
            Func<Task> onWillPopEnter = null,
            Action onDidPopEnter = null,
            Func<Task> onWillPopExit = null,
            Action onDidPopExit = null,
            Func<Task> onCleanup = null,
            int priority = 0)
        {
            var lifecycleEvent = new AnonymousPageLifecycleEvent(initialize,
                onWillPushEnter,
                onDidPushEnter,
                onWillPushExit,
                onDidPushExit,
                onWillPopEnter,
                onDidPopEnter,
                onWillPopExit,
                onDidPopExit,
                onCleanup);
            self.AddLifecycleEvent(lifecycleEvent, priority);
        }
    }
}