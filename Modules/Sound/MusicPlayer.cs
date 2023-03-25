using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    public class MusicPlayer : GameComponent
    {
        [SerializeField] private ScriptableEventNoParam onSceneReady;
        [SerializeField] private ScriptableEventPlayAudio playMusicEvent;
        [SerializeField] private Audio bgmMusic;
        [SerializeField] private AudioConfig audioConfig;

        [Header("Pause Menu Music")]
        [SerializeField] private Audio pauseMusic;
        [SerializeField] private ScriptableEventBool onPauseOpened; 

        private void OnEnable()
        {
            onPauseOpened.OnRaised += PlayPauseMusic;
            onSceneReady.OnRaised += PlayMusic;
        }

        private void OnDisable()
        {
            onPauseOpened.OnRaised -= PlayPauseMusic;
            onSceneReady.OnRaised -= PlayMusic;
        }

        private void PlayMusic()
        {
            playMusicEvent.Raise(bgmMusic, audioConfig, Vector3.zero);
        }

        private void PlayPauseMusic(bool open)
        {
            if (open) playMusicEvent.Raise(pauseMusic, audioConfig, Vector3.zero);
            else PlayMusic();
        }
    }

}