using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pancake.Linq;

namespace Pancake.UI
{
    public sealed class AnonymousSheetLifecycleEvent : ISheetLifecycleEvent
    {
        public event Action OnDidEnter;
        public event Action OnDidExit;

        public AnonymousSheetLifecycleEvent(
            Func<Task> initialize = null,
            Func<Task> onWillEnter = null,
            Action onDidEnter = null,
            Func<Task> onWillExit = null,
            Action onDidExit = null,
            Func<Task> onCleanup = null)

        {
            if (initialize != null) OnInitialize.Add(initialize);

            if (onWillEnter != null) OnWillEnter.Add(onWillEnter);

            OnDidEnter = onDidEnter;

            if (onWillExit != null) OnWillExit.Add(onWillExit);

            OnDidExit = onDidExit;

            if (onCleanup != null) OnCleanup.Add(onCleanup);
        }

        public List<Func<Task>> OnInitialize { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillEnter { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnWillExit { get; } = new List<Func<Task>>();
        public List<Func<Task>> OnCleanup { get; } = new List<Func<Task>>();


        Task ISheetLifecycleEvent.Initialize() { return Task.WhenAll(OnInitialize.Map(x => x.Invoke())); }


        Task ISheetLifecycleEvent.WillEnter() { return Task.WhenAll(OnWillEnter.Map(x => x.Invoke())); }


        void ISheetLifecycleEvent.DidEnter() { OnDidEnter?.Invoke(); }


        Task ISheetLifecycleEvent.WillExit() { return Task.WhenAll(OnWillExit.Map(x => x.Invoke())); }


        void ISheetLifecycleEvent.DidExit() { OnDidExit?.Invoke(); }


        Task ISheetLifecycleEvent.Cleanup() { return Task.WhenAll(OnCleanup.Map(x => x.Invoke())); }
    }
}