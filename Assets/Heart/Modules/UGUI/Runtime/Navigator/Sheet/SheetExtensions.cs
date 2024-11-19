#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;


namespace Pancake.UI
{
    public static class SheetExtensions
    {
        public static void AddLifecycleEvent(
            this Sheet self,
            Func<UniTask> initialize = null,
            Func<UniTask> onWillEnter = null,
            Action onDidEnter = null,
            Func<UniTask> onWillExit = null,
            Action onDidExit = null,
            Func<UniTask> onCleanup = null,
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
#endif