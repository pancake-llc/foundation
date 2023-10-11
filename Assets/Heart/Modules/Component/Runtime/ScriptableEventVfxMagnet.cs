using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Component
{
    [Serializable]
    [CreateAssetMenu(fileName = "scriptable_event_vfx_magnet.asset", menuName = "Pancake/Misc/Events/Vfx Magnet Event")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventVfxMagnet : ScriptableEventBase, IDrawObjectsInInspector
    {
        private readonly List<EventListenerVfxMagnet> _eventListeners = new List<EventListenerVfxMagnet>();
        private readonly List<Object> _listenerObjects = new List<Object>();
        
        private Action<Vector2, int> _onRaised;

        /// <summary> Event raised when the event is raised. </summary>
        public event Action<Vector2, int> OnRaised
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

        /// <summary> Raise the event </summary>
        public void Raise(Vector2 position, int value)
        {
            if (!Application.isPlaying) return;
            
            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this);
            }
            
            _onRaised?.Invoke(position, value);
        }

        public void RegisterListener(EventListenerVfxMagnet listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerVfxMagnet listener)
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