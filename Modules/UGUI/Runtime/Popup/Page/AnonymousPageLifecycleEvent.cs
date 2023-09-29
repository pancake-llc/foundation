using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pancake.Linq;

namespace Pancake.UI
{
    public class AnonymousPageLifecycleEvent : IPageLifecycleEvent
    {
        public event Action OnDidPushEnter;
        public event Action OnDidPushExit;
        public event Action OnDidPopEnter;
        public event Action OnDidPopExit;

        public List<Func<Task>> OnInitialize { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillPushEnter { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillPushExit { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillPopEnter { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillPopExit { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnCleanup { get; } = new List<Func<Task>>();

        public AnonymousPageLifecycleEvent(
            Func<Task> initialize = null,
            Func<Task> onWillPushEnter = null,
            Action onDidPushEnter = null,
            Func<Task> onWillPushExit = null,
            Action onDidPushExit = null,
            Func<Task> onWillPopEnter = null,
            Action onDidPopEnter = null,
            Func<Task> onWillPopExit = null,
            Action onDidPopExit = null,
            Func<Task> onCleanup = null)

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

        Task IPageLifecycleEvent.Initialize() { return Task.WhenAll(OnInitialize.Map(x => x.Invoke())); }

        Task IPageLifecycleEvent.WillPushEnter() { return Task.WhenAll(OnWillPushEnter.Map(x => x.Invoke())); }

        void IPageLifecycleEvent.DidPushEnter() { OnDidPushEnter?.Invoke(); }

        Task IPageLifecycleEvent.WillPushExit() { return Task.WhenAll(OnWillPushExit.Map(x => x.Invoke())); }

        void IPageLifecycleEvent.DidPushExit() { OnDidPushExit?.Invoke(); }

        Task IPageLifecycleEvent.WillPopEnter() { return Task.WhenAll(OnWillPopEnter.Map(x => x.Invoke())); }

        void IPageLifecycleEvent.DidPopEnter() { OnDidPopEnter?.Invoke(); }

        Task IPageLifecycleEvent.WillPopExit() { return Task.WhenAll(OnWillPopExit.Map(x => x.Invoke())); }

        void IPageLifecycleEvent.DidPopExit() { OnDidPopExit?.Invoke(); }

        Task IPageLifecycleEvent.Cleanup() { return Task.WhenAll(OnCleanup.Map(x => x.Invoke())); }
    }
}