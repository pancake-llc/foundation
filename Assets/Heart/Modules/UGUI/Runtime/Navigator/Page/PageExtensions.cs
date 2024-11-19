#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public static class PageExtensions
    {
        public static void AddLifecycleEvent(
            this Page self,
            Func<UniTask> initialize = null,
            Func<UniTask> onWillPushEnter = null,
            Action onDidPushEnter = null,
            Func<UniTask> onWillPushExit = null,
            Action onDidPushExit = null,
            Func<UniTask> onWillPopEnter = null,
            Action onDidPopEnter = null,
            Func<UniTask> onWillPopExit = null,
            Action onDidPopExit = null,
            Func<UniTask> onCleanup = null,
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
#endif