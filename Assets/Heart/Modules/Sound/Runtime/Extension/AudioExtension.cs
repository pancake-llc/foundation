using System;
using System.Collections.Generic;
using LitMotion;
using Pancake.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static Pancake.Sound.AudioConstant;
using Random = UnityEngine.Random;

namespace Pancake.Sound
{
    public static class AudioExtension
    {
        public struct AudioClipSetting
        {
            public readonly int frequency;
            public readonly int channels;
            public readonly int samples;
            public readonly bool ambisonic;
            public readonly AudioClipLoadType loadType;
            public readonly bool preloadAudioData;
            public readonly bool loadInBackground;
            public readonly AudioDataLoadState loadState;

            public AudioClipSetting(AudioClip originClip, bool isMono)
            {
                frequency = originClip.frequency;
                channels = isMono ? 1 : originClip.channels;
                samples = originClip.samples;
                ambisonic = originClip.ambisonic;
                loadType = originClip.loadType;
                preloadAudioData = originClip.preloadAudioData;
                loadInBackground = originClip.loadInBackground;
                loadState = originClip.loadState;
            }
        }

        private const float SECONDS_PER_MINUTE = 60f;
        public const int LAST_AUDIO_TYPE = ((int) EAudioType.All + 1) >> 1;
        public const int ID_CAPACITY = 0x10000000; // 1000 0000 in HEX. 268,435,456 in DEC
        public static int FinalIDLimit => ((EAudioType) LAST_AUDIO_TYPE).GetInitialId() + ID_CAPACITY;
        public static readonly Dictionary<int, int> ClipsSequencer = new();

        public static float ToDecibel(this float vol, bool allowBoost = true) { return Mathf.Log10(vol.ClampNormalize(allowBoost)) * DEFAULT_DECIBEL_VOLUME_SCALE; }

        public static float ToNormalizeVolume(this float dB, bool allowBoost = true)
        {
            float maxVol = allowBoost ? MAX_DECIBEL_VOLUME : FULL_DECIBEL_VOLUME;
            if (dB >= maxVol) return allowBoost ? MAX_VOLUME : FULL_VOLUME;

            return Mathf.Pow(10, dB.ClampDecibel(allowBoost) / DEFAULT_DECIBEL_VOLUME_SCALE);
        }

        public static float ClampNormalize(this float vol, bool allowBoost = false) { return Mathf.Clamp(vol, MIN_VOLUME, allowBoost ? MAX_VOLUME : FULL_VOLUME); }

        public static float ClampDecibel(this float dB, bool allowBoost = false)
        {
            return Mathf.Clamp(dB, MIN_DECIBEL_VOLUME, allowBoost ? MAX_DECIBEL_VOLUME : FULL_DECIBEL_VOLUME);
        }

        public static bool TryGetSampleData(this AudioClip originClip, out float[] sampleArray, float startPosInSecond, float endPosInSecond)
        {
            int dataSampleLength = GetDataSample(originClip, originClip.length - endPosInSecond - startPosInSecond);

            sampleArray = new float[dataSampleLength];
            bool sucess = originClip.GetData(sampleArray, GetTimeSample(originClip, startPosInSecond));

            if (!sucess) Debug.LogError($"Can't get audio clip : {originClip.name} 's sample data!");
            return sucess;
        }

        public static float[] GetSampleData(this AudioClip originClip, float startPosInSecond = 0f, float endPosInSecond = 0f)
        {
            if (TryGetSampleData(originClip, out float[] sampleArray, startPosInSecond, endPosInSecond)) return sampleArray;

            return null;
        }

        public static AudioClip CreateAudioClip(string name, float[] samples, AudioClipSetting setting)
        {
            AudioClip result = AudioClip.Create(name,
                samples.Length / setting.channels,
                setting.channels,
                setting.frequency,
                setting.loadType == AudioClipLoadType.Streaming);
            result.SetData(samples, 0);
            return result;
        }

        public static int GetDataSample(AudioClip clip, float time, MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            return (int) Math.Round(clip.frequency * clip.channels * time, rounding);
        }

        public static int GetTimeSample(AudioClip clip, float time, MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            return (int) Math.Round(clip.frequency * time, rounding);
        }

        public static AudioClipSetting GetAudioClipSetting(this AudioClip audioClip, bool isMono = false) { return new AudioClipSetting(audioClip, isMono); }

        public static bool IsValidFrequency(float freq)
        {
            if (freq is < MIN_FREQUENCY or > MAX_FREQUENCY)
            {
                Debug.LogError($"The given frequency should be in {MIN_FREQUENCY}Hz ~ {MAX_FREQUENCY}Hz.");
                return false;
            }

            return true;
        }

        public static float TempoToTime(float bpm, int beats)
        {
            if (bpm == 0) return 0;
            return SECONDS_PER_MINUTE / bpm * beats;
        }

        public static void ChangeChannel(this AudioMixer mixer, string from, string to, float targetVol)
        {
            mixer.SafeSetFloat(from, MIN_DECIBEL_VOLUME);
            mixer.SafeSetFloat(to, targetVol);
        }

        public static void SafeSetFloat(this AudioMixer mixer, string parameterName, float value)
        {
            if (!string.IsNullOrEmpty(parameterName)) mixer.SetFloat(parameterName, value);
        }

        public static bool SafeGetFloat(this AudioMixer mixer, string parameterName, out float value)
        {
            value = default;
            if (!string.IsNullOrEmpty(parameterName)) return mixer.GetFloat(parameterName, out value);

            return false;
        }

        public static IEnumerable<float> GetLerpValuesPerFrame(float start, float target, float duration, Ease ease)
        {
            var currentTime = 0f;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newValue = Mathf.Lerp(start, target, EaseUtility.Evaluate(currentTime / duration, ease));
                yield return newValue;
            }
        }

        public static string ToName(this int id) { return SoundManager.Instance.GetNameByID(id); }

        public static bool IsDefaultCurve(this AnimationCurve curve, float defaultValue)
        {
            if (curve == null || curve.length == 0) return true;

            if (curve.length == 1 && Mathf.Approximately(curve[0].value, defaultValue)) return true;

            return false;
        }

        public static void SetCustomCurveOrResetDefault(this AudioSource audioSource, AnimationCurve curve, AudioSourceCurveType curveType)
        {
            if (curveType == AudioSourceCurveType.CustomRolloff)
            {
                Debug.LogError(LOG_HEADER + $"Don't use this method on {AudioSourceCurveType.CustomRolloff}, please use RolloffMode to detect if is default or not");
                return;
            }

            float defaultValue = GetCurveDefaultValue(curveType);

            if (!curve.IsDefaultCurve(defaultValue)) audioSource.SetCustomCurve(curveType, curve);
            else
            {
                switch (curveType)
                {
                    case AudioSourceCurveType.SpatialBlend:
                        audioSource.spatialBlend = defaultValue;
                        break;
                    case AudioSourceCurveType.ReverbZoneMix:
                        audioSource.reverbZoneMix = defaultValue;
                        break;
                    case AudioSourceCurveType.Spread:
                        audioSource.spread = defaultValue;
                        break;
                }
            }
        }

        private static float GetCurveDefaultValue(AudioSourceCurveType curveType)
        {
            switch (curveType)
            {
                case AudioSourceCurveType.SpatialBlend: return SPATIAL_BLEND_2D;
                case AudioSourceCurveType.ReverbZoneMix: return DEFAULT_REVER_ZONE_MIX;
                case AudioSourceCurveType.Spread: return DEFAULT_SPREAD;
            }

            return default;
        }

        public static SoundClip PickNewOne(this SoundClip[] clips, EAudioPlayMode playMode, int id)
        {
            if (clips is not {Length: > 0})
            {
                Debug.LogError(LOG_HEADER + "There is no AudioClip in asset");
                return null;
            }

            if (clips.Length == 1) playMode = EAudioPlayMode.Single;

            switch (playMode)
            {
                case EAudioPlayMode.Single: return clips[0];
                case EAudioPlayMode.Sequence: return clips.PickNextClip(id);
                case EAudioPlayMode.Random: return clips.PickRandomClip();
            }

            return default;
        }

        private static SoundClip PickNextClip(this SoundClip[] clips, int id)
        {
            var resultIndex = 0;
            bool status = ClipsSequencer.TryAdd(id, 0);
            if (!status)
            {
                ClipsSequencer[id] = ClipsSequencer[id] + 1 >= clips.Length ? 0 : ClipsSequencer[id] + 1;
                resultIndex = ClipsSequencer[id];
            }

            return clips[resultIndex];
        }

        public static SoundClip PickRandomClip(this SoundClip[] clips)
        {
            int totalWeight = clips.Sum(x => x.Weight);

            // No Weight
            if (totalWeight == 0) return clips[Random.Range(0, clips.Length)];

            // Use Weight
            int targetWeight = Random.Range(0, totalWeight);
            int sum = 0;

            for (int i = 0; i < clips.Length; i++)
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

        public static int GetInitialId(this EAudioType audioType)
        {
            if (audioType == EAudioType.None) return 0;
            if (audioType == EAudioType.All) return int.MaxValue;

            // Faster than Math.Log2 ()
            int result = 0;
            int type = (int) audioType;

            while (type > 0)
            {
                result += ID_CAPACITY;
                type >>= 1;
            }

            return result;
        }

        public static EAudioType ToNext(this EAudioType current)
        {
            if (current == 0) return (EAudioType) 1;

            int next = (int) current << 1;
            if (next > LAST_AUDIO_TYPE) return EAudioType.All;
            return (EAudioType) next;
        }

        public static EAudioType GetAudioType(int id)
        {
            if (id >= FinalIDLimit) return EAudioType.None;

            var resultType = EAudioType.None;
            var nextType = resultType.ToNext();

            while (nextType <= (EAudioType) LAST_AUDIO_TYPE)
            {
                if (id >= resultType.GetInitialId() && id < nextType.GetInitialId()) break;

                resultType = nextType;
                nextType = nextType.ToNext();
            }

            return resultType;
        }

        public static bool IsConcrete(this EAudioType audioType, bool checkFlags = false)
        {
            if (audioType is EAudioType.None or EAudioType.All) return false;
            if (checkFlags && FlagsExtension.GetFlagsOnCount((int) audioType) > 1) return false;
            return true;
        }

        public static void ForeachConcreteAudioType(Action<EAudioType> loopCallback)
        {
            EAudioType currentType = EAudioType.None;
            while (currentType <= (EAudioType) LAST_AUDIO_TYPE)
            {
                if (currentType.IsConcrete()) loopCallback?.Invoke(currentType);

                currentType = currentType.ToNext();
            }
        }

        public static bool Validate(string name, SoundClip[] clips, int id)
        {
            if (id <= 0)
            {
                Debug.LogWarning(LOG_HEADER + "There is a missing or unassigned SoundID.");
                return false;
            }

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning(LOG_HEADER + $"{name} has no audio clips, please assign or delete the entity.");
                return false;
            }

            for (int i = 0; i < clips.Length; i++)
            {
                var clipData = clips[i];
                if (clipData.AudioClip == null)
                {
                    Debug.LogError(LOG_HEADER + $"Audio clip has not been assigned! please check {name} in Library Manager.");
                    return false;
                }

                float controlLength = (clipData.FadeIn > 0f ? clipData.FadeIn : 0f) + (clipData.FadeOut > 0f ? clipData.FadeOut : 0f) + clipData.StartPosition;
                if (controlLength > clipData.AudioClip.length)
                {
                    Debug.LogError(LOG_HEADER + $"Time control value should not greater than clip's length! please check clips element:{i} in {name}.");
                    return false;
                }
            }

            return true;
        }
    }
}