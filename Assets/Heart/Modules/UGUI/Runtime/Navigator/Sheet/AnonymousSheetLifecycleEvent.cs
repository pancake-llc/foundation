using System;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Pancake.Linq;
#endif

namespace Pancake.UI
{
    public sealed class AnonymousSheetLifecycleEvent : ISheetLifecycleEvent
    {
        public event Action OnDidEnter;
        public event Action OnDidExit;

#if PANCAKE_UNITASK
        public AnonymousSheetLifecycleEvent(
            Func<UniTask> initialize = null,
            Func<UniTask> onWillEnter = null,
            Action onDidEnter = null,
            Func<UniTask> onWillExit = null,
            Action onDidExit = null,
            Func<UniTask> onCleanup = null)

        {
            if (initialize != null) OnInitialize.Add(initialize);

            if (onWillEnter != null) OnWillEnter.Add(onWillEnter);

            OnDidEnter = onDidEnter;

            if (onWillExit != null) OnWillExit.Add(onWillExit);

            OnDidExit = onDidExit;

            if (onCleanup != null) OnCleanup.Add(onCleanup);
        }

        public List<Func<UniTask>> OnInitialize { get; } = new();
        public List<Func<UniTask>> OnWillEnter { get; } = new();
        public List<Func<UniTask>> OnWillExit { get; } = new();
        public List<Func<UniTask>> OnCleanup { get; } = new();


        UniTask ISheetLifecycleEvent.Initialize() { return UniTask.WhenAll(OnInitialize.Map(x => x.Invoke())); }

        UniTask ISheetLifecycleEvent.WillEnter() { return UniTask.WhenAll(OnWillEnter.Map(x => x.Invoke())); }

        UniTask ISheetLifecycleEvent.WillExit() { return UniTask.WhenAll(OnWillExit.Map(x => x.Invoke())); }

        UniTask ISheetLifecycleEvent.Cleanup() { return UniTask.WhenAll(OnCleanup.Map(x => x.Invoke())); }
#endif

        void ISheetLifecycleEvent.DidEnter() { OnDidEnter?.Invoke(); }

        void ISheetLifecycleEvent.DidExit() { OnDidExit?.Invoke(); }
    }
}