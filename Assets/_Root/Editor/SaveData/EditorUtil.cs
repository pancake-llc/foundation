using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pancake.Editor.SaveData
{
    internal class EditorUtil : UnityEditor.Editor
    {
        public static bool IsPrefabInAssets(Object obj)
        {
#if UNITY_2018_3_OR_NEWER
            return PrefabUtility.IsPartOfPrefabAsset(obj);
#else
		return (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab);
#endif
        }

        /* 
     * Gets all children and components from a GameObject or GameObjects.
     * We create our own method for this because EditorUtility.CollectDeepHierarchy isn't thread safe in the Editor.
     */
        public static IEnumerable<Object> CollectDeepHierarchy(IEnumerable<GameObject> gos)
        {
            var deepHierarchy = new HashSet<Object>();
            foreach (var go in gos)
            {
                deepHierarchy.Add(go);
                deepHierarchy.UnionWith(go.GetComponents<Component>());
                foreach (Transform t in go.transform)
                    deepHierarchy.UnionWith(CollectDeepHierarchy(new GameObject[] {t.gameObject}));
            }

            return deepHierarchy;
        }
    }
}
