using System;
using System.Collections.Generic;
using UnityEditor;

namespace Pancake.ExLibEditor
{
    public static class SerializedPropertyUtility
    {
        #region [Extension Methods]

        /// <summary>
        /// Get parent of serialized property.
        /// </summary>
        /// <returns>If serialized property is top, return itself.</returns>
        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            string[] paths = property.propertyPath.Split('.');
            if (paths != null && paths.Length > 1)
            {
                Array.Resize<string>(ref paths, paths.Length - 1);
                string path = string.Join(".", paths);
                return property.serializedObject.FindProperty(path);
            }

            return property;
        }

        /// <summary>
        /// Get visible children of serialized property.
        /// </summary>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                } while (currentProperty.NextVisible(false));
            }
        }

        #endregion
    }
}