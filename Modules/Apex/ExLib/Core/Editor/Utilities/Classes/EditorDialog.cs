using UnityEditor;

namespace Pancake.ExLibEditor
{
    using UnityEngine;

    public static class EditorDialog
    {
        /// <summary>
        /// Deletes an object after showing a confirmation dialog.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DeleteObjectWithConfirmation(Object obj)
        {
            bool confirmDelete = EditorUtility.DisplayDialog($"Delete {obj.name}?", $"Are you sure you want to delete '{obj.name}'?", "Yes", "No");
            if (confirmDelete)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.DeleteAsset(path);
                return true;
            }

            return false;
        }
    }
}