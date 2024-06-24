using System;
using System.Reflection;
using UnityEditor;

namespace PancakeEditor.Sound.Reflection
{
    public class AudioClassReflectionHelper
    {
        private Type _mixerClass;
        private Type _mixerGroupClass;
        private Type _effectClass;
        private Type _effectParameterPath;

        public Type MixerClass
        {
            get
            {
                _mixerClass ??= GetUnityAudioEditorClass("AudioMixerController");
                return _mixerClass;
            }
        }

        public Type MixerGroupClass
        {
            get
            {
                _mixerGroupClass ??= GetUnityAudioEditorClass("AudioMixerGroupController");
                return _mixerGroupClass;
            }
        }

        public Type EffectClass
        {
            get
            {
                _effectClass ??= GetUnityAudioEditorClass("AudioMixerEffectController");
                return _effectClass;
            }
        }

        public Type EffectParameterPath
        {
            get
            {
                _effectParameterPath ??= GetUnityAudioEditorClass("AudioEffectParameterPath");
                return _effectParameterPath;
            }
        }

        public static Type GetUnityAudioEditorClass(string className)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            return unityEditorAssembly?.GetType($"UnityEditor.Audio.{className}");
        }

        public static Type GetUnityEditorClass(string className)
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            return unityEditorAssembly?.GetType($"UnityEditor.{className}");
        }
    }
}