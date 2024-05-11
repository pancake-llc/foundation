using System.Collections;
#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
#endif
using UnityEngine;

namespace Pancake.Sound
{
    /// <summary>
    /// Simple implementation of a MonoBehaviour that is able to request a sound being played by the <c>AudioManager</c>.
    /// It fires an event on an <c>AudioEventScriptable</c> which acts as a channel, that the <c>AudioManager</c> will pick up and play.
    /// </summary>
    [EditorIcon("icon_default")]
    public sealed class AudioComponent : GameComponent
    {
        [Header("Sound definition")]
#if PANCAKE_ALCHEMY
        [LabelText("Audio")]
#endif
        [SerializeField]
        private Audio au;

        [SerializeField] private bool playOnStart;
        [SerializeField] private bool isSfx;

        private AudioHandle _audioHandle = AudioHandle.invalid;

        private void Start()
        {
            if (playOnStart) StartCoroutine(IePlayDelayed());
        }

        private void OnDisable()
        {
            playOnStart = false;
            Stop();
        }

        private IEnumerator IePlayDelayed()
        {
            //The wait allows the AudioManager to be ready for play requests
            yield return new WaitForSeconds(0.5f);

            //This additional check prevents the AudioCue from playing if the object is disabled or the scene unloaded
            //This prevents playing a looping AudioCue which then would be never stopped
            if (playOnStart) Play();
        }

        public void Play() { _audioHandle = isSfx ? au.PlaySfx() : au.PlayMusic(); }

        public void Stop()
        {
            if (isSfx) _audioHandle.StopSfx();
            else _audioHandle.StopMusic();
            _audioHandle = AudioHandle.invalid;
        }

        public void Pause()
        {
            if (isSfx) _audioHandle.PauseSfx();
            else _audioHandle.PauseMusic();
            _audioHandle = AudioHandle.invalid;
        }

        public void Resume()
        {
            if (isSfx) _audioHandle.ResumeSfx();
            else _audioHandle.ResumeMusic();
            _audioHandle = AudioHandle.invalid;
        }
    }
}