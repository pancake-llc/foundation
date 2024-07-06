using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.Audio;
using static PancakeEditor.Sound.ReflectionExtension;
using System.Linq;

namespace PancakeEditor.Sound.Reflection
{
    public static class CustomParameterExposer
    {
        public const string EXPOSED_PARAMETERS_PROP_NAME = "exposedParameters";
        public const string CACHED_EXPOSED_PARAMETERS_GETTER_NAME = "exposedParamCache";
        public const string CACHED_EXPOSED_PARAMETERS_FIELD_NAME = "m_ExposedParamPathCache";

        private class ReflectedExposedAudioParameter : ReflectedClass
        {
            private const string GUID_FIELD_NAME = "guid";
            private const string NAME_FIELD_NAME = "name";

            public ReflectedExposedAudioParameter(Type targetType, GUID guid, string name)
                : base(targetType)
            {
                SetInstanceField(GUID_FIELD_NAME, guid);
                Guid = guid;

                SetInstanceField(NAME_FIELD_NAME, name);
                Name = name;
            }

            public ReflectedExposedAudioParameter(object instance)
                : base(instance)
            {
                Guid = GetInstanceField<GUID>(GUID_FIELD_NAME);
                Name = GetInstanceField<string>(NAME_FIELD_NAME);
            }

            public GUID Guid { get; private set; }
            public string Name { get; private set; }
        }

        public static void AddExposedParameter(string exposedName, object paraPathInstance, GUID guid, AudioMixer audioMixer, AudioClassReflectionHelper reflection)
        {
            if (guid == default)
            {
                Debug.LogError("Trying to expose parameter with default GUID.");
                return;
            }

            if (paraPathInstance == null)
            {
                Debug.LogError("Trying to expose null parameter.");
                return;
            }

            var exposedParameters = GetProperty<object>(EXPOSED_PARAMETERS_PROP_NAME, reflection.MixerClass, audioMixer);
            if (!TryCastObjectArrayToList(exposedParameters, out var exposedParameterList))
            {
                Debug.LogError("Cast current exposed parameters failed");
            }

            if (ContainsExposedParameter(exposedParameterList, guid))
            {
                Debug.LogError("Cannot expose the same parameter more than once.");
                return;
            }

            var parameterType = AudioClassReflectionHelper.GetUnityAudioEditorClass("ExposedAudioParameter");
            var newParam = new ReflectedExposedAudioParameter(parameterType, guid, exposedName);
            exposedParameterList.Add(newParam);

            AddElementTo(ref exposedParameters, newParam.Instance, parameterType);

            SetProperty(EXPOSED_PARAMETERS_PROP_NAME, reflection.MixerClass, audioMixer, exposedParameters);

            ExecuteMethod("OnChangedExposedParameter", ReflectionExtension.Void, reflection.MixerClass, audioMixer);

            var exposedParamCache = GetProperty<IDictionary>(CACHED_EXPOSED_PARAMETERS_GETTER_NAME, reflection.MixerClass, audioMixer, PRIVATE_FLAG);
            exposedParamCache[guid] = paraPathInstance;
            SetField(CACHED_EXPOSED_PARAMETERS_FIELD_NAME,
                reflection.MixerClass,
                audioMixer,
                exposedParamCache,
                PRIVATE_FLAG);

            //AudioMixerUtility.RepaintAudioMixerAndInspectors();
            var mixerUtil = AudioClassReflectionHelper.GetUnityEditorClass("AudioMixerUtility");
            ExecuteMethod("RepaintAudioMixerAndInspectors",
                ReflectionExtension.Void,
                mixerUtil,
                null,
                BindingFlags.Public | BindingFlags.Static);
        }

        private static bool ContainsExposedParameter(IEnumerable<ReflectedExposedAudioParameter> parameters, GUID parameter)
        {
            return parameters.Where(val => val.Guid == parameter).ToArray().Length > 0;
        }

        private static bool TryCastObjectArrayToList(object arrayObject, out List<ReflectedExposedAudioParameter> resultList)
        {
            resultList = null;
            var type = arrayObject.GetType();
            if (type.IsArray)
            {
                var array = (Array) arrayObject;
                var resultArray = new object[array.Length];
                array.CopyTo(resultArray, 0);

                resultList = new List<ReflectedExposedAudioParameter>();
                foreach (var obj in resultArray)
                {
                    resultList.Add(new ReflectedExposedAudioParameter(obj));
                }

                return resultList.Count != 0;
            }

            return false;
        }

        private static void AddElementTo(ref object originalArray, object value, Type elementType)
        {
            var type = originalArray.GetType();
            if (type.IsArray)
            {
                var array = (Array) originalArray;
                var newArray = Array.CreateInstance(elementType, array.Length + 1);
                array.CopyTo(newArray, 0);
                newArray.SetValue(value, newArray.Length - 1);
                originalArray = newArray;
            }
        }
    }
}