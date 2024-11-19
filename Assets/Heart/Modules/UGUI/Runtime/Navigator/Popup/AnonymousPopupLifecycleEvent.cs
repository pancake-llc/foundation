using System;
using System.Collections.Generic;
using Pancake.Linq;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    public sealed class AnonymousPopupLifecycleEvent : IPopupLifecycleEvent
    {
        public event Action OnDidPushEnter;
        public event Action OnDidPushExit;
        public event Action OnDidPopEnter;
        public event Action OnDidPopExit;

#if PANCAKE_UNITASK
        public List<Func<UniTask>> OnInitialize { get; } = new();
        public List<Func<UniTask>> OnWillPushEnter { get; } = new();
        public List<Func<UniTask>> OnWillPushExit { get; } = new();
        public List<Func<UniTask>> OnWillPopEnter { get; } = new();
        public List<Func<UniTask>> OnWillPopExit { get; } = new();
        public List<Func<UniTask>> OnCleanup { get; } = new();

        public AnonymousPopupLifecycleEvent(
            Func<UniTask> initialize = null,
            Func<UniTask> onWillPushEnter = null,
            Action onDidPushEnter = null,
            Func<UniTask> onWillPushExit = null,
            Action onDidPushExit = null,
            Func<UniTask> onWillPopEnter = null,
            Action onDidPopEnter = null,
            Func<UniTask> onWillPopExit = null,
            Action onDidPopExit = null,
            Func<UniTask> onCleanup = null)

        {
            if (initialize != null) OnInitialize.Add(initialize);

            if (onWillPushEnter != null) OnWillPushEnter.Add(onWillPushEnter);

            OnDidPushEnter = onDidPushEnter;

            if (onWillPushExit != null) OnWillPushExit.Add(onWillPushExit);

            OnDidPushExit = onDidPushExit;

            if (onWillPopEnter != null) OnWillPopEnter.Add(onWillPopEnter);

            OnDidPopEnter = onDidPopEnter;

            if (onWillPopExit != null) OnWillPopExit.Add(onWillPopExit);

            OnDidPopExit = onDidPopExit;

            if (onCleanup != null) OnCleanup.Add(onCleanup);
        }

        UniTask IPopupLifecycleEvent.Initialize() { return UniTask.WhenAll(OnInitialize.Map(x => x.Invoke())); }

        UniTask IPopupLifecycleEvent.WillPushEnter() { return UniTask.WhenAll(OnWillPushEnter.Map(x => x.Invoke())); }

        UniTask IPopupLifecycleEvent.WillPushExit() { return UniTask.WhenAll(OnWillPushExit.Map(x => x.Invoke())); }

        UniTask IPopupLifecycleEvent.WillPopEnter() { return UniTask.WhenAll(OnWillPopEnter.Map(x => x.Invoke())); }

        UniTask IPopupLifecycleEvent.WillPopExit() { return UniTask.WhenAll(OnWillPopExit.Map(x => x.Invoke())); }

        UniTask IPopupLifecycleEvent.Cleanup() { return UniTask.WhenAll(OnCleanup.Map(x => x.Invoke())); }
#endif

        void IPopupLifecycleEvent.DidPushEnter() { OnDidPushEnter?.Invoke(); }
        void IPopupLifecycleEvent.DidPushExit() { OnDidPushExit?.Invoke(); }
        void IPopupLifecycleEvent.DidPopEnter() { OnDidPopEnter?.Invoke(); }
        void IPopupLifecycleEvent.DidPopExit() { OnDidPopExit?.Invoke(); }
    }
}