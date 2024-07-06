using LitMotion;
using UnityEngine;
using static Pancake.Sound.AudioPlayer;

namespace Pancake.Sound
{
    public struct PlaybackPreference
    {
        public readonly IAudioEntity entity;
        public readonly Vector3 position;
        public readonly Transform followTarget;
        public Ease FadeInEase => entity.SeamlessLoop ? AudioSettings.SeamlessFadeInEase : AudioSettings.DefaultFadeInEase;
        public Ease FadeOutEase => entity.SeamlessLoop ? AudioSettings.SeamlessFadeOutEase : AudioSettings.DefaultFadeOutEase;

        public float FadeIn { get; private set; }
        public float FadeOut { get; private set; }
        public Waiter PlayerWaiter { get; private set; }

        public PlaybackPreference(IAudioEntity entity, Vector3 position)
            : this(entity)
        {
            this.position = position;
        }

        public PlaybackPreference(IAudioEntity entity, Transform followTarget)
            : this(entity)
        {
            this.followTarget = followTarget;
        }

        public PlaybackPreference(IAudioEntity entity)
        {
            this.entity = entity;
            FadeIn = USE_ENTITY_SETTING;
            FadeOut = USE_ENTITY_SETTING;
            position = Vector3.negativeInfinity;
            followTarget = null;
            PlayerWaiter = null;
        }

        public void SetFadeTime(EAudioTransition transition, float fadeTime)
        {
            switch (transition)
            {
                case EAudioTransition.Immediate:
                    FadeIn = 0f;
                    FadeOut = 0f;
                    break;
                case EAudioTransition.OnlyFadeIn:
                    FadeIn = fadeTime;
                    FadeOut = 0f;
                    break;
                case EAudioTransition.OnlyFadeOut:
                    FadeIn = 0f;
                    FadeOut = fadeTime;
                    break;
                case EAudioTransition.Default:
                case EAudioTransition.CrossFade:
                    FadeIn = fadeTime;
                    FadeOut = fadeTime;
                    break;
            }
        }

        public void ApplySeamlessFade()
        {
            FadeIn = entity.TransitionTime;
            FadeOut = entity.TransitionTime;
        }

        public Waiter CreateWaiter()
        {
            PlayerWaiter ??= new Waiter();
            return PlayerWaiter;
        }

        public void DisposeWaiter() { PlayerWaiter = null; }

        public bool HasPosition(out Vector3 position)
        {
            position = this.position;
            return !this.position.Equals(Vector3.negativeInfinity);
        }
    }
}