using System.Collections;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Sound
{
    /// <summary>
    /// Simple implementation of a MonoBehaviour that is able to request a sound being played by the <c>AudioManager</c>.
    /// It fires an event on an <c>AudioEventScriptable</c> which acts as a channel, that the <c>AudioManager</c> will pick up and play.
    /// </summary>
    [EditorIcon("script_mono")]
    public sealed class AudioComponent : GameComponent
    {
        [Header("Sound definition")] [SerializeField, Label("Audio")] private Audio au;
        [SerializeField] private bool playOnStart;

        [Header("Configuration")] [SerializeField] private AudioPlayEvent audioPlayChannel;
        [SerializeField] private AudioHandleEvent audioStopChannel;
        [SerializeField] private AudioHandleEvent audioFinishChannel;
        [SerializeField] private AudioConfig audioConfig;

        private AudioHandle _audioHandle = AudioHandle.invalid;

        private void Start()
        {
            if (playOnStart) StartCoroutine(IePlayDelayed());
        }

        protected override void OnDisabled()
        {
            playOnStart = false;
            StopAudio();
            base.OnDisabled();
        }

        private IEnumerator IePlayDelayed()
        {
            //The wait allows the AudioManager to be ready for play requests
            yield return new WaitForSeconds(0.5f);

            //This additional check prevents the AudioCue from playing if the object is disabled or the scene unloaded
            //This prevents playing a looping AudioCue which then would be never stopped
            if (playOnStart) PlayAudio();
        }

        public void PlayAudio() { _audioHandle = audioPlayChannel.Raise(au, audioConfig, transform.position); }

        public void StopAudio()
        {
            if (_audioHandle == AudioHandle.invalid) return;
            if (!audioStopChannel.Raise(_audioHandle)) _audioHandle = AudioHandle.invalid;
        }

        public void FinishAudio()
        {
            if (_audioHandle == AudioHandle.invalid) return;
            if (!audioFinishChannel.Raise(_audioHandle)) _audioHandle = AudioHandle.invalid;
        }
    }
}