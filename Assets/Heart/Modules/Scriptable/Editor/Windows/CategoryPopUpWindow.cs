using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    public class CategoryPopUpWindow : PopupWindowContent
    {
        private readonly Rect _position;

        private readonly Vector2 _dimensions = new Vector2(350, 350);

        private readonly GUIStyle _buttonIconStyle;
        private readonly float _lineHeight = 20f;
        private readonly List<ScriptableBase> _scriptableBases;
        private Vector2 _scrollPosition = Vector2.zero;
        private readonly Texture[] _icons;
        private bool _isAddingNewCategory;
        private int _categoryBeingRenamed = -1;
        private int _categoryBeingDeleted = -1;
        private string _categoryName;

        public override Vector2 GetWindowSize() => _dimensions;

        public CategoryPopUpWindow(Rect origin)
        {
            _position = origin;
            _scriptableBases = ProjectDatabase.FindAll<ScriptableBase>();
            _buttonIconStyle = new GUIStyle(GUI.skin.button) {padding = new RectOffset(4, 4, 4, 4)};
            _icons = new[] {EditorResources.IconEdit, EditorResources.IconDelete, EditorGUIUtility.IconContent("Warning").image, EditorResources.IconCancel};
        }

        public override void OnGUI(Rect rect)
        {
            editorWindow.position = Uniform.CenterInWindow(editorWindow.position, _position);
            //Uniform.DrawHeader("Categoties");
            GUILayout.Space(10f);
            DrawCategories();
            GUILayout.Space(10f);
            DrawButtons();
        }

        private void DrawCategories()
        {
            GUI.enabled = !_isAddingNewCategory;
            EditorGUILayout.BeginVertical();

            //Draw the default category
            var labelStyle = new GUIStyle(GUI.skin.box) {alignment = TextAnchor.MiddleLeft, normal = {textColor = new Color(0.7f, 0.7f, 0.7f, 1f)}};
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.LabelField("Default", labelStyle, GUILayout.ExpandWidth(true));

            //Draw the rest of the categories
            for (int i = 1; i < ScriptableEditorSetting.Categories.Count; i++)
            {
                if (i == _categoryBeingRenamed) DrawCategoryBeingRenamed(i);
                else if (i == _categoryBeingDeleted) DrawCategoryBeingDeleted(i);
                else DrawDefaultCategoryEntry(i);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void DrawButtons()
        {
            if (_isAddingNewCategory) DrawNewCategoryBeingAdded();
            else
            {
                if (HasReachedMaxCategory)
                {
                    var labelStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.red}, alignment = TextAnchor.MiddleCenter};
                    EditorGUILayout.LabelField("Maximum Amount of Categories reached (32)", labelStyle);
                }
                else
                {
                    DrawAddNewCategoryButton();
                }
            }

            if (GUILayout.Button("Close", GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT))) editorWindow.Close();

            void DrawNewCategoryBeingAdded()
            {
                EditorGUILayout.BeginHorizontal();

                DrawRenameLayout(() =>
                    {
                        _isAddingNewCategory = false;
                        ScriptableEditorSetting.Categories.Add(_categoryName);
                    },
                    () => { _isAddingNewCategory = false; });

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            void DrawAddNewCategoryButton()
            {
                GUI.enabled = IsAllowedToCreateNewCategory;
                if (GUILayout.Button("Add New Category", GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
                {
                    _categoryName = "";
                    _isAddingNewCategory = true;
                }

                GUI.enabled = true;
            }
        }

        private void DrawCategoryBeingRenamed(int index)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            DrawRenameLayout(() =>
                {
                    _categoryBeingRenamed = -1;
                    ScriptableEditorSetting.Categories[index] = _categoryName;
                },
                () => { _categoryBeingRenamed = -1; });
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryBeingDeleted(int i)
        {
            if (IsCategoryUsed(i, out var usedBy))
            {
                DrawUseCategoryBeingDeleted();
            }
            else
            {
                DrawConfirmDelete();
            }

            void DrawUseCategoryBeingDeleted()
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                //Warning icon
                var iconStyle = new GUIStyle(GUIStyle.none);
                iconStyle.margin = new RectOffset(0, 0, 5, 0);
                GUILayout.Box(_icons[2], iconStyle, GUILayout.Width(_lineHeight), GUILayout.Height(_lineHeight));

                //Label
                EditorGUILayout.LabelField($"{ScriptableEditorSetting.Categories[i]} can't be deleted because it's used by:");

                //Close Button
                if (GUILayout.Button(_icons[3], _buttonIconStyle, GUILayout.Width(_lineHeight), GUILayout.Height(_lineHeight))) _categoryBeingDeleted = -1;

                EditorGUILayout.EndHorizontal();

                //Draw the list of ScriptableBases that use this category
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red.Lighten(.75f);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                foreach (var scriptableBase in usedBy)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    EditorGUILayout.LabelField(scriptableBase.name);
                    var categories = ScriptableEditorSetting.Categories.ToArray();

                    EditorGUI.BeginChangeCheck();
                    int newCategoryIndex = EditorGUILayout.Popup(scriptableBase.categoryIndex, categories);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(scriptableBase, "Change Category");
                        scriptableBase.categoryIndex = newCategoryIndex;
                        EditorUtility.SetDirty(scriptableBase);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUI.backgroundColor = originalColor;
                if (GUILayout.Button("Set Default category for all", GUILayout.Height(_lineHeight)))
                {
                    foreach (var scriptableBase in usedBy)
                    {
                        scriptableBase.categoryIndex = 0;
                        EditorUtility.SetDirty(scriptableBase);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            void DrawConfirmDelete()
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.LabelField($"Confirm Delete {ScriptableEditorSetting.Categories[i]}?");
                if (GUILayout.Button("Yes", GUILayout.Width(35), GUILayout.Height(_lineHeight)))
                {
                    ScriptableEditorSetting.Categories.RemoveAt(i);
                    EditorUtility.SetDirty(ScriptableEditorSetting.Instance);
                    _categoryBeingDeleted = -1;
                }

                if (GUILayout.Button("No", GUILayout.Width(35), GUILayout.Height(_lineHeight))) _categoryBeingDeleted = -1;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawDefaultCategoryEntry(int i)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField(ScriptableEditorSetting.Categories[i]);
            if (GUILayout.Button(_icons[0], _buttonIconStyle, GUILayout.Width(_lineHeight), GUILayout.Height(_lineHeight)))
            {
                _categoryBeingRenamed = i;
                _categoryName = ScriptableEditorSetting.Categories[i];
            }

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red.Lighten(0.75f);
            if (GUILayout.Button(_icons[1], _buttonIconStyle, GUILayout.Width(_lineHeight), GUILayout.Height(_lineHeight)))
            {
                _categoryBeingDeleted = i;
            }

            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();
        }


        private void DrawRenameLayout(Action onConfirm, Action onCancel)
        {
            _categoryName = EditorGUILayout.TextField(_categoryName, EditorStyles.textField, GUILayout.Height(_lineHeight));
            GUI.enabled = IsNameValid(_categoryName);
            if (GUILayout.Button("Confirm", GUILayout.Width(60), GUILayout.Height(_lineHeight)))
            {
                onConfirm?.Invoke();
                EditorUtility.SetDirty(ScriptableEditorSetting.Instance);
            }

            GUI.enabled = true;
            if (GUILayout.Button("Cancel", GUILayout.Width(60), GUILayout.Height(_lineHeight)))
            {
                _categoryName = "";
                onCancel?.Invoke();
            }
        }

        private bool IsNameValid(string name) { return !string.IsNullOrEmpty(name) && !ScriptableEditorSetting.Categories.Contains(name); }

        private bool IsCategoryUsed(int categoryIndex, out List<ScriptableBase> usedBy)
        {
            usedBy = _scriptableBases.Where(x => x.categoryIndex == categoryIndex).ToList();
            return usedBy.Count > 0;
        }

        private bool HasReachedMaxCategory => ScriptableEditorSetting.Categories.Count >= 32;

        private bool IsAllowedToCreateNewCategory => _categoryBeingDeleted == -1 && _categoryBeingRenamed == -1;
    }
}