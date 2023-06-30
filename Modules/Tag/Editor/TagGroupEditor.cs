using System.IO;
using Pancake.Tag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomEditor(typeof(TagGroupBase), true)]
    [CanEditMultipleObjects]
    public class TagGroupEditor : Editor
    {
        private void ShowSettings() { SettingsService.OpenUserPreferences("Preferences/Pancake/TagSettings"); }

        Vector2 scrollPos = Vector2.zero;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(target +
                                    "\nSelect any of the children (sub assets) to see where they are used.\n\nCreate a new Group via the Tag drop-down menu, by Selecting one or more assets and grouping them (Cmd+G) or via the project window's context menu.",
                MessageType.Info);

            GUIStyle deleteBtnStyle = new GUIStyle(GUI.skin.button);
            deleteBtnStyle.padding = new RectOffset();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            string newGroupName = EditorGUILayout.TextField(target.name);
            if (newGroupName != target.name)
            {
                TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.RenameGroup((target as TagGroupBase), newGroupName);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Children:");
            Rect r = EditorGUILayout.GetControlRect(false, 0);
            r.y += EditorGUIUtility.standardVerticalSpacing;
            r.height = 200;
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.1f));

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            GUIContent pickerBtnContent = new GUIContent(EditorGUIUtility.FindTexture("Record Off@2x"), "Click to select the asset associated with this " + target.name);
            GUIStyle pickerBtnStyle = new GUIStyle(GUI.skin.button);
            pickerBtnStyle.alignment = TextAnchor.MiddleLeft;
            pickerBtnStyle.padding = new RectOffset(3, 3, 3, 3);
            pickerBtnStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(target));
            int assetToBeginEditing = -1;
            for (int s = 0; s < subAssets.Length; ++s)
            {
                if (subAssets[s] != null && subAssets[s] is ScriptableTag)
                {
                    string tfControlName = "edit_tf_" + s;
                    EditorGUILayout.BeginHorizontal();
                    pickerBtnContent.text = "  " + subAssets[s].name;
                    pickerBtnContent.tooltip = "Click to select " + subAssets[s];
                    bool isEditing = TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.GetIsEdit(serializedObject, s.ToString());
                    bool shouldEdit = TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.GetShouldEdit(serializedObject, s.ToString());
                    if (shouldEdit)
                    {
                        GUI.SetNextControlName(tfControlName);

                        string newAssetName =
                            EditorGUILayout.TextField(TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.GetEditString(serializedObject, s.ToString()));
                        TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetEditString(serializedObject, s.ToString(), newAssetName);
                        if (!isEditing)
                        {
                            if (Event.current.type == EventType.Layout) EditorGUI.FocusTextInControl(tfControlName);
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetIsEdit(serializedObject, s.ToString(), true);
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetEditString(serializedObject, s.ToString(), subAssets[s].name);
                        }

                        if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || GUI.GetNameOfFocusedControl() != tfControlName))
                        {
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetShouldEdit(serializedObject, s.ToString(), false);
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetIsEdit(serializedObject, s.ToString(), false);
                            newAssetName = TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.GetEditString(serializedObject, s.ToString());
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.RenameSoAsset((target as TagGroupBase),
                                subAssets[s] as ScriptableTag,
                                target.name + "/" + newAssetName);
                        }
                        else if (Event.current.keyCode == KeyCode.Escape)
                        {
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetShouldEdit(serializedObject, s.ToString(), false);
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetIsEdit(serializedObject, s.ToString(), false);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(pickerBtnContent, pickerBtnStyle))
                        {
                            Selection.activeObject = (subAssets[s] as ScriptableTag);
                        }
                    }

                    if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.black;
                    if (GUILayout.Button(ProjectDatabase.FindAssetWithPath<Texture2D>("tag_btn_edit.png", TagEditorUtils.RELATIVE_PATH),
                            deleteBtnStyle,
                            GUILayout.Width(20),
                            GUILayout.Height(20)))
                    {
                        if (!isEditing) assetToBeginEditing = s;
                        EditorGUI.FocusTextInControl(null);
                    }

                    if (GUILayout.Button(ProjectDatabase.FindAssetWithPath<Texture2D>("tag_btn_delete.png", TagEditorUtils.RELATIVE_PATH),
                            deleteBtnStyle,
                            GUILayout.Width(20),
                            GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Deleting " + subAssets[s].name,
                                "Are you sure you wish to delete " + target.name + "/" + subAssets[s].name + "?",
                                "Ok",
                                "Cancel"))
                        {
                            TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.TryDeleteSo(subAssets[s] as ScriptableTag);
                        }

                        GUIUtility.ExitGUI();
                    }

                    if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (assetToBeginEditing >= 0)
            {
                string p = assetToBeginEditing.ToString();
                EditorApplication.delayCall += () => TagPropertyDrawerBase<TagGroupBase, ScriptableTag>.SetShouldEdit(serializedObject, p, true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add")) TagEditorUtils.ShowAddToGroupPopup(target as TagGroupBase, string.Empty, null);
            GUIContent lbl = new GUIContent("Tag Settings", EditorGUIUtility.FindTexture("_Popup"));
            EditorGUILayout.Space();
            if (GUILayout.Button(lbl)) ShowSettings();
            EditorGUILayout.EndHorizontal();
        }
    }
}