using UnityEngine;

namespace Pancake.Sound
{
    [System.Serializable]
    public struct SoundId
    {
        public int id;
#if UNITY_EDITOR
        [SerializeField] private ScriptableObject sourceAsset;
#endif

        public SoundId(int id)
            : this()
        {
            this.id = id;
        }

        public SoundId(EAudioType audioType, int index)
            : this()
        {
            id = audioType.GetInitialId() + index;
        }

        public static implicit operator int(SoundId soundId) => soundId.id;
        public static implicit operator SoundId(int id) => new(id);

#if UNITY_EDITOR
        public static class NameOf
        {
            public static string SourceAsset => nameof(sourceAsset);
        }
#endif
    }
}