using System;

namespace Pancake.Sound
{
    public class MusicPlayer : AudioPlayerDecorator, IMusicPlayer
    {
        public static AudioPlayer currentPlayer;

        private EAudioTransition _transition;
        private EAudioStopMode _stopMode;
        private float _overrideFade = AudioPlayer.USE_ENTITY_SETTING;

        public bool IsPlayingVirtually => IsActive && Instance?.MixerDecibelVolume <= AudioConstant.MIN_DECIBEL_VOLUME;

        public MusicPlayer(AudioPlayer audioPlayer)
            : base(audioPlayer)
        {
        }

        protected override void Recycle(AudioPlayer player)
        {
            base.Recycle(player);
            _transition = default;
            _stopMode = default;
            _overrideFade = AudioPlayer.USE_ENTITY_SETTING;
        }

        IMusicPlayer IMusicPlayer.SetTransition(EAudioTransition transition, EAudioStopMode stopMode, float overrideFade)
        {
            _transition = transition;
            _stopMode = stopMode;
            _overrideFade = overrideFade;
            return this;
        }

        public PlaybackPreference Transition(PlaybackPreference pref)
        {
            if (currentPlayer != null)
            {
                pref.SetFadeTime(_transition, _overrideFade);
                switch (_transition)
                {
                    case EAudioTransition.Immediate:
                    case EAudioTransition.OnlyFadeIn:
                    case EAudioTransition.CrossFade:
                        StopCurrentMusic();
                        break;
                    case EAudioTransition.Default:
                    case EAudioTransition.OnlyFadeOut:

                        var waiter = pref.CreateWaiter();
                        StopCurrentMusic(waiter.Finish);
                        break;
                }
            }

            currentPlayer = Instance;
            return pref;
        }

        private void StopCurrentMusic(Action onFinished = null)
        {
            bool noFadeOut = _transition is EAudioTransition.Immediate or EAudioTransition.OnlyFadeIn;
            float fadeOut = noFadeOut ? 0f : _overrideFade;
            currentPlayer.Stop(fadeOut, _stopMode, onFinished);
        }
    }
}