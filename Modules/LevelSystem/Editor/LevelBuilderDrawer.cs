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
        private Rect _rect;
        private int _previousCountInRow = -1;

        private void OnEnable()
        {
            _builder = target as LevelBuilder;
            _settingProperty = serializedObject.FindProperty("setting");
            _currentLevelProperty = serializedObject.FindProperty("currentLevel");
            _levelExtraInfosProperty = serializedObject.FindProperty("levelExtraInfos");

            if (_settingProperty.objectReferenceValue != null) GetLevelCount();
        }

        private void GetLevelCount() { _levelCount = _builder.GetLevelCount(); }

        private float GetViewWidth()
        {
            GUILayout.Label("hack", GUILayout.MaxHeight(0));
            if (Event.current.type == EventType.Repaint)
            {
                // hack to get real view width
                _rect = GUILayoutUtility.GetLastRect();
            }

            return _rect.width;
        }

        public override void OnInspectorGUI()
        {
            void OpenLevel(int level)
            {
                GetLevelCount();
                _builder.CurrentLevel = level;
                _builder.ClearLevel();
                var levelNode = _builder.OpenLevel(level);
                DebugEditor.Toast(levelNode == null ? $"Level {level} can not be found!" : $"Open level {level} success!");
            }

            serializedObject.Update();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_settingProperty, GUILayout.Height(30));
            if (_settingProperty.objectReferenceValue == null)
            {
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.Space(4);
            if (GUILayout.Button(Uniform.IconContent("d_Refresh@2x", "Refresh"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                OpenLevel(_builder.CurrentLevel);
            }

            if (GUILayout.Button(Uniform.IconContent("d_winbtn_win_restore_h@2x", "Open Level Editor"), GUILayout.Width(30), GUILayout.Height(30)))
            {
                var window = EditorWindow.GetWindow<LevelEditor>("Level Editor", true);
                if (window)
                {
                    window.minSize = new Vector2(275, 0);
                    window.Show(true);
                }
            }

            GUILayout.EndHorizontal();

            if (_confirmSave)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Save current scene in to:", GUILayout.Width(155));
                GUI.color = Uniform.FieryRose;
                GUILayout.Label($"Level {_builder.CurrentLevel}", new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold});
                GUI.color = Color.white;

                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Uniform.Purple;
                if (GUILayout.Button("Back", GUILayout.Height(40)))
                {
                    _confirmSave = false;
                }

                GUI.backgroundColor = Uniform.FieryRose;
                if (GUILayout.Button("Save", GUILayout.Height(40)))
                {
                    _builder.Save();
                    GetLevelCount();
                    _confirmSave = false;
                }

                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(_levelExtraInfosProperty);
                GUI.color = Uniform.Green;
                EditorGUILayout.PropertyField(_currentLevelProperty);
                GUI.color = Color.white;

                GUILayout.Space(4);
                Uniform.DrawLine();
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.white;
                if (_builder.CurrentLevel <= 1) GUI.enabled = false;
                if (GUILayout.Button("Load Previous Level", GUILayout.Height(40)))
                {
                    OpenLevel(_builder.CurrentLevel - 1);
                }

                GUI.enabled = true;
                GUI.backgroundColor = Uniform.FieryRose;
                if (GUILayout.Button("Save Level", GUILayout.Height(40)))
                {
                    _confirmSave = true;
                }

                if (_builder.CurrentLevel >= _levelCount) GUI.enabled = false;
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Load Next Level", GUILayout.Height(40)))
                {
                    OpenLevel(_builder.CurrentLevel + 1);
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                Uniform.DrawLine();
                GUILayout.Space(4);

                float _ = GetViewWidth();
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);
                Uniform.DrawGroupFoldout("level_builder_select_level", "Level Collection", DrawLevelCollectionArea);
                GUILayout.EndScrollView();
            }


            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLevelCollectionArea()
        {
            if (_levelSelected > 0)
            {
                DrawMenuLevelSelected();
            }
            else
            {
                DrawLevelCollection();
            }
        }

        private void DrawLevelCollection()
        {
            // levels panel
            if (_levelCount > 0)
            {
                var i = 1;
                while (i <= _levelCount)
                {
                    GUILayout.BeginHorizontal();
                    int countInRow = (int) _rect.width / 80;
                    if (countInRow == 0) return;

                    if (_previousCountInRow != countInRow)
                    {
                        if (Event.current.type != EventType.Layout) _previousCountInRow = countInRow;
                        Repaint();
                        return;
                    }

                    for (int j = 0; j < countInRow; j++)
                    {
                        if (i > _levelCount) break;

                        GUI.backgroundColor = i == _currentLevelProperty.intValue ? Uniform.Green : Color.white;
                        if (GUILayout.Button($"Level {i}", GUILayout.Width(85), GUILayout.Height(60)))
                        {
                            _levelSelected = i;
                        }

                        GUI.backgroundColor = Color.white;

                        i++;
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No levels saved yet");
            }
        }

        private void DrawMenuLevelSelected()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level Selected: ", GUILayout.Width(90));
            GUI.color = Uniform.Green;
            GUILayout.Label($"Level {_levelSelected}", new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold});
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Uniform.Purple;
            if (GUILayout.Button("Back", GUILayout.Height(40))) _levelSelected = 0;

            GUI.backgroundColor = Uniform.Green;
            if (GUILayout.Button("Open", GUILayout.Height(40)))
            {
                _builder.OpenLevel(_levelSelected);
                _levelSelected = 0;
            }

            GUI.backgroundColor = Uniform.FieryRose;
            if (GUILayout.Button("Delete", GUILayout.Height(40)))
            {
                const string msg = "Are you sure to delete the level? The next levels will be adjusted to fill the empty space";
                if (EditorUtility.DisplayDialog("Confirm Delete", msg, "OK", "Cancel"))
                {
                    _builder.DeleteLevel(_levelSelected);
                    GetLevelCount();
                }

                _levelSelected = 0;
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}