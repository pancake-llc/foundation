#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;

namespace Pancake.UI
{
    public static class PopupExtensions
    {
        public static void AddLifecycleEvent(
            this Popup self,
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
            var lifecycleEvent = new AnonymousPopupLifecycleEvent(initialize,
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