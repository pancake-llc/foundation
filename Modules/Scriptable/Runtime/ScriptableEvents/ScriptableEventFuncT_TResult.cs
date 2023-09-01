using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Pancake.Scriptable
{
    using UnityEngine;

    [Serializable]
    [EditorIcon("scriptable_event")]
    // ReSharper disable once InconsistentNaming
    public abstract class ScriptableEventFuncT_TResult<T, TResult> : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;
        [SerializeField] protected T debugValue = default;

        private readonly List<EventListenerFuncT_TResult<T, TResult>> _eventListeners = new List<EventListenerFuncT_TResult<T, TResult>>();
        private readonly List<Object> _listenerObjects = new List<Object>();
        private Func<T, TResult> _onRaised;

        public override Type GetGenericType => typeof(T);

        public event Func<T, TResult> OnRaised
        {
            add
            {
                _onRaised += value;
                var listener = value.Target as Object;
                if (listener != null && !_listenerObjects.Contains(listener)) _listenerObjects.Add(listener);
            }
            remove
            {
                _onRaised -= value;

                var listener = value.Target as Object;
                if (listener != null && _listenerObjects.Contains(listener)) _listenerObjects.Remove(listener);
            }
        }

        public TResult Raise(T param)
        {
            TResult result = default;
            if (!Application.isPlaying) return result;

            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this, param, debugLogEnabled);
            }

            if (_onRaised != null) result = _onRaised.Invoke(param);

            // As this uses reflection, I only allow it to be called in Editor. So you need remember turnoff debug when build
            if (debugLogEnabled) Debug();

            return result;
        }

        public void RegisterListener(EventListenerFuncT_TResult<T, TResult> listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerFuncT_TResult<T, TResult> listener)
        {
            if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
        }

        public List<Object> GetAllObjects()
        {
            var allObjects = new List<Object>(_eventListeners);
            allObjects.AddRange(_listenerObjects);
            return allObjects;
        }

        private void Debug()
        {
            var delegates = _onRaised.GetInvocationList();
            foreach (var del in delegates)
            {
                var sb = new StringBuilder();
                sb.Append($"<color=#f75369>[Event] </color>");
                sb.Append(name);
                sb.Append(" => ");
                sb.Append(del.GetMethodInfo().Name);
                sb.Append("()");
                var monoBehaviour = del.Target as MonoBehaviour;
                if (monoBehaviour != null) UnityEngine.Debug.Log(sb.ToString(), monoBehaviour.gameObject);
                else UnityEngine.Debug.Log(sb.ToString());
            }
        }
    }
}