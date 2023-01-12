using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pancake.Linq;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "SoundPreset", menuName = "Pancake/Audio/Sound Preset", order = 0)]
    [Serializable]
    public class SoundPreset : ScriptableObject
    {
        [SerializeField, TextArea] private string developerDescription;
        [SerializeField, DisableInEditMode, DisableInPlayMode] private GameObject prefab;
        [SerializeField] private List<Sound> sounds = new List<Sound>();

        public List<Sound> Sounds => sounds;

#if UNITY_EDITOR

        [ShowIf(nameof(prefab), null)]
        [Button(ButtonSize.Medium)]
        private void FillPrefab()
        {
            const string relativePath = "Runtime/Gam/Sound";
            var upmPath = $"Packages/com.pancake.heart/{relativePath}/Sound.prefab";
            var p = !System.IO.File.Exists(System.IO.Path.GetFullPath(upmPath)) ? $"Assets/heart/{relativePath}/Sound.prefab" : upmPath;
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (prefab == null) Debug.LogError($"Couldn't load the prefab at path :{p}");
        }

        [HideIf(nameof(IsMissingAnySound))]
        [Button(ButtonSize.Medium)]
        private void Generate()
        {
            Global.ValidateScriptGenPath();
            var implPath = $"{Global.DEFAULT_SCRIPT_GEN_PATH}/Audio.cs";

            var str = "namespace Pancake\n{";
            str += "\n\tpublic static class Audio\n\t";

            for (int i = 0; i < sounds.Count; i++)
            {
                var item = sounds[i].soundName.Replace(" ", "");
                str += $"\n\t\tpublic static void Play{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item)}()";
                str += "\n\t\t{";
                str += $"\n\t\t\t";
                str += "\n\t\t}";
                str += "\n";
            }
            str += "\n\t}";
            str += "\n}";

            var writer = new StreamWriter(implPath, false);
            writer.Write(str);
            writer.Close();
            UnityEditor.AssetDatabase.ImportAsset(implPath);
        }

        private bool IsMissingAnySound()
        {
            Type audio = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                string assemblyFullName = assembly.FullName;
                if (assemblyFullName.StartsWith("Assembly-CSharp"))
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var type in types)
                    {
                        if (type.FullName == null || !type.FullName.Equals("Pancake.Audio")) continue;

                        audio = type;
                        break;
                    }
                }
            }

            if (audio != null)
            {
                var sources = sounds.Map(_ => $"Play{_.soundName.Replace(" ", "")}");
                var bindingFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var methods = audio.GetMethods(bindingFlag);
                var flag = false;
                for (int i = 0; i < sources.Count; i++)
                {
                    string s = sources[i];

                    for (int j = 0; j < methods.Length; j++)
                    {
                        string n = methods[j].Name;
                        if (n.Equals(s))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag) break;
                }

                return flag;
            }

            return false;
        }
#endif
    }
}