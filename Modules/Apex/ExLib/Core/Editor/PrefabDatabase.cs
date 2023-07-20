using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public static class PrefabDatabase
    {
        public static bool HasPrefabInSelection()
        {
            foreach (var o in Selection.gameObjects)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(o);
                if (source == null) continue;
                return true;
            }

            return false;
        }

        public static bool IsPrefabRoot(GameObject gameObject)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            if (source == null) return false;
            var r = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            return r == gameObject;
        }

        public static bool IsInPrefabMode() { return PrefabStageUtility.GetCurrentPrefabStage() != null; }

        public static GameObject GetPrefabRoot()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null ? prefabStage.prefabContentsRoot : null;
        }
    }
}