using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Pancake.Common;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("so_blue_audioclip")]
    [CreateAssetMenu(fileName = "Audio", menuName = "Pancake/Audio Data")]
    public class AudioData : ScriptableObject, ISerializationCallbackReceiver
    {
        [Guid] public string id;
        public EAudioType type;
        public bool loop;
        [Range(0f, 1f)] public float masterVolume = 1f;
        [SerializeField] private AudioGroup[] groups;

        public (Clip[], EAudioType, float, bool) GetClips()
        {
            int count = groups.Length;
            var result = new Clip[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = groups[i].GetNextClip();
            }

            return (result, type, masterVolume, loop);
        }

        public void OnBeforeSerialize()
        {
            if (!groups.IsNullOrEmpty())
            {
                foreach (var g in groups) g.ResetLastTimePlayed();
            }
        }

        public void OnAfterDeserialize() { }

#if UNITY_EDITOR
        [Button]
        private void AddToAudioAsset()
        {
            AudioAsset audioAsset = null;
            string[] assetNames = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(AudioAsset)}");
            foreach (string assetName in assetNames)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetName);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioAsset>(assetPath);
                if (asset != null)
                {
                    audioAsset = asset;
                    break;
                }
            }

            if (audioAsset != null)
            {
                if (!audioAsset.audios.Contains(this))
                {
                    audioAsset.audios.Add(this);
                    Debug.Log("[Audio]".ToWhiteBold() + $" {name} was added to {audioAsset.name}");
                    UnityEditor.EditorUtility.SetDirty(audioAsset);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            else
            {
                Debug.LogWarning("[Audio]".ToWhiteBold() + " AudioAsset can not be found!");
            }
        }
#endif
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
        public Clip[] clips;

        private int _nextClipToPlay = -1;
        private int _lastClipPlayed = -1;
        private float _lastTimePlayed;
        [SerializeField] private bool cooldownToPlay;

        [ShowIf(nameof(cooldownToPlay)), SerializeField, Indent] private float cooldown;

        /// <summary>
        /// Chooses the next clip in the sequence, either following the order or randomly.
        /// </summary>
        /// <returns>A reference to an AudioClip</returns>
        public Clip GetNextClip()
        {
            if (cooldownToPlay)
            {
                if (Time.time - _lastTimePlayed <= cooldown) return null;

                _lastTimePlayed = Time.time;
            }

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

        internal void ResetLastTimePlayed()
        {
            _lastTimePlayed = 0;
            _nextClipToPlay = -1;
            _lastClipPlayed = -1;
        }
    }

    [Serializable]
    public class Clip
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }
}