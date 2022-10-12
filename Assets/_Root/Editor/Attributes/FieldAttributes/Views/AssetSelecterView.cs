using Pancake.Editor.Window.Searchable;
using System;
using System.IO;
using System.Reflection;
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
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as AssetSelecterAttribute;
            content = EditorGUIUtility.IconContent("align_vertically_center");
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            Rect objectFieldPosition = new Rect(position.xMin + 17, position.y, position.width - 17, position.height);
            EditorGUI.ObjectField(objectFieldPosition, element.serializedProperty, GUIContent.none);

            EditorGUI.BeginDisabledGroup(!Directory.Exists(attribute.Path));
            Rect buttonPosition = new Rect(position.xMin, position.y - 1, 20, position.height);
            if (GUI.Button(buttonPosition, content, "IconButton"))
            {
                Type propertyType = attribute.AssetType;
                if (propertyType == null)
                {
                    propertyType = ((FieldInfo) element.memberInfo).FieldType;
                }

                SearchableWindow searchableWindow = SearchableWindow.Create();
                string[] paths = Directory.GetFiles(attribute.Path, "*.*", attribute.Search);
                for (int i = 0; i < paths.Length; i++)
                {
                    Object asset = AssetDatabase.LoadAssetAtPath(paths[i], propertyType);
                    if (asset != null)
                    {
                        SearchItem emptyItem = new SearchItem(new GUIContent(asset.name));
                        emptyItem.OnClickCallback += () => element.SetObject(asset);
                        searchableWindow.AddItem(emptyItem);
                    }
                }

                searchableWindow.ShowAsDropDown(objectFieldPosition);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}