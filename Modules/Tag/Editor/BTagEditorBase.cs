using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pancake.BTag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pancake.BTagEditor
{
    public class BTagEditorBase : Editor
    {
        [NonSerialized]
        // Used for a visual highlight to help editor navigation of assets
        public static int SentFromInstanceId = 0;

        static UnityEngine.Object lastSearchObject = null;
        List<SceneObjectIDBundle> openSceneReferences;
        List<SceneObjectIDBundle> prefabReferences;
        List<SceneObjectIDBundle> otherAssets;
        List<SceneObjectIDBundle> otherSceneReferences;

        private void ShowSettings() { SettingsService.OpenUserPreferences("Preferences/BTagSettings"); }

        private void OnEnable()
        {
            if (lastSearchObject == null || lastSearchObject != target || !ScriptableBTagRegistry.HasCachedResults(target as ScriptableBTag))
            {
                RefreshUses(BTagSetting.Instance.searchMode);
                lastSearchObject = target;
            }
            else
            {
                RefreshUses(SearchRegistryOption.IterativeRefresh);
            }
        }

        private void OnDestroy()
        {
            SentFromInstanceId = 0;
            lastOpenSceneRefPinged = -1;
        }

        bool refresh = false;
        int lastOpenSceneRefPinged = -1;

        public override void OnInspectorGUI()
        {
            Color defaultGUIColor = GUI.color;
            GUIContent pickerBtnContent = new GUIContent(EditorGUIUtility.FindTexture("Record Off@2x"), "Hover to Ping, click to Select");
            GUIStyle pickerBtnStyle = new GUIStyle(GUI.skin.button);
            pickerBtnStyle.padding = new RectOffset(0, 0, 3, 3);
            GUIStyle hashStyle = new GUIStyle(GUI.skin.label);
            hashStyle.alignment = TextAnchor.MiddleRight;
            hashStyle.wordWrap = true;
            hashStyle.richText = true;

            if (openSceneReferences != null)
            {
                GUILayoutUtility.GetRect(0, 0);
                Rect r = GUILayoutUtility.GetLastRect();
                r.x = 0;
                r.width = EditorGUIUtility.currentViewWidth;
                //r.y += EditorGUIUtility.standardVerticalSpacing;
                r.height = EditorGUIUtility.standardVerticalSpacing + (2.5f * EditorGUIUtility.singleLineHeight);
                GUILayoutUtility.GetRect(r.width, 4f);
                EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.1f));

                EditorGUILayout.BeginHorizontal();
                var group = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as BTagGroupBase;
                string existingGroupName = (group == null ? string.Empty : group.name + "/");
                GUIContent groupLabel = new GUIContent(existingGroupName);

                string nameOfSelected = target.name;
                bool hasDir = nameOfSelected.Contains("/");
                if (serializedObject.isEditingMultipleObjects)
                {
                    for (int i = 0; i < serializedObject.targetObjects.Length; ++i)
                    {
                        if (!(serializedObject.targetObjects[i] is ScriptableBTag)) continue;
                        var tn = serializedObject.targetObjects[i].name;
                        if (hasDir)
                        {
                            while (!tn.StartsWith(nameOfSelected) && nameOfSelected.Contains("/"))
                                nameOfSelected = nameOfSelected.Substring(0, nameOfSelected.LastIndexOf("/"));
                        }
                        else
                        {
                            while (!tn.StartsWith(nameOfSelected) && nameOfSelected.Length > 0) nameOfSelected = nameOfSelected.Substring(0, nameOfSelected.Length - 1);
                        }
                    }
                }

                float defaultLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = GUI.skin.textField.CalcSize(groupLabel).x;
                EditorGUILayout.PrefixLabel(groupLabel);
                EditorGUIUtility.labelWidth = defaultLabelWidth;
                string newAssetName = EditorGUILayout.DelayedTextField(nameOfSelected);

                if (newAssetName != nameOfSelected)
                {
                    var allTargs = new List<UnityEngine.Object>(serializedObject.targetObjects);
                    for (int i = 0; i < allTargs.Count; ++i)
                    {
                        if (!(allTargs[i] is ScriptableBTag)) continue;
                        var renameGroup = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(allTargs[i])) as BTagGroupBase;
                        string renameGroupName = (renameGroup == null ? string.Empty : renameGroup.name + "/");
                        var newName = string.Empty;
                        if (string.IsNullOrEmpty(nameOfSelected))
                        {
                            newName = renameGroupName + newAssetName + allTargs[i].name;
                        }
                        else
                        {
                            var regex = new Regex(Regex.Escape(nameOfSelected));
                            newName = renameGroupName + regex.Replace(allTargs[i].name, newAssetName, 1);
                        }

                        BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.RenameSoAsset(renameGroup, allTargs[i] as ScriptableBTag, newName);
                    }
                }

                if (group != null)
                {
                    GUIContent btnContent = new GUIContent(ProjectDatabase.FindAssetWithPath<Texture2D>("tag_btn_delete.png", BTagEditorUtils.RELATIVE_PATH));
                    GUIStyle deleteBtnStyle = new GUIStyle(GUI.skin.button);
                    deleteBtnStyle.padding = new RectOffset();
                    if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.black;
                    if (GUILayout.Button(btnContent, deleteBtnStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Deleting " + target, "Are you sure you wish to delete?", "Ok", "Cancel"))
                        {
                            if (group != null) Selection.activeObject = group;
                            BTagPropertyDrawerBase<BTagGroupBase, ScriptableBTag>.TryDeleteSo(target as ScriptableBTag);
                        }

                        GUIUtility.ExitGUI();
                    }

                    if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();

                if (BTagSetting.Instance.showHashes)
                {
                    var hashContent = new GUIContent("<color=grey><i>#" + (target as ScriptableBTag).Hash.ToString() + "</i></color>",
                        "Click to copy hash.\n\nAs int4:\nx:" + (target as ScriptableBTag).Hash.x + "\n" + "y:" + (target as ScriptableBTag).Hash.y + "\n" + "z:" +
                        (target as ScriptableBTag).Hash.z + "\n" + "w:" + (target as ScriptableBTag).Hash.w);
                    bool copyHash = GUILayout.Button(hashContent, hashStyle);
                    if (copyHash)
                    {
                        TextEditor te = new TextEditor();
                        te.text = (target as ScriptableBTag).Hash.ToString();
                        te.SelectAll();
                        te.Copy();
                    }
                }

                EditorGUI.indentLevel--;
                DrawDivider();
                EditorGUI.indentLevel++;

                EditorGUILayout.Space();
                GUIStyle lblStyle = new GUIStyle(GUI.skin.label);
                lblStyle.richText = true;

                var show = EditorGUILayout.BeginFoldoutHeaderGroup(BTagSetting.Instance.showAssetReferences, "Project References");
                if (show != BTagSetting.Instance.showAssetReferences)
                {
                    BTagSetting.Instance.showAssetReferences = show;
                    if (show) RefreshUses(SearchRegistryOption.FullRefresh);
                }

                if (BTagSetting.Instance.showAssetReferences)
                {
                    bool empty = true;
                    if (openSceneReferences != null && openSceneReferences.Count > 0)
                    {
                        empty = false;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b><color=cyan>In Open Scenes:</color> <color=yellow>" + openSceneReferences.Count + "</color> reference(s)</b>",
                            lblStyle);
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < openSceneReferences.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (SentFromInstanceId == openSceneReferences[i].id) GUI.color = Color.yellow;
                            if (GUILayout.Button(pickerBtnContent, pickerBtnStyle, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                Selection.activeInstanceID = openSceneReferences[i].id;
                                BTagPropertyDrawerBase.sentFromInstanceId = target.GetInstanceID();
                            }

                            GUI.color = defaultGUIColor;
                            if (GUILayout.Button(openSceneReferences[i].objectName, lblStyle))
                            {
                                lastOpenSceneRefPinged = i;
                                EditorGUIUtility.PingObject(openSceneReferences[i].id);
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                    }

                    if (prefabReferences != null && prefabReferences.Count > 0)
                    {
                        empty = false;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b><color=cyan>Prefabs:</color> <color=yellow>" + prefabReferences.Count + "</color> reference(s)</b>", lblStyle);
                        DrawDivider();
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < prefabReferences.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(pickerBtnContent, pickerBtnStyle, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(prefabReferences[i].scenePath));
                            }

                            if (GUILayout.Button(Path.GetFileName(prefabReferences[i].scenePath) + " : " + prefabReferences[i].objectName, lblStyle))
                            {
                                lastOpenSceneRefPinged = i + openSceneReferences.Count;
                                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(prefabReferences[i].scenePath));
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }

                    if (otherAssets != null && otherAssets.Count > 0)
                    {
                        empty = false;
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b><color=cyan>Other Assets:</color> <color=yellow>" + otherAssets.Count + "</color> reference(s)</b>", lblStyle);
                        DrawDivider();
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < otherAssets.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (SentFromInstanceId == otherAssets[i].id) GUI.color = Color.yellow;
                            if (GUILayout.Button(pickerBtnContent, pickerBtnStyle, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                Selection.activeInstanceID = otherAssets[i].id;
                                BTagPropertyDrawerBase.sentFromInstanceId = target.GetInstanceID();
                            }

                            GUI.color = defaultGUIColor;
                            if (GUILayout.Button(Path.GetFileName(otherAssets[i].scenePath), lblStyle))
                            {
                                lastOpenSceneRefPinged = i;
                                EditorGUIUtility.PingObject(otherAssets[i].id);
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }

                    if (otherSceneReferences != null && otherSceneReferences.Count > 0)
                    {
                        empty = false;
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("<b><color=cyan>Other Scenes:</color> <color=yellow>" + otherSceneReferences.Count + "</color> reference(s)</b>",
                            lblStyle);
                        DrawDivider();
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < otherSceneReferences.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(pickerBtnContent, pickerBtnStyle, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                EditorSceneManager.OpenScene(otherSceneReferences[i].scenePath, OpenSceneMode.Single);
                            }

                            if (GUILayout.Button(Path.GetFileName(otherSceneReferences[i].scenePath) + " : " + otherSceneReferences[i].id + " uses", lblStyle))
                            {
                                lastOpenSceneRefPinged = i + openSceneReferences.Count + prefabReferences.Count;
                                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(otherSceneReferences[i].scenePath));
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }

                    if (empty)
                    {
                        EditorGUILayout.LabelField(" - No References Found -");
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                EditorGUILayout.Space();
                //EditorGUI.indentLevel--;
                //DrawDivider();
                //EditorGUI.indentLevel++;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Find All References", GUILayout.Width(150)))
            {
                BTagSetting.Instance.showAssetReferences = true;
                refresh = true;
            }

            if (GUILayout.Button(EditorGUIUtility.FindTexture("_Popup"))) ShowSettings();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (refresh && Event.current.type == EventType.Repaint)
            {
                RefreshUses(SearchRegistryOption.FullRefresh);
                refresh = false;
            }
        }

        private void DrawDivider()
        {
            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            Handles.DrawLine(new Vector2(rect.x - 2f + (EditorGUI.indentLevel * 15), rect.y), new Vector2(rect.width + 6f - (EditorGUI.indentLevel * 14), rect.y));
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshUses(SearchRegistryOption searchOption)
        {
            if (!BTagSetting.Instance.showAssetReferences)
            {
                openSceneReferences = new List<SceneObjectIDBundle>();
                return;
            }

            if (searchOption != SearchRegistryOption.CachedResultsOnly)
            {
                ScriptableBTagRegistry.OnSearchProgress = UpdateProgressBar;
                ScriptableBTagRegistry.OnOpenSceneSearchProgress = UpdateOpenSceneProgressBar;
                //EditorUtility.DisplayProgressBar("Searching Project", "Finding references to " + target, 0);
            }

            try
            {
                int sceneCount = EditorSceneManager.sceneCount;
                string[] originalScenes = new string[sceneCount];
                for (int s = 0; s < sceneCount; ++s) originalScenes[s] = EditorSceneManager.GetSceneAt(s).path;

                var allReferences = new List<SceneObjectIDBundle>();
                ScriptableBTagRegistry.References((target as ScriptableBTag), ref allReferences, searchOption);

                prefabReferences = new List<SceneObjectIDBundle>();
                openSceneReferences = new List<SceneObjectIDBundle>();
                otherAssets = new List<SceneObjectIDBundle>();
                otherSceneReferences = new List<SceneObjectIDBundle>();

                // Distribute all references into appropriate categories
                for (int i = allReferences.Count - 1; i >= 0; --i)
                {
                    // This happens when a Prefab is open for editing/staged 
                    if (string.IsNullOrEmpty(allReferences[i].scenePath)) continue;

                    if (allReferences[i].scenePath.EndsWith(".prefab"))
                    {
                        prefabReferences.Add(allReferences[i]);
                    }
                    else if (allReferences[i].scenePath.EndsWith(".asset"))
                    {
                        otherAssets.Add(allReferences[i]);
                    }
                    else if (originalScenes.Contains(allReferences[i].scenePath))
                    {
                        openSceneReferences.Add(allReferences[i]);
                    }
                    else
                    {
                        otherSceneReferences.Add(allReferences[i]);
                    }
                }

                // Group other scene references by their path and replace them with a new bundle containing the count
                otherSceneReferences = otherSceneReferences
                    .GroupBy(x => x.scenePath, (scene, sceneRefs) => new SceneObjectIDBundle(scene, string.Empty, sceneRefs.Count()))
                    .ToList();
            }
            finally
            {
                if (searchOption != SearchRegistryOption.CachedResultsOnly)
                {
                    ScriptableBTagRegistry.OnSearchProgress = null;
                    ScriptableBTagRegistry.OnOpenSceneSearchProgress = null;
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void UpdateProgressBar(float current, float total)
        {
            if (target == null) return;
            float progress = Mathf.Max(current, 1f) / Mathf.Max(total, 1f);
            bool cancel = EditorUtility.DisplayCancelableProgressBar("Searching Project", "Finding references to " + target + " " + current + "/" + total, progress);
            if (cancel) ScriptableBTagRegistry.CancelSearch = true;
        }

        private void UpdateOpenSceneProgressBar(float current, float total)
        {
            if (target == null) return;
            float progress = Mathf.Max(current, 1f) / Mathf.Max(total, 1f);
            bool cancel = EditorUtility.DisplayCancelableProgressBar("Searching Open Scenes", "Finding references to " + target + " " + current + "/" + total, progress);
            if (cancel) ScriptableBTagRegistry.CancelSearch = true;
        }
    }
}