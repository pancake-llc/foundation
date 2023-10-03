using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pancake.Linq;

namespace Pancake.UI
{
    public sealed class AnonymousPopupLifecycleEvent : IPopupLifecycleEvent
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

        public AnonymousPopupLifecycleEvent(
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
        
        Task IPopupLifecycleEvent.Initialize() { return Task.WhenAll(OnInitialize.Map(x => x.Invoke())); }

        Task IPopupLifecycleEvent.WillPushEnter() { return Task.WhenAll(OnWillPushEnter.Map(x => x.Invoke())); }
        
        void IPopupLifecycleEvent.DidPushEnter() { OnDidPushEnter?.Invoke(); }

        Task IPopupLifecycleEvent.WillPushExit() { return Task.WhenAll(OnWillPushExit.Map(x => x.Invoke())); }

        void IPopupLifecycleEvent.DidPushExit() { OnDidPushExit?.Invoke(); }

        Task IPopupLifecycleEvent.WillPopEnter() { return Task.WhenAll(OnWillPopEnter.Map(x => x.Invoke())); }
        
        void IPopupLifecycleEvent.DidPopEnter() { OnDidPopEnter?.Invoke(); }

        Task IPopupLifecycleEvent.WillPopExit() { return Task.WhenAll(OnWillPopExit.Map(x => x.Invoke())); }
        
        void IPopupLifecycleEvent.DidPopExit() { OnDidPopExit?.Invoke(); }

        Task IPopupLifecycleEvent.Cleanup() { return Task.WhenAll(OnCleanup.Map(x => x.Invoke())); }
    }
}