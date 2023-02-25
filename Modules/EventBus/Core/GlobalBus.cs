using System;
using System.Linq;
using System.Reflection;
using Pancake.EventBus.Utils;
using UnityEngine;

namespace Pancake.EventBus
{
    /// <summary>
    /// EventBus singleton
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed partial class GlobalBus : MonoBehaviour, IEventBus
    {
        private static GlobalBus s_Instance;
        public static GlobalBus  Instance
        {
            get
            {
#if !UNITY_EVENT_BUS_DISABLE_AUTO_INITIALIZATION
                if (s_Instance.IsNull())
                    Create(false, false);
#endif

                return s_Instance;
            }

            private set
            {
                if (s_Instance == value)
                    return;

                s_Instance = value;

                // instance discarded
                if (s_Instance.IsNull())
                    return;
            }
        }

        public const object k_DefaultEventData = null;
        public const int    k_DefaultPriority  = 0;
        public const string k_DefaultName      = "";

        private IEventBusImpl m_Impl;
        internal IEventBusImpl Impl => m_Impl;

        public bool InitOnAwake;
        public bool CollectClasses;
        public bool CollectFunctions;

        // =======================================================================
        private abstract class ListenerActionBase<T> : IListener<T>, ISubscriberOptions
        {
            protected string            m_Name;
            protected int               m_Proprity;

            public string               Name => m_Name;
            public int                  Priority => m_Proprity;

            // =======================================================================
            public abstract void React(in T e);
            
            protected ListenerActionBase(string name, int proprity)
            {
                m_Name     = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;
                m_Proprity = proprity;
            }
        }

        private class ListenerStaticFunction<T> : ListenerActionBase<T>
        {
            private ProcessDelagate    m_Action;
            
            // =======================================================================
            private delegate void ProcessDelagate(T e);
            
            // =======================================================================
            public override void React(in T e)
            {
                // if key matches invoke action
                m_Action(e);
            }

            public ListenerStaticFunction(string name, MethodInfo method, int proprity)
                : base(name, proprity)
            {
                // proceed call
                m_Action = (ProcessDelagate)Delegate.CreateDelegate(typeof(ProcessDelagate), method);

                // set defaults from method info
                if (string.IsNullOrEmpty(name))
                    m_Name = method.Name;
            }
        }
        
        // =======================================================================
        private void Awake()
        {
            if (InitOnAwake)
            {
                Init(new EventBusImpl());
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Init(IEventBusImpl impl)
        {
            if (s_Instance != null && s_Instance != this)
                throw new NotSupportedException("Can't initialize EventSystem singleton twice.");

            if (impl == null)
                throw new ArgumentNullException(nameof(impl));

            // set implementation
            m_Impl = impl;

            // set instance
            Instance = this;
            
            // parse assembly for listeners
            _collectAssemblyListeners();
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
                Instance = null;

            if (m_Impl != null)
            {
                m_Impl.Dispose();
                m_Impl = null;
            }
        }

        // =======================================================================
        void IEventBus.Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
        {
            Send(in e, in invoker);
        }

        void IEventBus.Subscribe(ISubscriber sub)
        {
            Subscribe(sub);
        }

        void IEventBus.UnSubscribe(ISubscriber sub)
        {
            UnSubscribe(sub);
        }

        // =======================================================================
        /// <summary> Create and initialize EventSystem singleton game object, if singleton already created nothing will happen </summary>
        public static void Create(bool collectClasses, bool collectFunctions)
        {
            if (s_Instance != null)
                return;

            var go = new GameObject(nameof(GlobalBus));
            DontDestroyOnLoad(go);

            var es = go.AddComponent<GlobalBus>();
            es.CollectClasses = collectClasses;
            es.CollectFunctions = collectFunctions;

            es.Init(new EventBusImpl());
        }

        public static void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
            Instance.m_Impl.Send(in e, in invoker);
        }
        
        public static void Send<TEvent>(in TEvent e, in Func<ISubscriber, bool> check)
        {
            Instance.m_Impl.Send(in e, new Extensions.DefaultInvokerConditional() { m_Filter = check });
        }
        
        public static void Send<TEvent>(in TEvent e)
        { 
            Instance.Send(in e);
        }

        public static void Subscribe(ISubscriber sub)
	    {
            Instance.m_Impl.Subscribe(sub);
        }

        public static void UnSubscribe(ISubscriber sub)
	    {
#if UNITY_EDITOR
            if (s_Instance == null)
                return;
#endif
            Instance.m_Impl.UnSubscribe(sub);
	    }
        
        // =======================================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void _domainReloadCapability()
        {
            s_Instance = null;
        }
        
        private void _collectAssemblyListeners()
        { 
            // not tested with AOT, questionable usefulness, confusional behavior
            if (CollectClasses || CollectFunctions)
            {
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(n => n.GetTypes()).ToArray();

                // create listener instances
                if (CollectClasses)
                {
                    foreach (var type in types)
                    {
                        var attribure = type.GetCustomAttribute<ListenerAttribute>();
                        // not null & active
                        if (attribure != null && attribure.Active)
                        {
                            // must be creatable class
                            if (type.IsAbstract || type.IsClass == false || type.IsGenericType)
                                continue;

                            // must implement event listener interface
                            if (typeof(ISubscriber).IsAssignableFrom(type) == false)
                                continue;

                            // create & register listener
                            try
                            {
                                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                                {
                                    // listener is monobehaviour type
                                    var el = new GameObject(attribure.Name, type).GetComponent(type) as MonoBehaviour;
                                    el.transform.SetParent(transform);

                                    Subscribe(el as ISubscriber);
                                }
                                else
                                {
                                    // listener is class
                                    var el = (ISubscriber)Activator.CreateInstance(type);
                                    Subscribe(el);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e);
                            }
                        }
                    }
                }

                // create static function listeners
                if (CollectFunctions)
                {
                    foreach (var type in types)
                    {
                        // check all static methods
                        foreach (var methodInfo in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                        {
                            try
                            {
                                // must be static
                                if (methodInfo.IsStatic == false)
                                    continue;

                                // not generic
                                if (methodInfo.IsGenericMethod)
                                    continue;

                                var attribure = methodInfo.GetCustomAttribute<ListenerAttribute>();
                                // not null & active attribute
                                if (attribure == null || attribure.Active == false)
                                    continue;

                                var args = methodInfo.GetParameters();

                                // must have input parameter
                                if (args.Length != 1)
                                    continue;

                                // create & register listener
                                var keyType = args[0].ParameterType;
                                var el = Activator.CreateInstance(
                                    typeof(ListenerStaticFunction<>).MakeGenericType(keyType),
                                    attribure.Name, methodInfo, attribure.Order) as ISubscriber;
                                Subscribe(el);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e);
                            }
                        }
                    }
                }
            }
        }

        // =======================================================================
        [ContextMenu("Log subscribers")]
        public void LogSubscribers()
        {
            foreach (var subscriber in m_Impl.GetSubscribers())
                Debug.Log(subscriber, subscriber.Target as MonoBehaviour);
        }
    }
}