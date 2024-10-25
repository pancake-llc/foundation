using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
#if UNITY_EDITOR
    public static class ObjectUtility
    {
        public static void Destroy([AllowNull] Object target, bool undoable)
        {
            if(!target)
            {
                return;
            }

            if(Application.isPlaying && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(target))
            {
                Object.Destroy(target);
                return;
            }

            if(undoable)
            {
                Undo.DestroyObjectImmediate(target);
                return;
            }

            if(UnityEditor.PrefabUtility.IsPartOfPrefabAsset(target))
            {
                EditorUtility.SetDirty(target);
            }

            Object.DestroyImmediate(target, true);
        }
    }
#endif
}