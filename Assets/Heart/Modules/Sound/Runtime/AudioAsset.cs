using System.Collections.Generic;
using Sirenix.OdinInspector;
using Pancake.Common;
using UnityEngine;

namespace Pancake.Sound
{
    public class AudioAsset : ScriptableObject
    {
        public List<AudioData> audios = new();

#if UNITY_EDITOR
        [Button]
        private void LoadAll()
        {
            int countAdded = 0;
            string[] assetNames = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(AudioData)}");
            foreach (string assetName in assetNames)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetName);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioData>(assetPath);
                if (asset != null)
                {
                    if (!audios.Contains(asset))
                    {
                        audios.Add(asset);
                        countAdded++;
                    }
                }
            }

            Debug.Log("[Audio]".ToWhiteBold() + $" {countAdded} AudioData was added to AudioAsset");
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

#endif
    }
}