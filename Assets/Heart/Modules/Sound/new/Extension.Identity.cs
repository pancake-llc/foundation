using System;
using UnityEngine;

namespace Pancake.Sound
{
    public static partial class SoundExtension
    {
        public const int LAST_AUDIO_TYPE = ((int) ESoundType.All + 1) >> 1;
        public const int ID_CAPACITY = 0x10000000; // 1000 0000 in HEX. 268,435,456 in DEC

        public static int FinalIdLimit => ((ESoundType) LAST_AUDIO_TYPE).GetInitialId() + ID_CAPACITY;

        public static int GetInitialId(this ESoundType audioType)
        {
            if (audioType == ESoundType.None) return 0;

            if (audioType == ESoundType.All) return int.MaxValue;

            // Faster than Math.Log2 ()
            var result = 0;
            var type = (int) audioType;

            while (type > 0)
            {
                result += ID_CAPACITY;
                type >>= 1;
            }

            return result;
        }

        public static ESoundType ToNext(this ESoundType current)
        {
            if (current == 0) return (ESoundType) 1;

            int next = (int) current << 1;
            if (next > LAST_AUDIO_TYPE) return ESoundType.All;

            return (ESoundType) next;
        }

        public static ESoundType GetAudioType(int id)
        {
            if (id >= FinalIdLimit) return ESoundType.None;

            var resultType = ESoundType.None;
            var nextType = resultType.ToNext();

            while (nextType <= (ESoundType) LAST_AUDIO_TYPE)
            {
                if (id >= resultType.GetInitialId() && id < nextType.GetInitialId()) break;

                resultType = nextType;
                nextType = nextType.ToNext();
            }

            return resultType;
        }

        public static bool IsConcrete(this ESoundType audioType, bool checkFlags = false)
        {
            if (audioType is ESoundType.None or ESoundType.All) return false;

            if (checkFlags && FlagsExtension.GetFlagsOnCount((int) audioType) > 1) return false;

            return true;
        }

        public static void ForeachConcreteAudioType(Action<ESoundType> loopCallback)
        {
            var currentType = ESoundType.None;
            while (currentType <= (ESoundType) LAST_AUDIO_TYPE)
            {
                if (currentType.IsConcrete()) loopCallback?.Invoke(currentType);

                currentType = currentType.ToNext();
            }
        }

        public static bool Validate(string name, SoundClip[] clips, int id)
        {
            if (id <= 0)
            {
                Debug.LogWarning(LOG_TITLE + "There is a missing or unassigned SoundId.");
                return false;
            }

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning(LOG_TITLE + $"{name} has no audio clips, please assign or delete the entity.");
                return false;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                var clipData = clips[i];
                if (clipData.Clip == null)
                {
                    Debug.LogWarning(LOG_TITLE + $"Audio clip has not been assigned! please check {name} in Library Manager.");
                    return false;
                }

                float controlLength = (clipData.FadeIn > 0f ? clipData.FadeIn : 0f) + (clipData.FadeOut > 0f ? clipData.FadeOut : 0f) + clipData.StartPosition;
                if (controlLength > clipData.Clip.length)
                {
                    Debug.LogWarning(LOG_TITLE + $"Time control value should not greater than clip's length! please check clips element:{i} in {name}.");
                    return false;
                }
            }

            return true;
        }
    }
}