using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : CacheGameComponent<AudioSource>
    {
        public event UnityAction<SoundEmitter> OnCompleted;

        protected override void Awake()
        {
            base.Awake();
            component.playOnAwake = false;
        }

        /// <summary>
        /// Instructs the AudioSource to play a single clip, with optional looping, in a position in 3D space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="config"></param>
        /// <param name="loop"></param>
        /// <param name="position"></param>
        public void PlayAudioClip(AudioClip clip, AudioConfig config, bool loop, Vector3 position = default)
        {
            component.clip = clip;
            config.ApplyTo(component);
            component.transform.position = position;
            component.loop = loop;
            component.time = 0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
            component.Play();
            if (!loop)
            {
                StartCoroutine(IeFinishPlaying(clip.length));
            }
        }

        private IEnumerator IeFinishPlaying(float length)
        {
            yield return new WaitForSeconds(length);
            NotifyBeingDone();
        }

        private void NotifyBeingDone()
        {
            OnCompleted?.Invoke(this); // The AudioManager will pick this up
        }

        public void FadeMusicIn(AudioClip clip, AudioConfig config, float duration, float startTime = 0f)
        {
            PlayAudioClip(clip, config, true);

            component.volume = 0f;
            //Start the clip at the same time the previous one left, if length allows
            //TODO: find a better way to sync fading songs
            if (startTime <= component.clip.length) component.time = startTime;
            
            
        }
    }
}