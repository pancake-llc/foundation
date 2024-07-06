using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("icon_blue_audiosource")]
    public class SoundSource : MonoBehaviour
    {
        public enum PositionMode
        {
            Global,
            FollowGameObject,
            StayHere,
        }

        [SerializeField] private bool playOnStart = true;
        [SerializeField] private SoundId sound;
        [SerializeField] private PositionMode positionMode = PositionMode.Global;

        private IAudioPlayer _currentPlayer;

        public void Play() => _currentPlayer = AudioStatic.Play(sound);
        public void Play(Transform followTarget) => _currentPlayer = AudioStatic.Play(sound, followTarget);
        public void Play(Vector3 positon) => _currentPlayer = AudioStatic.Play(sound, positon);
        public void Stop() => _currentPlayer?.Stop();
        public void Stop(float fadeTime) => _currentPlayer?.Stop(fadeTime);
        public void SetVolume(float vol) => _currentPlayer?.SetVolume(vol);
        public void SetVolume(float vol, float fadeTime) => _currentPlayer?.SetVolume(vol, fadeTime);
        public void SetPitch(float pitch) => _currentPlayer?.SetPitch(pitch);
        public void SetPitch(float pitch, float fadeTime) => _currentPlayer?.SetPitch(pitch, fadeTime);
        
        private void Start()
        {
            if (!playOnStart) return;

            switch (positionMode)
            {
                case PositionMode.Global:
                    Play();
                    break;
                case PositionMode.FollowGameObject:
                    Play(transform);
                    break;
                case PositionMode.StayHere:
                    Play(transform.position);
                    break;
            }
        }
    }
}