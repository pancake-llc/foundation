using System;
using UnityEngine;

namespace Pancake.Sound
{
    [Serializable]
    public struct SoundId
    {
        public int id;
#if UNITY_EDITOR
        [SerializeField] private ScriptableObject asset;
#endif

        public SoundId(int id)
            : this()
        {
            this.id = id;
        }

        public SoundId(ESoundType type, int index)
            : this()
        {
            id = type.GetInitialId() + index;
        }

        public static implicit operator int(SoundId soundId) => soundId.id;
        public static implicit operator SoundId(int id) => new(id);

#if UNITY_EDITOR
        public static class NameOf
        {
            public static string Asset => nameof(asset);
        }
#endif
    }
}