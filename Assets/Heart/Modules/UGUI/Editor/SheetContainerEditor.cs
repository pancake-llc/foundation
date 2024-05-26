using System.Linq;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(SheetContainer))]
    public class SheetContainerEditor : UIContainerEditor
    {
        #region Fields

        private SerializedProperty _instantiateType;
        private SerializedProperty _registerSheetsByPrefab;
        private SerializedProperty _registerSheetsByAddressable;
        private SerializedProperty _hasDefault;
        private readonly string[] _toggleArray = {"On", "Off"};

        #endregion

        #region Properties

        private SheetContainer Target => target as SheetContainer;

        protected override string[] PropertyToExclude() =>
            base.PropertyToExclude()
                .Concat(new[]
                {
                    $"<{nameof(SheetContainer.InstantiateType)}>k__BackingField", $"<{nameof(SheetContainer.RegisterSheetsByPrefab)}>k__BackingField",
#if PANCAKE_ADDRESSABLE
                    $"<{nameof(SheetContainer.RegisterSheetsByAddressable)}>k__BackingField",
#endif
                    $"<{nameof(SheetContainer.HasDefault)}>k__BackingField"
                })
                .ToArray();

        #endregion

        #region Unity Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            _instantiateType = serializedObject.FindProperty($"<{nameof(SheetContainer.InstantiateType)}>k__BackingField");
            _registerSheetsByPrefab = serializedObject.FindProperty($"<{nameof(SheetContainer.RegisterSheetsByPrefab)}>k__BackingField");
#if PANCAKE_ADDRESSABLE
            _registerSheetsByAddressable = serializedObject.FindProperty($"<{nameof(SheetContainer.RegisterSheetsByAddressable)}>k__BackingField");
#endif
            _hasDefault = serializedObject.FindProperty($"<{nameof(SheetContainer.HasDefault)}>k__BackingField");
        }

        #endregion

        #region GUI Process

        protected override void AdditionalGUIProcess()
        {
            var area = EditorGUILayout.BeginVertical();
            {
                GUI.Box(area, GUIContent.none);
                DrawTitleField("Initialize Setting");
                EditorGUILayout.PropertyField(_instantiateType, GUIContent.none);
                switch (Target.InstantiateType)
                {
                    case EInstantiateType.ByPrefab:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_registerSheetsByPrefab);
                        EditorGUI.indentLevel--;
                        break;
#if PANCAKE_ADDRESSABLE
                    case EInstantiateType.ByAddressable:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_registerSheetsByAddressable);
                        EditorGUI.indentLevel--;
                        break;
#endif
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PrefixLabel(new GUIContent("Has Default"));
                    var select = GUILayout.Toolbar(_hasDefault.boolValue ? 0 : 1, _toggleArray);
                    _hasDefault.boolValue = select == 0;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}