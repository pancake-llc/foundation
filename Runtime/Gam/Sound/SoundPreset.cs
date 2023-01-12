using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "SoundPreset", menuName = "Pancake/Audio/Sound Preset", order = 0)]
    [Serializable]
    public class SoundPreset : ScriptableObject
    {
        [SerializeField, TextArea] private string developerDescription;

        [SerializeField, DisableInEditMode, DisableInPlayMode, ValidateInput("ValidateSoundPrefab")]
        private GameObject prefab;

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
            str += "\n\tpublic static class Audio\n\t{";

            for (int i = 0; i < sounds.Count; i++)
            {
                var item = sounds[i].soundName.Replace(" ", "");
                str += $"\n\t\t[Identificate(\"{sounds[i].ID}\")]";
                str += $"\n\t\tpublic static void Play{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item)}()";
                str += "\n\t\t{";
                str += $"\n\t\t\tPancake.MagicAudio.Play(\"{sounds[i].ID}\");";
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
                var sources = new Dictionary<string, string>();
                foreach (var t in sounds)
                {
                    sources.Add(t.ID, $"Play{t.soundName.Replace(" ", "")}");
                }

                var bindingFlag = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly;
                var methods = audio.GetMethods(bindingFlag);
                var flag = false;

                foreach (var source in sources)
                {
                    flag = false;
                    for (int j = 0; j < methods.Length; j++)
                    {
                        string n = methods[j].Name;
                        if (n.Equals(source.Value))
                        {
                            if (((IdentificateAttribute) methods[j].GetCustomAttributes(typeof(IdentificateAttribute), true)[0]).Value.Equals(source.Key)) flag = true;
                            break;
                        }
                    }

                    if (!flag) break;
                }

                return flag;
            }

            return false;
        }

        private ValidationResult ValidateSoundPrefab() { return prefab == null ? ValidationResult.Error("Prefab can not be null") : ValidationResult.Valid; }
#endif
    }
}