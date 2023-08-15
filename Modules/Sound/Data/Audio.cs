using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_audio")]
    [CreateAssetMenu(fileName = "Audio", menuName = "Pancake/Sound/Audio")]
    public class Audio : ScriptableObject
    {
        public bool loop;
        [Range(0f, 1f)] public float volume = 1f;
        [SerializeField, Array] private AudioGroup[] groups;

        public AudioClip[] GetClips()
        {
            int count = groups.Length;
            var result = new AudioClip[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = groups[i].GetNextClip();
            }

            return result;
        }
    }
    
    /// <summary>
    /// Represents a group of AudioClips that can be treated as one
    /// and provides automatic randomisation or sequencing based on the <c>SequenceMode</c> value.
    /// </summary>
    [Serializable]
    public class AudioGroup
    {
        public enum SequenceMode
        {
            Random,
            RandomNoImmediateRepeat,
            Sequential,
        }

        public SequenceMode sequenceMode = SequenceMode.RandomNoImmediateRepeat;
        [Array] public AudioClip[] clips;

        private int _nextClipToPlay = -1;
        private int _lastClipPlayed = -1;

        /// <summary>
        /// Chooses the next clip in the sequence, either following the order or randomly.
        /// </summary>
        /// <returns>A reference to an AudioClip</returns>
        public AudioClip GetNextClip()
        {
            // Fast out if there is only one clip to play
            if (clips.Length == 1) return clips[0];

            if (_nextClipToPlay == -1)
            {
                // Index needs to be initialised: 0 if Sequential, random if otherwise
                _nextClipToPlay = sequenceMode == SequenceMode.Sequential ? 0 : UnityEngine.Random.Range(0, clips.Length);
            }
            else
            {
                // Select next clip index based on the appropriate SequenceMode
                switch (sequenceMode)
                {
                    case SequenceMode.Random:
                        _nextClipToPlay = UnityEngine.Random.Range(0, clips.Length);
                        break;

                    case SequenceMode.RandomNoImmediateRepeat:
                        do
                        {
                            _nextClipToPlay = UnityEngine.Random.Range(0, clips.Length);
                        } while (_nextClipToPlay == _lastClipPlayed);

                        break;

                    case SequenceMode.Sequential:
                        _nextClipToPlay = (int) Mathf.Repeat(++_nextClipToPlay, clips.Length);
                        break;
                }
            }

            _lastClipPlayed = _nextClipToPlay;

            return clips[_nextClipToPlay];
        }
    }
}