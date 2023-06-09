using Pancake.Apex;
using Pancake.ExLibEditor.Windows;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(AssetSelecterAttribute))]
    public sealed class AssetSelecterView : FieldView
    {
        private static Texture Icon;
        private static GUIStyle IconStyle;

        private AssetSelecterAttribute attribute;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static AssetSelecterView() { Icon = EditorGUIUtility.IconContent("align_vertically_center").image; }

        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as AssetSelecterAttribute;
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (IconStyle == null)
            {
                IconStyle = new GUIStyle("IconButton");
            }

            position = EditorGUI.PrefixLabel(position, label);

            position.x += 20;
            position.width -= 20;
            EditorGUI.ObjectField(position, serializedField.GetSerializedProperty(), GUIContent.none);

            EditorGUI.BeginDisabledGroup(!Directory.Exists(attribute.Path));
            Rect buttonPosition = new Rect(position.xMin - 20, position.y + 1, 20, 20);
            if (GUI.Button(buttonPosition, Icon, IconStyle))
            {
                Type type = serializedField.GetMemberType();
                if (attribute.AssetType != null)
                {
                    type = attribute.AssetType;
                }

                ExSearchWindow searchWindow = ExSearchWindow.Create();
                foreach (string path in Directory.EnumerateFiles(attribute.Path, attribute.Pattern, attribute.Option))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath(path, type);
                    if (asset != null)
                    {
                        searchWindow.AddEntry(attribute.Sort ? $"{asset.GetType().Name}/{asset.name}" : asset.name,
                            () =>
                            {
                                serializedField.SetObject(asset);
                                serializedField.GetSerializedObject().ApplyModifiedProperties();
                            });
                    }
                }

                searchWindow.Open(position);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}