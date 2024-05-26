using System.Linq;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(ModalContainer))]
    public class ModalContainerEditor : UIContainerEditor
    {
        #region Fields

        private SerializedProperty _instantiateType;
        private SerializedProperty _registerModalsByPrefab;
        private SerializedProperty _registerModalsByAddressable;
        private SerializedProperty _modalBackdrop;

        #endregion

        #region Properties

        private ModalContainer Target => target as ModalContainer;

        protected override string[] PropertyToExclude() =>
            base.PropertyToExclude()
                .Concat(new[]
                {
                    $"<{nameof(ModalContainer.InstantiateType)}>k__BackingField", $"<{nameof(ModalContainer.RegisterModalsByPrefab)}>k__BackingField",
#if PANCAKE_ADDRESSABLE
                    $"<{nameof(ModalContainer.RegisterModalsByAddressable)}>k__BackingField",
#endif
                    "modalBackdrop"
                })
                .ToArray();

        #endregion

        #region Unity Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            _instantiateType = serializedObject.FindProperty($"<{nameof(ModalContainer.InstantiateType)}>k__BackingField");
            _registerModalsByPrefab = serializedObject.FindProperty($"<{nameof(ModalContainer.RegisterModalsByPrefab)}>k__BackingField");
#if PANCAKE_ADDRESSABLE
            _registerModalsByAddressable = serializedObject.FindProperty($"<{nameof(ModalContainer.RegisterModalsByAddressable)}>k__BackingField");
#endif
            _modalBackdrop = serializedObject.FindProperty("modalBackdrop");
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
                        EditorGUILayout.PropertyField(_registerModalsByPrefab);
                        EditorGUI.indentLevel--;
                        break;
#if PANCAKE_ADDRESSABLE
                    case EInstantiateType.ByAddressable:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_registerModalsByAddressable);
                        EditorGUI.indentLevel--;
                        break;
#endif
                }

                EditorGUILayout.PropertyField(_modalBackdrop);
                EditorGUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}