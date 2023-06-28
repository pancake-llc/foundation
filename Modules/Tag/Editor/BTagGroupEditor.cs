using System.IO;
using Pancake.BTag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomEditor(typeof(BTagGroupBase), true)]
    [CanEditMultipleObjects]
    public class BTagGroupEditor : Editor
    {
        private void ShowSettings() { SettingsService.OpenUserPreferences("Preferences/BTagSettings"); }

        Vector2 scrollPos = Vector2.zero;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(target +
                                    "\nSelect any of the children (sub assets) to see where they are used.\n\nCreate a new Group via the BTag drop-down menu, by Selecting one or more assets and grouping them (Cmd+G) or via the project window's context menu.",
                MessageType.Info);

            GUIStyle deleteBtnStyle = new GUIStyle(GUI.skin.button);
            deleteBtnStyle.padding = new RectOffset();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            string newGroupName = EditorGUILayout.TextField(target.name);
            if (newGroupName != target.name)
            {
                BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.RenameGroup((target as BTagGroupBase), newGroupName);
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
                if (subAssets[s] != null && subAssets[s] is ScriptableBTag)
                {
                    string tfControlName = "edit_tf_" + s;
                    EditorGUILayout.BeginHorizontal();
                    pickerBtnContent.text = "  " + subAssets[s].name;
                    pickerBtnContent.tooltip = "Click to select " + subAssets[s];
                    bool isEditing = BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.GetIsEdit(serializedObject, s.ToString());
                    bool shouldEdit = BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.GetShouldEdit(serializedObject, s.ToString());
                    if (shouldEdit)
                    {
                        GUI.SetNextControlName(tfControlName);

                        string newAssetName =
                            EditorGUILayout.TextField(BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.GetEditString(serializedObject, s.ToString()));
                        BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetEditString(serializedObject, s.ToString(), newAssetName);
                        if (!isEditing)
                        {
                            if (Event.current.type == EventType.Layout) EditorGUI.FocusTextInControl(tfControlName);
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetIsEdit(serializedObject, s.ToString(), true);
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetEditString(serializedObject, s.ToString(), subAssets[s].name);
                        }

                        if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || GUI.GetNameOfFocusedControl() != tfControlName))
                        {
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetShouldEdit(serializedObject, s.ToString(), false);
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetIsEdit(serializedObject, s.ToString(), false);
                            newAssetName = BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.GetEditString(serializedObject, s.ToString());
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.RenameSoAsset((target as BTagGroupBase),
                                subAssets[s] as ScriptableBTag,
                                target.name + "/" + newAssetName);
                        }
                        else if (Event.current.keyCode == KeyCode.Escape)
                        {
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetShouldEdit(serializedObject, s.ToString(), false);
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetIsEdit(serializedObject, s.ToString(), false);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(pickerBtnContent, pickerBtnStyle))
                        {
                            Selection.activeObject = (subAssets[s] as ScriptableBTag);
                        }
                    }

                    if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.black;
                    if (GUILayout.Button(ProjectDatabase.FindAssetWithPath<Texture2D>("tag_btn_edit.png", BTagEditorUtils.RELATIVE_PATH),
                            deleteBtnStyle,
                            GUILayout.Width(20),
                            GUILayout.Height(20)))
                    {
                        if (!isEditing) assetToBeginEditing = s;
                        EditorGUI.FocusTextInControl(null);
                    }

                    if (GUILayout.Button(ProjectDatabase.FindAssetWithPath<Texture2D>("tag_btn_delete.png", BTagEditorUtils.RELATIVE_PATH),
                            deleteBtnStyle,
                            GUILayout.Width(20),
                            GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Deleting " + subAssets[s].name,
                                "Are you sure you wish to delete " + target.name + "/" + subAssets[s].name + "?",
                                "Ok",
                                "Cancel"))
                        {
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.TryDeleteSo(subAssets[s] as ScriptableBTag);
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
                EditorApplication.delayCall += () => BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.SetShouldEdit(serializedObject, p, true);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add")) BTagEditorUtils.ShowAddToGroupPopup(target as BTagGroupBase, string.Empty, null);
            GUIContent lbl = new GUIContent("BTag Settings", EditorGUIUtility.FindTexture("_Popup"));
            EditorGUILayout.Space();
            if (GUILayout.Button(lbl)) ShowSettings();
            EditorGUILayout.EndHorizontal();
        }
    }
}