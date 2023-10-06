#if PANCAKE_SPINE
using System.Linq;
using Pancake.Spine;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;

namespace Pancake.SpineEditor
{
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SpineSkinPickupAttribute))]
    public class SpineSkinPickupDrawer : PropertyDrawer
    {
        private const string DEFAULT_SKIN_NAME = "default";
        private SkeletonDataAsset _skeletonDataAsset;
        private GUIContent _noneLabel;
        private static GUIStyle errorPopupStyle;

        private GUIContent NoneLabel(Texture2D image = null)
        {
            if (_noneLabel == null) _noneLabel = new GUIContent(DEFAULT_SKIN_NAME);
            _noneLabel.image = image;
            return _noneLabel;
        }

        private GUIStyle ErrorPopupStyle
        {
            get
            {
                if (errorPopupStyle == null) errorPopupStyle = new GUIStyle(EditorStyles.popup);
                errorPopupStyle.normal.textColor = Color.red;
                errorPopupStyle.hover.textColor = Color.red;
                errorPopupStyle.focused.textColor = Color.red;
                errorPopupStyle.active.textColor = Color.red;
                return errorPopupStyle;
            }
        }

        private SpineSkinPickupAttribute TargetAttribute => (SpineSkinPickupAttribute) attribute;


        private bool IsValueValid(SerializedProperty property)
        {
            if (_skeletonDataAsset != null)
            {
                var skeletonData = _skeletonDataAsset.GetSkeletonData(true);
                if (skeletonData != null && !string.IsNullOrEmpty(property.stringValue)) return IsValueValid(skeletonData, property);
            }

            return true;
        }

        private bool IsValueValid(SkeletonData skeletonData, SerializedProperty property) { return skeletonData.FindSkin(property.stringValue) != null; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
                return;
            }

            // Handle multi-editing when instances don't use the same SkeletonDataAsset.
            if (!SpineInspectorUtility.TargetsUseSameData(property.serializedObject))
            {
                EditorGUI.DelayedTextField(position, property, label);
                return;
            }

            var guid = AssetDatabase.FindAssets($"{TargetAttribute.Name} t:SkeletonDataAsset").FirstOrDefault();

            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
                if (_skeletonDataAsset == null)
                {
                    EditorGUI.LabelField(position, "ERROR:", $"Can not load SkeletonDataAsset from path: {path}");
                    return;
                }
            }

            position = EditorGUI.PrefixLabel(position, label);

            var usedStyle = IsValueValid(property) ? EditorStyles.popup : ErrorPopupStyle;
            string propertyStringValue = (property.hasMultipleDifferentValues) ? SpineInspectorUtility.EmDash : property.stringValue;
            if (GUI.Button(position,
                    string.IsNullOrEmpty(propertyStringValue)
                        ? NoneLabel(SpineEditorUtilities.Icons.skin)
                        : TempContent(propertyStringValue, SpineEditorUtilities.Icons.skin),
                    usedStyle))
                Selector(property);
        }

        private static GUIContent tempContent;

        private static GUIContent TempContent(string text, Texture2D image = null, string tooltip = null)
        {
            if (tempContent == null) tempContent = new GUIContent();
            tempContent.text = text;
            tempContent.image = image;
            tempContent.tooltip = tooltip;
            return tempContent;
        }

        protected virtual void Selector(SerializedProperty property)
        {
            var data = _skeletonDataAsset.GetSkeletonData(true);
            if (data == null) return;

            var menu = new GenericMenu();
            PopulateMenu(menu, property, this.TargetAttribute, data);
            menu.ShowAsContext();
        }

        private void PopulateMenu(GenericMenu menu, SerializedProperty property, SpineSkinPickupAttribute targetAttribute, SkeletonData data)
        {
            menu.AddDisabledItem(new GUIContent(_skeletonDataAsset.name));
            menu.AddSeparator("");

            for (var i = 0; i < data.Skins.Count; i++)
            {
                string name = data.Skins.Items[i].Name;
                menu.AddItem(new GUIContent(name),
                    !property.hasMultipleDifferentValues && name == property.stringValue,
                    HandleSelect,
                    new SpineDrawerValuePair(name, property));
            }
        }

        protected virtual void HandleSelect(object menuItemObject)
        {
            var clickedItem = (SpineDrawerValuePair) menuItemObject;
            var serializedProperty = clickedItem.property;
            if (serializedProperty.serializedObject.isEditingMultipleObjects)
                serializedProperty.stringValue = "oaifnoiasf��123526"; // HACK: to trigger change on multi-editing.
            serializedProperty.stringValue = clickedItem.stringValue;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 18; }
    }
}
#endif