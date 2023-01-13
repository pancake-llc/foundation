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
        public GameObject Prefab => prefab;

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

            var typeStr = typeof(SoundPreset).ToString();
            var temp = TempCollection.GetList<Sound>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeStr);
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                SoundPreset preset = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundPreset>(path);
                if (preset != null)
                {
                    for (int i = 0; i < preset.sounds.Count; i++)
                    {
                        var s = preset.sounds[i];
                        if (!temp.Contains(s)) temp.Add(s);
                    }
                }
            }

            var str = "namespace Pancake\n{";
            str += "\n\tpublic static class Audio\n\t{";

            str += "\n\t\tpublic static void PauseAll() => Pancake.MagicAudio.PauseAll();";
            str += "\n\t\tpublic static void ResumeAll() => Pancake.MagicAudio.ResumeAll();";
            str += "\n\t\tpublic static void StopAll() => Pancake.MagicAudio.StopAll();\n";
            
            for (int i = 0; i < temp.Count; i++)
            {
                var item = temp[i].soundName.Replace(" ", "");
                str += $"\n\t\t[Identificate(\"{temp[i].ID}\")]";
                str += $"\n\t\tpublic static AudioHandle Play{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item)}()";
                str += " {";
                str += $" return Pancake.MagicAudio.Play(\"{temp[i].ID}\");";
                str += " }";
                str += "\n";
            }

            str += "\n\t}";
            str += "\n}";

            temp.Dispose();
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
                    sources.Add(t.ID, $"Play{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.soundName.Replace(" ", ""))}");
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