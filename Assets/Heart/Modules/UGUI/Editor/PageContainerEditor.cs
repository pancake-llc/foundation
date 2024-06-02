using System.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(PageContainer))]
    public class PageContainerEditor : UIContainerEditor
    {
        #region Fields

        private SerializedProperty _instantiateType;
        private SerializedProperty _registerPagesByPrefab;
        private SerializedProperty _registerPagesByAddressable;
        private SerializedProperty _hasDefault;
        private readonly string[] _toggleArray = {"On", "Off"};

        #endregion

        #region Properties

        private PageContainer Target => target as PageContainer;

        protected override string[] PropertyToExclude() =>
            base.PropertyToExclude()
                .Concat(new[]
                {
                    $"<{nameof(PageContainer.InstantiateType)}>k__BackingField", $"<{nameof(PageContainer.RegisterPagesByPrefab)}>k__BackingField",
#if PANCAKE_ADDRESSABLE
                    $"<{nameof(PageContainer.RegisterPagesByAddressable)}>k__BackingField",
#endif
                    $"<{nameof(PageContainer.HasDefault)}>k__BackingField"
                })
                .ToArray();

        #endregion

        #region Unity Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            _instantiateType = serializedObject.FindProperty($"<{nameof(PageContainer.InstantiateType)}>k__BackingField");
            _registerPagesByPrefab = serializedObject.FindProperty($"<{nameof(PageContainer.RegisterPagesByPrefab)}>k__BackingField");
#if PANCAKE_ADDRESSABLE
            _registerPagesByAddressable = serializedObject.FindProperty($"<{nameof(PageContainer.RegisterPagesByAddressable)}>k__BackingField");
#endif
            _hasDefault = serializedObject.FindProperty($"<{nameof(PageContainer.HasDefault)}>k__BackingField");
        }

        #endregion

        #region GUI Process

        protected override void AdditionalGUIProcess()
        {
            Uniform.DrawGroupFoldout("page_container_initialize",
                "Initialize Setting",
                () =>
                {
                    EditorGUILayout.PropertyField(_instantiateType, GUIContent.none);
                    switch (Target.InstantiateType)
                    {
                        case EInstantiateType.ByPrefab:
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(_registerPagesByPrefab);
                            EditorGUI.indentLevel--;
                            break;
#if PANCAKE_ADDRESSABLE
                        case EInstantiateType.ByAddressable:
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(_registerPagesByAddressable);
                            EditorGUI.indentLevel--;
                            break;
#endif
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel(new GUIContent("Has Default"));
                        int select = GUILayout.Toolbar(_hasDefault.boolValue ? 0 : 1, _toggleArray);
                        _hasDefault.boolValue = select == 0;
                    }
                    EditorGUILayout.EndHorizontal();
                });
        }

        #endregion
    }
}