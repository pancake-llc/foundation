#if PANCAKE_IAP
using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.IAP
{
    [Serializable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "iap_noparam_chanel.asset", menuName = "Pancake/IAP/No Parameters Event")]
    public class ScriptableEventIAPNoParam : ScriptableEventBase, IDrawObjectsInInspector
    {
        private readonly List<EventListenerIAPNoParam> _eventListeners = new List<EventListenerIAPNoParam>();
        private readonly List<Object> _listenersObjects = new List<Object>();

        public override Type GetGenericType => typeof(ScriptableEventIAPNoParam);

        private Action _onRaised = null;

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
            if (!Application.isPlaying) return;

            for (var i = _eventListeners.Count - 1; i >= 0; i--)
                _eventListeners[i].OnEventRaised(this);

            _onRaised?.Invoke();
        }

        public void RegisterListener(EventListenerIAPNoParam listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerIAPNoParam listener)
        {
            if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
        }

        public List<Object> GetAllObjects()
        {
            var allObjects = new List<Object>(_eventListeners);
            allObjects.AddRange(_listenersObjects);
            return allObjects;
        }
    }
}
#endif