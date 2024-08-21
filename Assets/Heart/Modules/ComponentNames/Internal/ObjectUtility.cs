using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Sisus.ComponentNames.EditorOnly
{
#if UNITY_EDITOR
    internal static class ObjectUtility
    {
        internal static void Destroy([AllowNull] Object target, bool undoable)
        {
            if (!target) return;

            if (Application.isPlaying && !PrefabUtility.IsPartOfPrefabAsset(target))
            {
                Object.Destroy(target);
                return;
            }

            if (undoable)
            {
                Undo.DestroyObjectImmediate(target);
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(target)) EditorUtility.SetDirty(target);

            Object.DestroyImmediate(target, true);
        }
    }
#endif
}