using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.SpineEditor
{
    public abstract class SpineAttributeDrawerBase : PropertyDrawer
    {
        private const string DEFAULT_SKIN_NAME = "default";
        protected SkeletonDataAsset _skeletonDataAsset;
        private GUIContent _noneLabel;
        private static GUIStyle errorPopupStyle;
        private static GUIContent tempContent;

        protected GUIContent NoneLabel(Texture2D image = null)
        {
            if (_noneLabel == null) _noneLabel = new GUIContent(DEFAULT_SKIN_NAME);
            _noneLabel.image = image;
            return _noneLabel;
        }

        protected GUIStyle ErrorPopupStyle
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

        protected bool IsValueValid(SerializedProperty property)
        {
            if (_skeletonDataAsset != null)
            {
                var skeletonData = _skeletonDataAsset.GetSkeletonData(true);
                if (skeletonData != null && !string.IsNullOrEmpty(property.stringValue)) return IsValueValid(skeletonData, property);
            }

            return true;
        }

        protected abstract bool IsValueValid(SkeletonData skeletonData, SerializedProperty property);

        protected static GUIContent TempContent(string text, Texture2D image = null, string tooltip = null)
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
            PopulateMenu(menu, property, data);
            menu.ShowAsContext();
        }

        protected abstract void PopulateMenu(GenericMenu menu, SerializedProperty property, SkeletonData data);

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