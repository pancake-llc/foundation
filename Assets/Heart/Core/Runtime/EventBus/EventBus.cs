using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Pancake
{
    public static class EventBusUtility
    {
        public static IReadOnlyList<Type> EventTypes { get; private set; }
        public static IReadOnlyList<Type> StaticEventBusesTypes { get; private set; }

#if UNITY_EDITOR
        public static UnityEditor.PlayModeStateChange PlayModeState { get; private set; }

        [UnityEditor.InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= HandleEditorStateChange;
            UnityEditor.EditorApplication.playModeStateChanged += HandleEditorStateChange;
        }

        private static void HandleEditorStateChange(UnityEditor.PlayModeStateChange state)
        {
            PlayModeState = state;

            if (PlayModeState == UnityEditor.PlayModeStateChange.EnteredEditMode) ClearAllBuses();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[] assemblyCSharp = null;
            Type[] assemblyCSharpFirstpass = null;

            for (var i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == "Assembly-CSharp") assemblyCSharp = assemblies[i].GetTypes();
                else if (assemblies[i].GetName().Name == "Assembly-CSharp-firstpass") assemblyCSharpFirstpass = assemblies[i].GetTypes();

                if (assemblyCSharp != null && assemblyCSharpFirstpass != null) break;
            }

            var eventTypes = new List<Type>();

            if (assemblyCSharp != null)
            {
                for (var i = 0; i < assemblyCSharp.Length; i++)
                {
                    var type = assemblyCSharp[i];
                    if (typeof(IEvent) != type && typeof(IEvent).IsAssignableFrom(type))
                    {
                        eventTypes.Add(type);
                    }
                }
            }

            if (assemblyCSharpFirstpass != null)
            {
                for (var i = 0; i < assemblyCSharpFirstpass.Length; i++)
                {
                    var type = assemblyCSharpFirstpass[i];
                    if (typeof(IEvent) != type && typeof(IEvent).IsAssignableFrom(type))
                    {
                        eventTypes.Add(type);
                    }
                }
            }

            EventTypes = eventTypes;

            var staticEventBusesTypes = new List<Type>();
            var typedef = typeof(EventBus<>);
            for (var i = 0; i < EventTypes.Count; i++)
            {
                var type = EventTypes[i];
                var gentype = typedef.MakeGenericType(type);
                staticEventBusesTypes.Add(gentype);
            }

            StaticEventBusesTypes = staticEventBusesTypes;
        }

        public static void ClearAllBuses()
        {
            for (var i = 0; i < StaticEventBusesTypes.Count; i++)
            {
                var type = EventTypes[i];
                var clearMethod = type.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
                if (clearMethod != null) clearMethod.Invoke(null, null);
            }
        }
    }

    public static class EventBus<T> where T : struct, IEvent
    {
        private static EventBinding<T>[] bindings = new EventBinding<T>[64];
        private static readonly List<Callback> Callbacks = new List<Callback>();
        private static int count;
        private static readonly Stack<Awaiter> AwaiterPool = new Stack<Awaiter>();

        public class Awaiter : EventBinding<T>
        {
            private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();

            public Awaiter()
                : base((Action) null)
            {
                ((IEventBindingInternal<T>) this).OnEvent = OnEvent;
            }

            private void OnEvent(T ev)
            {
                _tcs.TrySetResult(ev);
                EventBus<T>.ReleaseAwaiter(this);
            }

            public Task<T> Async() => _tcs.Task;
        }

        private struct Callback
        {
            public Action onEventNoArg;
            public Action<T> onEvent;
        }

        private static void Clear()
        {
            Array.Clear(bindings, 0, count);
            Callbacks.Clear();
            count = 0;
        }

        public static void Register(EventBinding<T> binding)
        {
            if (binding.Registered) return;

            if (bindings.Length <= count) Array.Resize(ref bindings, bindings.Length * 2);

            binding.InternalIndex = count;
            bindings[count] = binding;

            count++;
        }

        public static void AddCallback(Action callback)
        {
            if (callback == null) return;
            Callbacks.Add(new Callback() {onEventNoArg = callback});
        }

        public static void AddCallback(Action<T> callback)
        {
            if (callback == null) return;
            Callbacks.Add(new Callback() {onEvent = callback});
        }

        public static void Unregister(EventBinding<T> binding)
        {
#if UNITY_EDITOR
            if (EventBusUtility.PlayModeState == UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
#endif
            int index = binding.InternalIndex;

            // binding invalid
            if (index == -1 || index >= count || bindings[index] != binding) return;

            if (index == count - 1)
            {
                bindings[count - 1] = null;
                binding.InternalIndex = -1;
                count--;
                return;
            }

            int lastIndex = count - 1;
            var last = bindings[lastIndex];

            bindings[index] = last;
            bindings[lastIndex] = null;

            if (last != null) last.InternalIndex = index;
            binding.InternalIndex = -1;
            count--;
        }

        public static void Raise(T ev = default)
        {
#if UNITY_EDITOR
            if (EventBusUtility.PlayModeState == UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
#endif
            for (var i = 0; i < count; i++)
            {
                IEventBindingInternal<T> internalBind = bindings[i];
                internalBind.OnEvent?.Invoke(ev);
                internalBind.OnEventArgs?.Invoke();
            }

            for (var i = 0; i < Callbacks.Count; i++)
            {
                Callbacks[i].onEvent?.Invoke(ev);
                Callbacks[i].onEventNoArg?.Invoke();
            }

            Callbacks.Clear();
        }

        public static string GetDebugInfoString() { return "Bindings: " + count + " BufferSize: " + bindings.Length + "\n" + "Callbacks: " + Callbacks.Count; }

        public static Awaiter GetAwaiter() { return AwaiterPool.Count > 0 ? AwaiterPool.Pop() : new Awaiter(); }

        private static void ReleaseAwaiter(Awaiter awaiter) { AwaiterPool.Push(awaiter); }
    }
}