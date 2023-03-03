using System;
using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_noParam.asset", menuName = "Pancake/Scriptable/Events/noParam")]
    public class ScriptableEventNoParam : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;
        private readonly List<EventListenerNoParam> _eventListeners = new List<EventListenerNoParam>();
        private readonly List<Object> _listenerObjects = new List<Object>();

        private Action _onRaised;

        public event Action OnRaised
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
                if (_listenerObjects.Contains(listener)) _listenerObjects.Remove(listener);
            }
        }

        public void Raise()
        {
            if (!Application.isPlaying) return;

            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this, debugLogEnabled);
            }

            _onRaised?.Invoke();
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
            allObjects.AddRange(_listenerObjects);
            return allObjects;
        }
    }
}