using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public static class SearchContentUtility
    {
        /// <summary>
        /// Try to load content image from project.
        /// </summary>
        /// <param name="path">Path to the image relative 'EditorResources' folder or name of built-in image with '@' prefix..</param>
        /// <param name="image">Loaded image in Texture2D representation.</param>
        /// <returns></returns>
        public static bool TryLoadContentImage(string path, out Texture2D image)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path[0] == '@')
                {
                    path = path.Remove(0, 1);
                    image = EditorGUIUtility.IconContent(path).image as Texture2D;
                }
                else
                {
                    image = ProjectDatabase.FindAssetWithPath<Texture2D>(path);
                }

                return image != null;
            }

            image = null;
            return false;
        }
    }
}