using System;
using System.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(UIContext), true)]
    public class UIContextEditor : UnityEditor.Editor
    {
        #region Fields

        private SerializedProperty _script;
        private SerializedProperty _animationSetting;
        private SerializedProperty _animationType;
        private SerializedProperty _showAnimation;
        private SerializedProperty _hideAnimation;

        #endregion

        #region Properties

        private UIContext Target => target as UIContext;

        protected virtual string[] PropertyToExclude() => new[] {"m_Script", "showAnimation", "hideAnimation", "animationSetting", "animationType"};

        private SerializedProperty ShowAnimation
        {
            get
            {
                SerializedProperty targetAnimation;
                if (_animationSetting.enumValueIndex == 0)
                {
                    var parentContainer = Target.GetComponentInParent<UIContainer>();
                    if (parentContainer)
                    {
                        var container = new SerializedObject(parentContainer);
                        targetAnimation = container.FindProperty($"<{nameof(UIContainer.ShowAnimation)}>k__BackingField");
                    }
                    else return null;
                }
                else targetAnimation = _showAnimation;

                return targetAnimation;
            }
        }

        private SerializedProperty HideAnimation
        {
            get
            {
                SerializedProperty targetAnimation;
                if (_animationSetting.enumValueIndex == 0)
                {
                    var parentContainer = Target.GetComponentInParent<UIContainer>();
                    if (parentContainer)
                    {
                        var container = new SerializedObject(parentContainer);
                        targetAnimation = container.FindProperty($"<{nameof(UIContainer.HideAnimation)}>k__BackingField");
                    }
                    else return null;
                }
                else targetAnimation = _hideAnimation;

                return targetAnimation;
            }
        }

        #endregion

        #region Unity Lifecycle

        protected virtual void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _animationSetting = serializedObject.FindProperty("animationSetting");
            _animationType = serializedObject.FindProperty("animationType");
            _showAnimation = serializedObject.FindProperty("showAnimation");
            _hideAnimation = serializedObject.FindProperty("hideAnimation");
        }

        #endregion

        #region GUI Process

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_script);
            GUI.enabled = true;

            DrawAnimationSetting();

            AdditionalGUIProcess();

            DrawPropertiesExcluding(serializedObject, PropertyToExclude());

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        private void DrawAnimationSetting()
        {
            Uniform.DrawGroupFoldout("ui_context_animation",
                "Animation Setting",
                () =>
                {
                    var settingTabArea = EditorGUILayout.BeginVertical();
                    {
                        settingTabArea = new Rect(settingTabArea) {xMin = 18, height = 20};
                        GUI.Box(settingTabArea, GUIContent.none);
                        _animationSetting.enumValueIndex = GUI.Toolbar(settingTabArea, _animationSetting.enumValueIndex, _animationSetting.enumNames);
                        EditorGUILayout.Space(settingTabArea.height);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);

                    var animationTabArea = EditorGUILayout.BeginVertical();
                    {
                        animationTabArea = new Rect(animationTabArea) {xMin = 18, height = 20};
                        GUI.Box(animationTabArea, GUIContent.none);
                        _animationType.enumValueIndex = GUI.Toolbar(animationTabArea, _animationType.enumValueIndex, _animationSetting.enumNames);
                        EditorGUILayout.Space(animationTabArea.height);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                    switch (_animationType.enumValueIndex)
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

        private void DrawAnimationSetting<TShow, THide>(string propertyRelative) where TShow : class where THide : class
        {
            GUI.enabled = _animationSetting.enumValueIndex != 0;
            EditorGUILayout.BeginVertical();
            {
                DrawReferenceField<TShow>(ShowAnimation?.FindPropertyRelative(propertyRelative), "Show Animation");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            {
                DrawReferenceField<THide>(HideAnimation?.FindPropertyRelative(propertyRelative), "Hide Animation");
            }
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void DrawReferenceField<T>(SerializedProperty target, string title = null) where T : class
        {
            EditorGUILayout.BeginVertical();
            {
                if (title != null) Uniform.DrawTitleField(title);

                if (target != null)
                {
                    string[] options = TypeCache.GetTypesDerivedFrom<T>().Select(x => x.Name).Prepend("None").ToArray();
                    int currentIndex = target.managedReferenceValue != null
                        ? Array.FindIndex(options, option => option == target.managedReferenceValue.GetType().Name)
                        : 0;
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
                else
                {
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.LabelField(new GUIContent("Failed to find parent container"), new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft});
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