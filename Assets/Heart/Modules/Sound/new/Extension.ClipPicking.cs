using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Sound
{
    public static partial class SoundExtension
    {
        public static readonly Dictionary<int, int> ClipsSequencer = new();

        public static SoundClip PickNewOne(this SoundClip[] clips, EPlayMode playMode, int id)
        {
            if (clips == null || clips.Length <= 0)
            {
                Debug.LogError(LOG_TITLE + "There is no AudioClip in asset");
                return null;
            }

            if (clips.Length == 1) playMode = EPlayMode.Single;

            switch (playMode)
            {
                case EPlayMode.Single: return clips[0];
                case EPlayMode.Sequence: return clips.PickNextClip(id);
                case EPlayMode.Random: return clips.PickRandomClip();
            }

            return default;
        }

        private static SoundClip PickNextClip(this SoundClip[] clips, int id)
        {
            var resultIndex = 0;
            if (ClipsSequencer.ContainsKey(id))
            {
                ClipsSequencer[id] = ClipsSequencer[id] + 1 >= clips.Length ? 0 : ClipsSequencer[id] + 1;
                resultIndex = ClipsSequencer[id];
            }
            else ClipsSequencer.Add(id, 0);

            return clips[resultIndex];
        }

        public static SoundClip PickRandomClip(this SoundClip[] clips)
        {
            int totalWeight = clips.Sum(x => x.Weight);

            // No Weight
            if (totalWeight == 0) return clips[Random.Range(0, clips.Length)];

            // Use Weight
            int targetWeight = Random.Range(0, totalWeight);
            var sum = 0;

            for (var i = 0; i < clips.Length; i++)
            {
                sum += clips[i].Weight;
                if (targetWeight < sum) return clips[i];
            }

            return default;
        }

        public static void ResetSequencer(int id)
        {
            if (ClipsSequencer.ContainsKey(id)) ClipsSequencer[id] = 0;
        }
    }
}