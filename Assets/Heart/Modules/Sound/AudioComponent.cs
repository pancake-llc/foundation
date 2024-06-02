using System.Collections;
#if PANCAKE_ALCHEMY
using Alchemy.Inspector;
#endif
using UnityEngine;
using VContainer;

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

        private AudioHandle _handle;
        [Inject] private readonly AudioManager _audioManager;

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

            //This additional check prevents the Audio from playing if the object is disabled or the scene unloaded
            //This prevents playing a looping Audio which then would be never stopped
            if (playOnStart) Play();
        }

        public void Play() { _handle = isSfx ? _audioManager.PlaySfx(au) : _audioManager.PlayMusic(au); }

        public void Stop()
        {
            if (isSfx) _audioManager.StopSfx(_handle);
            else _audioManager.StopMusic(_handle);
        }

        public void Pause()
        {
            if (isSfx) _audioManager.PauseSfx(_handle);
            else _audioManager.PauseMusic(_handle);
        }

        public void Resume()
        {
            if (isSfx) _audioManager.ResumeSfx(_handle);
            else _audioManager.ResumeMusic(_handle);
        }
    }
}