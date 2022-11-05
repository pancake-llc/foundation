using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    [ViewTarget(typeof(AssetSelecterAttribute))]
    sealed class AssetSelecterView : FieldView
    {
        private AssetSelecterAttribute attribute;
        private GUIContent content;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as AssetSelecterAttribute;
            content = EditorGUIUtility.IconContent("align_vertically_center");
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            Rect objectFieldPosition = new Rect(position.xMin + 17, position.y, position.width - 17, position.height);
            EditorGUI.ObjectField(objectFieldPosition, serializedField.GetSerializedProperty(), GUIContent.none);

            EditorGUI.BeginDisabledGroup(!Directory.Exists(attribute.Path));
            Rect buttonPosition = new Rect(position.xMin, position.y - 1, 20, position.height);
            if (GUI.Button(buttonPosition, content, "IconButton"))
            {
                Type propertyType = attribute.AssetType;
                if (propertyType == null)
                {
                    propertyType = serializedField.GetMemberType();
                }

                SearchableWindow searchableWindow = SearchableWindow.Create();
                string[] paths = Directory.GetFiles(attribute.Path, "*.*", attribute.Search);
                for (int i = 0; i < paths.Length; i++)
                {
                    Object asset = AssetDatabase.LoadAssetAtPath(paths[i], propertyType);
                    if (asset != null)
                    {
                        SearchItem emptyItem = new SearchItem(new GUIContent(asset.name));
                        emptyItem.OnClickCallback += () => serializedField.SetObject(asset);
                        searchableWindow.AddItem(emptyItem);
                    }
                }

                searchableWindow.ShowAsDropDown(objectFieldPosition);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}