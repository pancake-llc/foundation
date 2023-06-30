#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using System;
using UnityEngine;

namespace Pancake.Tag
{
    public class TagGroupBase : ScriptableObject
    {
    }

    public class TagGroup<T> : TagGroupBase, ISerializationCallbackReceiver where T : ScriptableObject
    {
        public T[] assets = Array.Empty<T>();

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this)).Where(x => x is T).ToArray();
            if (subAssets.Length != assets.Length)
            {
                assets = new T[subAssets.Length];
                for (int i = 0; i < subAssets.Length; ++i) assets[i] = subAssets[i] as T;
            }
#endif
        }
    }
}