using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_noparam.asset", menuName = "Pancake/Scriptable/Events/no param")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventNoParam : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;

        private readonly List<EventListenerNoParam> _eventListeners = new List<EventListenerNoParam>();
        private readonly List<Object> _listenersObjects = new List<Object>();

        private Action _onRaised;

        public event Action OnRaised
        {
            add
            {
                _onRaised += value;

                var listener = value.Target as Object;
                if (listener != null && !_listenersObjects.Contains(listener))
                    _listenersObjects.Add(listener);
            }
            remove
            {
                _onRaised -= value;

                var listener = value.Target as Object;
                if (_listenersObjects.Contains(listener))
                    _listenersObjects.Remove(listener);
            }
        }

        public void Raise()
        {
            if (!Application.isPlaying)
                return;

            for (var i = _eventListeners.Count - 1; i >= 0; i--)
                _eventListeners[i].OnEventRaised(this, debugLogEnabled);

            _onRaised?.Invoke();

#if UNITY_EDITOR
            //As this uses reflection, I only allow it to be called in Editor.
            //If you want to display debug in builds, delete the #if UNITY_EDITOR
            if (debugLogEnabled)
                Debug();
#endif
        }

        public void RegisterListener(EventListenerNoParam listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerNoParam listener)
        {
            if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
        }

        public List<Object> GetAllObjects()
        {
            var allObjects = new List<Object>(_eventListeners);
            allObjects.AddRange(_listenersObjects);
            return allObjects;
        }

        private void Debug()
        {
            if (_onRaised == null)
                return;
            var delegates = _onRaised.GetInvocationList();
            foreach (var del in delegates)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#f75369>[Event] </color>");
                sb.Append(name);
                sb.Append(" => ");
                sb.Append(del.GetMethodInfo().Name);
                sb.Append("()");
                var monoBehaviour = del.Target as MonoBehaviour;
                UnityEngine.Debug.Log(sb.ToString(), monoBehaviour?.gameObject);
            }
        }

        public override void Reset() { debugLogEnabled = false; }
    }
}