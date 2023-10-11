using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [Serializable]
    [EditorIcon("scriptable_event")]
    public abstract class ScriptableEventFunc<TResult> : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;

        private readonly List<EventListenerFunc<TResult>> _eventListeners = new List<EventListenerFunc<TResult>>();
        private readonly List<Object> _listenerObjects = new List<Object>();
        private Func<TResult> _onRaised;

        public event Func<TResult> OnRaised
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

        public TResult Raise()
        {
            TResult result = default;
            if (!Application.isPlaying) return result;

            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this, debugLogEnabled);
            }

            if (_onRaised != null) result = _onRaised.Invoke();

            // As this uses reflection, I only allow it to be called in Editor. So you need remember turnoff debug when build
            if (debugLogEnabled) Debug();

            return result;
        }

        public void RegisterListener(EventListenerFunc<TResult> listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerFunc<TResult> listener)
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