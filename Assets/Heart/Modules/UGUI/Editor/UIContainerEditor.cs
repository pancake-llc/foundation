using System;
using System.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Editor = UnityEditor.Editor;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(UIContainer), true)]
    public class UIContainerEditor : Editor
    {
        #region Fields

        private SerializedProperty _script;
        private SerializedProperty _viewShowAnimation;
        private SerializedProperty _viewHideAnimation;
        private SerializedProperty _containerName;
        private SerializedProperty _isDontDestroyOnLoad;

        private readonly string[] _tabArray = {"Move", "Rotate", "Scale", "Fade"};
        private readonly string[] _toggleArray = {"On", "Off"};
        private int _selectionValue;

        #endregion

        #region Properties

        private UIContainer Target => target as UIContainer;

        protected virtual string[] PropertyToExclude() =>
            new[]
            {
                "m_Script", $"<{nameof(UIContainer.ShowAnimation)}>k__BackingField", $"<{nameof(UIContainer.HideAnimation)}>k__BackingField",
                $"<{nameof(UIContainer.ContainerName)}>k__BackingField", "isDontDestroyOnLoad"
            };

        #endregion

        #region Unity Lifecycle

        protected virtual void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _viewShowAnimation = serializedObject.FindProperty($"<{nameof(UIContainer.ShowAnimation)}>k__BackingField");
            _viewHideAnimation = serializedObject.FindProperty($"<{nameof(UIContainer.HideAnimation)}>k__BackingField");
            _containerName = serializedObject.FindProperty($"<{nameof(UIContainer.ContainerName)}>k__BackingField");
            _isDontDestroyOnLoad = serializedObject.FindProperty("isDontDestroyOnLoad");
        }

        #endregion

        #region GUI Process

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_script);
            GUI.enabled = true;
            EditorGUILayout.Space(2);
            var prefix = $"Container - {(string.IsNullOrWhiteSpace(Target.ContainerName) ? "" : Target.ContainerName + " ")}";
            if (Target is SheetContainer && Target.name != $"{prefix}Sheet" && GUILayout.Button($"Rename to '{prefix}Sheet'"))
            {
                Target.name = $"{prefix}Sheet";
            }
            else if (Target is PageContainer && Target.name != $"{prefix}Page" && GUILayout.Button($"Rename to '{prefix}Page'"))
            {
                Target.name = $"{prefix}Page";
            }
            else if (Target is ModalContainer && Target.name != $"{prefix}Modal" && GUILayout.Button($"Rename to '{prefix}Modal'"))
            {
                Target.name = $"{prefix}Modal";
            }

            DrawAnimationSetting();
            EditorGUILayout.Space(6);
            DrawContainerSetting();

            EditorGUILayout.Space(6);
            AdditionalGUIProcess();

            DrawPropertiesExcluding(serializedObject, PropertyToExclude());

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        private void DrawAnimationSetting()
        {
            Uniform.DrawGroupFoldout("ui_container_animation",
                "Animation Setting",
                () =>
                {
                    var tabArea = EditorGUILayout.BeginVertical();
                    {
                        tabArea = new Rect(tabArea) {xMin = 18, height = 20};
                        GUI.Box(tabArea, GUIContent.none);
                        _selectionValue = GUI.Toolbar(tabArea, _selectionValue, _tabArray);
                        EditorGUILayout.Space(tabArea.height);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                    switch (_selectionValue)
                    {
                        case 0:
                            DrawAnimationSetting<MoveShowAnimation, MoveHideAnimation>("moveAnimation");
                            break;
                        case 1:
                            DrawAnimationSetting<RotateShowAnimation, RotateHideAnimation>("rotateAnimation");
                            break;
                        case 2:
                            DrawAnimationSetting<ScaleShowAnimation, ScaleHideAnimation>("scaleAnimation");
                            break;
                        case 3:
                            DrawAnimationSetting<FadeShowAnimation, FadeHideAnimation>("fadeAnimation");
                            break;
                    }
                });
        }

        private void DrawContainerSetting()
        {
            Uniform.DrawGroupFoldout("ui_container_setting",
                "Container Setting",
                () =>
                {
                    EditorGUILayout.PropertyField(_containerName);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel(new GUIContent("IsDontDestroyOnLoad"));
                        int select = GUILayout.Toolbar(_isDontDestroyOnLoad.boolValue ? 0 : 1, _toggleArray);
                        _isDontDestroyOnLoad.boolValue = select == 0;
                    }
                    EditorGUILayout.EndHorizontal();
                });
        }

        private void DrawAnimationSetting<TShow, THide>(string propertyRelative) where TShow : class where THide : class
        {
            EditorGUILayout.BeginVertical();
            {
                DrawReferenceField<TShow>(_viewShowAnimation.FindPropertyRelative(propertyRelative), "Show Animation");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            {
                DrawReferenceField<THide>(_viewHideAnimation.FindPropertyRelative(propertyRelative), "Hide Animation");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawReferenceField<T>(SerializedProperty target, string title = null) where T : class
        {
            EditorGUILayout.BeginVertical();
            {
                if (title != null) Uniform.DrawTitleField(title);

                string[] options = TypeCache.GetTypesDerivedFrom<T>().Select(x => x.Name).Prepend("None").ToArray();
                int currentIndex = target.managedReferenceValue != null ? Array.FindIndex(options, option => option == target.managedReferenceValue.GetType().Name) : 0;
                int selectedIndex = EditorGUILayout.Popup(currentIndex, options);
                if (currentIndex != selectedIndex)
                {
                    target.managedReferenceValue = selectedIndex == 0 ? null : Activator.CreateInstance(TypeCache.GetTypesDerivedFrom<T>()[selectedIndex - 1]) as T;
                    serializedObject.ApplyModifiedProperties();
                }

                if (currentIndex != 0)
                {
                    EditorGUI.indentLevel++;
                    {
                        target.isExpanded = true;
                        EditorGUILayout.PropertyField(target, GUIContent.none, true, GUILayout.ExpandHeight(false));
                    }
                    EditorGUI.indentLevel--;
                }
            }
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Virtual Methods

        protected virtual void AdditionalGUIProcess() { }

        #endregion
    }
}