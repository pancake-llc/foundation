using System;
using Pancake.ExLibEditor;
using UnityEditor;

namespace Pancake.LevelSystemEditor
{
    using UnityEngine;

    [CustomEditor(typeof(LevelBuilder))]
    internal sealed class LevelBuilderDrawer : Editor
    {
        private SerializedProperty _settingProperty;
        private SerializedProperty _currentLevelProperty;
        private SerializedProperty _levelExtraInfosProperty;
        private Vector2 _scrollPosition;

        private LevelBuilder _builder;
        private int _levelCount;
        private bool _displayMenuOpen;
        private bool _confirmSave;
        private int _levelSelected;

        private void OnEnable()
        {
            _builder = target as LevelBuilder;
            _settingProperty = serializedObject.FindProperty("setting");
            _currentLevelProperty = serializedObject.FindProperty("currentLevel");
            _levelExtraInfosProperty = serializedObject.FindProperty("levelExtraInfos");

            GetLevelCount();
        }

        private void GetLevelCount() { _levelCount = _builder.GetLevelCount(); }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_settingProperty);

            if (_confirmSave)
            {
                GUI.color = Uniform.FieryRose;
                GUILayout.Label($"Save current scene in to level :{_builder.CurrentLevel}");
                GUI.color = Color.white;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Back"))
                {
                    _confirmSave = false;
                }

                if (GUILayout.Button("Save"))
                {
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                Uniform.DrawGroupFoldout("level_builder_select_level",
                    "Level Collection",
                    () =>
                    {
                        if (_levelSelected > 0) // open, delete options
                        {
                            EditorGUILayout.LabelField("Level Selected: " + _levelSelected);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Back"))
                            {
                                _levelSelected = 0;
                            }

                            if (GUILayout.Button("Open"))
                            {
                                _builder.OpenLevel(_levelSelected);
                                _levelSelected = 0;
                            }

                            if (GUILayout.Button("Delete"))
                            {
                                _builder.DeleteLevel(_levelSelected);
                                _levelSelected = 0;
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            // levels panel
                            if (_levelCount > 0)
                            {
                                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, new GUIStyle(GUI.skin.box), GUILayout.MaxHeight(300));
                                int i = 1;
                                while (i <= _levelCount)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    for (int j = 0; j < 5; j++)
                                    {
                                        if (i > _levelCount)
                                            break;
                                        if (GUILayout.Button(i.ToString(), GUILayout.MaxWidth(110)))
                                        {
                                            _levelSelected = i;
                                        }

                                        i++;
                                    }

                                    EditorGUILayout.EndHorizontal();
                                }

                                EditorGUILayout.EndScrollView();
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No levels saved yet");
                            }
                        }
                    });
            }

            EditorGUILayout.PropertyField(_levelExtraInfosProperty);
            EditorGUILayout.PropertyField(_currentLevelProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}