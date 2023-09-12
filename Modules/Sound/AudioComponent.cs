using System.Collections;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pancake.Sound
{
    /// <summary>
    /// Simple implementation of a MonoBehaviour that is able to request a sound being played by the <c>AudioManager</c>.
    /// It fires an event on an <c>AudioEventScriptable</c> which acts as a channel, that the <c>AudioManager</c> will pick up and play.
    /// </summary>
    [EditorIcon("csharp")]
    public sealed class AudioComponent : GameComponent
    {
        [Header("Sound definition")] [SerializeField, Label("Audio")]
        private Audio au;

        [SerializeField] private bool playOnStart;

        [FormerlySerializedAs("audioPlayChannel")] [Header("Configuration")] [SerializeField]
        private ScriptableEventAudio playAudioEvent;

        [SerializeField] private ScriptableEventAudioHandle stopAudioEvent;
        [SerializeField] private ScriptableEventAudioHandle pauseAudioEvent;
        [SerializeField] private ScriptableEventAudioHandle resumeAudioEvent;

        private AudioHandle _audioHandle = AudioHandle.invalid;

        private void Start()
        {
            if (playOnStart) StartCoroutine(IePlayDelayed());
        }

        protected override void OnDisabled()
        {
            playOnStart = false;
            Stop();
            base.OnDisabled();
        }

        private IEnumerator IePlayDelayed()
        {
            //The wait allows the AudioManager to be ready for play requests
            yield return new WaitForSeconds(0.5f);

            //This additional check prevents the AudioCue from playing if the object is disabled or the scene unloaded
            //This prevents playing a looping AudioCue which then would be never stopped
            if (playOnStart) Play();
        }

        public void Play() { _audioHandle = playAudioEvent.Raise(au); }

        public void Stop()
        {
            stopAudioEvent.Raise(_audioHandle);
            _audioHandle = AudioHandle.invalid;
        }

        public void Pause()
        {
            pauseAudioEvent.Raise(_audioHandle);
            _audioHandle = AudioHandle.invalid;
        }

        public void Resume()
        {
            resumeAudioEvent.Raise(_audioHandle);
            _audioHandle = AudioHandle.invalid;
        }
    }
}