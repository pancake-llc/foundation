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
    public class SpineSkinPickupDrawer : SpineAttributeDrawerBase
    {
        protected override bool IsValueValid(SkeletonData skeletonData, SerializedProperty property) { return skeletonData.FindSkin(property.stringValue) != null; }

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

            string guid = AssetDatabase.FindAssets($"{((SpineSkinPickupAttribute) attribute).Name} t:SkeletonDataAsset").FirstOrDefault();

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


        protected override void PopulateMenu(GenericMenu menu, SerializedProperty property, SkeletonData data)
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
    }
}
#endif