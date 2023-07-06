using UnityEditor;
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
    }
}