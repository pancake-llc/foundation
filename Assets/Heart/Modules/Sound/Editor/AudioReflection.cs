using System;
using System.Collections.Generic;
using Pancake.Common;
using Pancake.Sound;
using UnityEditor;
using UnityEngine.Audio;
using static PancakeEditor.Sound.ReflectionExtension;

namespace PancakeEditor.Sound.Reflection
{
    public static class AudioReflection
    {
        public enum MethodName
        {
            None,
            // ReSharper disable once InconsistentNaming
            GetGUIDForVolume,
            // ReSharper disable once InconsistentNaming
            GetGUIDForPitch,
            // ReSharper disable once InconsistentNaming
            GetGUIDForMixLevel,
            GetValueForVolume,
            GetValueForPitch,
            SetValueForVolume,
            SetValueForPitch,
        }

        public const string DEFAULT_SNAPSHOT = "Snapshot";
        public const string SEND_EFFECT_NAME = "Send";
        public const string ATTENUATION_EFFECT_PARAMETER = "Attenuation";
        public const string DUCK_VOLUME_EFFECT = "Duck Volume";

        public const string SEND_TARGET_PROPERTY = "sendTarget";
        public const string COLOR_INDEX_PARAMETER = "userColorIndex";
        public const string WET_MIX_PROPERTY = "enableWetMix";

        public static AudioMixerGroup DuplicateAudioTrack(
            AudioMixer mixer,
            AudioMixerGroup parentTrack,
            AudioMixerGroup sourceTrack,
            string newTrackName,
            EExposedParameterType exposedParameterType = EExposedParameterType.All)
        {
            // Using [DuplicateGroupRecurse] method on AudioMixerController will cause some unexpected result.
            // Create a new one and copy the setting manually might be better.

            var reflection = new AudioClassReflectionHelper();

            var newGroup = ExecuteMethod("CreateNewGroup", new object[] {newTrackName, false}, reflection.MixerClass, mixer) as AudioMixerGroup;
            if (newGroup != null)
            {
                ExecuteMethod("AddChildToParent", new object[] {newGroup, parentTrack}, reflection.MixerClass, mixer);
                ExecuteMethod("AddGroupToCurrentView", new object[] {newGroup}, reflection.MixerClass, mixer);
                ExecuteMethod("OnSubAssetChanged", null, reflection.MixerClass, mixer);

                CopyColorIndex(sourceTrack, newGroup, reflection);
                CopyMixerGroupValue(EExposedParameterType.Volume,
                    mixer,
                    reflection.MixerGroupClass,
                    sourceTrack.name,
                    newGroup);
                //CopyMixerGroupValue(ExposedParameterType.Pitch, mixer, reflection.MixerGroupClass, sourceTrack.name, newGroup);

                object effect = CopySendEffect(sourceTrack, newGroup, reflection);

                ExposeParameterIfContains(EExposedParameterType.Volume);
                //ExposeParameterIfContains(ExposedParameterType.Pitch);
                ExposeParameterIfContains(EExposedParameterType.EffectSend, effect);
            }

            return newGroup;

            void ExposeParameterIfContains(EExposedParameterType targetType, params object[] additionalObjects)
            {
                if (exposedParameterType.HasFlagUnsafe(targetType))
                {
                    ExposeParameter(targetType, newGroup, reflection, additionalObjects);
                }
            }
        }

        private static object CreateParameterPathInstance(string className, params object[] parameters)
        {
            var type = AudioClassReflectionHelper.GetUnityAudioEditorClass(className);
            return CreateNewObjectWithReflection(type, parameters);
        }

        private static void CopyColorIndex(AudioMixerGroup sourceGroup, AudioMixerGroup targetGroup, AudioClassReflectionHelper reflection)
        {
            var colorIndex = GetProperty<int>(COLOR_INDEX_PARAMETER, reflection.MixerGroupClass, sourceGroup);
            SetProperty(COLOR_INDEX_PARAMETER, reflection.MixerGroupClass, targetGroup, colorIndex);
        }

        private static void CopyMixerGroupValue(EExposedParameterType parameterType, AudioMixer mixer, Type mixerGroupClass, string sourceTrackName, AudioMixerGroup to)
        {
            MethodName setterMethod = default;
            string getterParaName = null;
            var snapshot = mixer.FindSnapshot(DEFAULT_SNAPSHOT);

            switch (parameterType)
            {
                case EExposedParameterType.Volume:
                    setterMethod = MethodName.SetValueForVolume;
                    getterParaName = sourceTrackName;
                    break;
                case EExposedParameterType.Pitch:
                    setterMethod = MethodName.SetValueForPitch;
                    getterParaName = sourceTrackName + AudioConstant.PITCH_PARA_NAME_SUFFIX;
                    break;
                case EExposedParameterType.EffectSend:
                    UnityEngine.Debug.LogError("This method can only be used with mixerGroup only");
                    return;
            }

            // don't know why this can't be done, it always returns 0f
            //object value = ExecuteMethod(getterMethod.ToString(), new object[] { mixer, snapshot }, mixerGroupClass, from);

            if (mixer.SafeGetFloat(getterParaName, out float value))
            {
                ExecuteMethod(setterMethod.ToString(), new object[] {mixer, snapshot, value}, mixerGroupClass, to);
            }
        }

        private static object CopySendEffect(AudioMixerGroup sourceGroup, AudioMixerGroup targetGroup, AudioClassReflectionHelper reflection)
        {
            if (TryGetFirstEffect(sourceGroup,
                    SEND_EFFECT_NAME,
                    reflection,
                    out object sourceSendEffect,
                    out int effectIndex))
            {
                var sendTarget = GetProperty<object>(SEND_TARGET_PROPERTY, reflection.EffectClass, sourceSendEffect);
                var clonedEffect = ExecuteMethod("CopyEffect", new[] {sourceSendEffect}, reflection.MixerClass, sourceGroup.audioMixer);
                SetProperty(SEND_TARGET_PROPERTY, reflection.EffectClass, clonedEffect, sendTarget);
                SetProperty(WET_MIX_PROPERTY, reflection.EffectClass, clonedEffect, true);
                ExecuteMethod("InsertEffect", new[] {clonedEffect, effectIndex}, reflection.MixerGroupClass, targetGroup);
                return clonedEffect;
            }

            return null;
        }

        public static bool TryGetFirstEffect(
            AudioMixerGroup mixerGroup,
            string targetEffectName,
            AudioClassReflectionHelper reflection,
            out object result,
            out int effectIndex,
            bool isAscending = true)
        {
            result = null;
            effectIndex = 0;
            object[] effects = GetProperty<object[]>("effects", reflection.MixerGroupClass, mixerGroup);

            if (isAscending)
            {
                for (var i = 0; i < effects.Length; i++)
                {
                    if (IsTarget(i))
                    {
                        result = effects[i];
                        effectIndex = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = effects.Length - 1; i >= 0; i--)
                {
                    if (IsTarget(i))
                    {
                        result = effects[i];
                        effectIndex = i;
                        break;
                    }
                }
            }

            return result != null;

            bool IsTarget(int index)
            {
                var effectName = GetProperty<string>("effectName", reflection.EffectClass, effects[index]);
                return effectName.Equals(targetEffectName);
            }
        }

        public static void ExposeParameter(
            EExposedParameterType parameterType,
            AudioMixerGroup mixerGroup,
            AudioClassReflectionHelper reflection = null,
            params object[] additionalObjects)
        {
            reflection ??= new AudioClassReflectionHelper();
            var audioMixer = mixerGroup.audioMixer;

            switch (parameterType)
            {
                case EExposedParameterType.Volume:
                    if (TryGetGuid(MethodName.GetGUIDForVolume, reflection.MixerGroupClass, mixerGroup, out var volGuid))
                    {
                        object volParaPath = CreateParameterPathInstance("AudioGroupParameterPath", mixerGroup, volGuid);
                        CustomParameterExposer.AddExposedParameter(mixerGroup.name,
                            volParaPath,
                            volGuid,
                            audioMixer,
                            reflection);
                    }

                    break;
                case EExposedParameterType.Pitch:
                    if (TryGetGuid(MethodName.GetGUIDForPitch, reflection.MixerGroupClass, mixerGroup, out var pitchGuid))
                    {
                        object pitchParaPath = CreateParameterPathInstance("AudioGroupParameterPath", mixerGroup, pitchGuid);
                        CustomParameterExposer.AddExposedParameter(mixerGroup.name + AudioConstant.PITCH_PARA_NAME_SUFFIX,
                            pitchParaPath,
                            pitchGuid,
                            audioMixer,
                            reflection);
                    }

                    break;
                case EExposedParameterType.EffectSend:
                    object effect;
                    if (additionalObjects != null && additionalObjects.Length > 0)
                    {
                        effect = additionalObjects[0];
                    }
                    else if (!TryGetFirstEffect(mixerGroup,
                                 SEND_EFFECT_NAME,
                                 reflection,
                                 out effect,
                                 out _))
                    {
                        UnityEngine.Debug.LogError($"Can't expose [{SEND_EFFECT_NAME}] on AudioMixerGroup:{mixerGroup.name}");
                        return;
                    }

                    if (TryGetGuid(MethodName.GetGUIDForMixLevel, reflection.EffectClass, effect, out var effectGuid))
                    {
                        object effectParaPath = CreateParameterPathInstance("AudioEffectParameterPath", mixerGroup, effect, effectGuid);
                        CustomParameterExposer.AddExposedParameter(mixerGroup.name + AudioConstant.EFFECT_PARA_NAME_SUFFIX,
                            effectParaPath,
                            effectGuid,
                            audioMixer,
                            reflection);
                    }

                    break;
            }
        }

        public static void RemoveAudioEffect(AudioMixer mixer, string targetEffectName, AudioMixerGroup mixerGroup, AudioClassReflectionHelper reflection = null)
        {
            reflection ??= new AudioClassReflectionHelper();

            object[] effects = GetProperty<object[]>("effects", reflection.MixerGroupClass, mixerGroup);

            for (var i = 0; i < effects.Length; i++)
            {
                var effectName = GetProperty<string>("effectName", reflection.EffectClass, effects[i]);
                if (effectName == targetEffectName)
                {
                    ExecuteMethod("RemoveEffect", new[] {effects[i], mixerGroup}, reflection.MixerClass, mixer);
                    break;
                }
            }
        }

        private static void AssignSendTarget(object sendTarget, AudioMixerGroup mixerGroup, bool isSendInLast, AudioClassReflectionHelper reflection = null)
        {
            reflection ??= new AudioClassReflectionHelper();

            if (mixerGroup != null && TryGetFirstEffect(mixerGroup,
                    SEND_EFFECT_NAME,
                    reflection,
                    out object sendEffect,
                    out _,
                    !isSendInLast))
            {
                SetProperty("sendTarget", reflection.EffectClass, sendEffect, sendTarget);
            }
        }

        private static void AssignSendTarget(object sendTarget, bool isSendInLast, IEnumerable<AudioMixerGroup> mixerGroups)
        {
            foreach (var group in mixerGroups)
            {
                AssignSendTarget(sendTarget, group, isSendInLast);
            }
        }


        private static bool TryGetGuid(MethodName methodName, Type type, object target, out GUID guid)
        {
            guid = default;
            object obj = ExecuteMethod(methodName.ToString(), ReflectionExtension.Void, type, target);
            return TryConvertGuid(obj, ref guid);
        }

        private static bool TryConvertGuid(object obj, ref GUID guid)
        {
            try
            {
                guid = (GUID) obj;
            }
            catch (InvalidCastException)
            {
                UnityEngine.Debug.LogError(AudioConstant.LOG_HEADER + $"Cast GUID failed! object :{obj}");
            }

            return guid != default && !guid.Empty();
        }
    }
}