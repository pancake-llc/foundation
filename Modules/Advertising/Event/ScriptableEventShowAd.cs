using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Monetization
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "ad_show_chanel.asset", menuName = "Pancake/AD/Event/Show Ad")]
    public class ScriptableEventShowAd : ScriptableEventBase, IDrawObjectsInInspector
    {
        private readonly List<EventListenerShowAd> _eventListeners = new List<EventListenerShowAd>();
        private readonly List<Object> _listenerObjects = new List<Object>();

        private Action<AdUnitVariable> _onRaised;

        public event Action<AdUnitVariable> OnRaised
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

        public void Raise(AdUnitVariable unit)
        {
            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this);
            }

            _onRaised?.Invoke(unit);
        }

        public void RegisterListener(EventListenerShowAd listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerShowAd listener)
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