using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "audio_play_channel.asset", menuName = "Pancake/Sound/AudioPlayEvent")]
    public class AudioPlayEvent : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;
        private readonly List<EventListenerPlayAudio> _eventListeners = new List<EventListenerPlayAudio>();
        private readonly List<Object> _listenerObjects = new List<Object>();

        private Func<Audio, AudioConfig, Vector3, AudioHandle> _onRaised;

        public event Func<Audio, AudioConfig, Vector3, AudioHandle> OnRaised
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

        public AudioHandle Raise(Audio audio, AudioConfig config, Vector3 position)
        {
            var handle = AudioHandle.invalid;
            if (!Application.isPlaying) return handle;

            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this, debugLogEnabled);
            }

            var result = _onRaised?.Invoke(audio, config, position);
            if (result != null) handle = result.Value;
            return handle;
        }

        public void RegisterListener(EventListenerPlayAudio listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerPlayAudio listener)
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