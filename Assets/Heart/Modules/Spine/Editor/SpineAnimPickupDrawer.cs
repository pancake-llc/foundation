using System.Linq;
using Pancake.Spine;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;

#if PANCAKE_SPINE
namespace Pancake.SpineEditor
{
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SpineAnimPickupAttribute))]
    public class SpineAnimPickupDrawer : SpineAttributeDrawerBase
    {
        protected override bool IsValueValid(SkeletonData skeletonData, SerializedProperty property) { return skeletonData.FindAnimation(property.stringValue) != null; }

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

            string guid = AssetDatabase.FindAssets($"{((SpineAnimPickupAttribute) attribute).Name} t:SkeletonDataAsset").FirstOrDefault();

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
                        ? NoneLabel(SpineEditorUtilities.Icons.animation)
                        : TempContent(propertyStringValue, SpineEditorUtilities.Icons.animation),
                    usedStyle))
                Selector(property);
        }


        protected override void PopulateMenu(GenericMenu menu, SerializedProperty property, SkeletonData data)
        {
            var animations = _skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations;

            for (int i = 0; i < animations.Count; i++)
            {
                string name = animations.Items[i].Name;

                menu.AddItem(new GUIContent(name),
                    !property.hasMultipleDifferentValues && name == property.stringValue,
                    HandleSelect,
                    new SpineDrawerValuePair(name, property));
            }
        }
    }
}
#endif