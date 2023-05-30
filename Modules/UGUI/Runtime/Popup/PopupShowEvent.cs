using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.UI
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "popup_display_chanel.asset", menuName = "Pancake/UI/PopupShowEvent")]
    public class PopupShowEvent : ScriptableEventBase, IDrawObjectsInInspector
    {
        private readonly List<EventListenerShowPopup> _eventListeners = new List<EventListenerShowPopup>();
        private readonly List<Object> _listenerObjects = new List<Object>();

        private Action<UIPopup, Transform, Action<UIPopup>> _onRaised;

        public event Action<UIPopup, Transform, Action<UIPopup>> OnRaised
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

        public void Raise(UIPopup prefab, Transform parent, Action<UIPopup> callback)
        {
            if (!Application.isPlaying) return;
            
            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this);
            }

            _onRaised?.Invoke(prefab, parent, callback);
        }

        public void RegisterListener(EventListenerShowPopup listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerShowPopup listener)
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