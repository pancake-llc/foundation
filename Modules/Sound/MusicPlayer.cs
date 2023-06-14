using System.Collections;
using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("script_mono")]
    public sealed class MusicPlayer : GameComponent
    {
        [Header("Music definition")] [SerializeField] private Audio music;
        [SerializeField] private bool playOnEnable;

        [Header("Configuration")] [SerializeField] private AudioPlayEvent playMusicEvent;
        [SerializeField] private AudioHandleEvent stopMusicEvent;
        [SerializeField] private AudioConfig audioConfig;

        private AudioHandle _audioHandle = AudioHandle.invalid;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            if (playOnEnable) StartCoroutine(IePlayDelayed());
        }

        private void OnDestroy() { stopMusicEvent.Raise(_audioHandle); }

        private IEnumerator IePlayDelayed()
        {
            //The wait allows the AudioManager to be ready for play requests
            yield return new WaitForSeconds(0.5f);

            //This additional check prevents the AudioCue from playing if the object is disabled or the scene unloaded
            //This prevents playing a looping AudioCue which then would be never stopped
            if (playOnEnable) PlayMusic();
        }
        
        private void PlayMusic() { _audioHandle = playMusicEvent.Raise(music, audioConfig, Vector3.zero); }
    }
}