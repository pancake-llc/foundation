using System;
using System.Linq;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PancakeEditor.ContextMenu
{
    [InitializeOnLoad]
    internal static class HierarchyMenuCustomizer
    {
        private static HierarchyMenuSettings settings;


        static HierarchyMenuCustomizer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        }

        private static void OnPrefabStageOpened(PrefabStage stage) { sessionParentCache = null; }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { sessionParentCache = null; }

        private static HierarchyMenuSettings GetSettings()
        {
            if (settings == null)
            {
                settings = AssetDatabase.FindAssets("t:HierarchyMenuSettings")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<HierarchyMenuSettings>)
                    .FirstOrDefault();
            }

            return settings;
        }

        private static GameObject sessionParentCache;


        private static void OnGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current.type != EventType.ContextClick) return;

            var s = GetSettings();

            if (s == null) return;

            Event.current.Use();

            var list = s.List;
            var genericMenu = new GenericMenu();

            for (var i = 0; i < list.Count; i++)
            {
                var data = list[i];

                if (data.IsSeparator)
                {
                    genericMenu.AddSeparator(data.Name);
                }
                else
                {
                    string name = data.Name;
                    string menuItemPath = data.MenuItemPath;
                    var content = new GUIContent(name);

                    if (name == "Select Children")
                    {
                        var flag = false;
                        foreach (var o in Selection.gameObjects)
                        {
                            if (o.transform.childCount > 0) flag = true;
                        }

                        if (!flag)
                        {
                            genericMenu.AddDisabledItem(content: content, @on: false);
                        }
                        else
                        {
                            genericMenu.AddItem(content: content, @on: false, func: () => EditorApplication.ExecuteMenuItem(menuItemPath));
                        }
                    }
                    else if (name == "Set Default Parent")
                    {
                        if (Selection.gameObjects.Length <= 0) continue;
                        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            // in prefab stage
                            if (Selection.gameObjects[^1] == PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot) continue;
                        }

                        if (sessionParentCache == Selection.gameObjects[^1]) continue;

                        genericMenu.AddItem(content: content,
                            @on: false,
                            func: () =>
                            {
                                sessionParentCache = Selection.gameObjects[^1];
                                EditorUtility.SetDefaultParentObject(Selection.gameObjects[^1]);
                            });
                    }
                    else if (name == "Clear Default Parent")
                    {
                        if (Selection.gameObjects.Length <= 0) continue;
                        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            // in prefab stage
                            if (Selection.gameObjects[^1] == PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot) continue;
                        }

                        if (Selection.gameObjects[^1] == sessionParentCache)
                        {
                            genericMenu.AddItem(content: content,
                                @on: false,
                                func: () =>
                                {
                                    sessionParentCache = null;
                                    EditorUtility.ClearDefaultParentObject();
                                });
                        }
                    }
                    else if (name.StartsWith("Prefab/"))
                    {
                        if (PrefabDatabase.HasPrefabInSelection())
                        {
                            if (name == "Prefab/Replace...")
                            {
                                genericMenu.AddItem(content: content,
                                    @on: false,
                                    func: () =>
                                    {
                                        foreach (var o in Selection.gameObjects)
                                        {
                                        }
                                    });
                            }
                            else if (name == "Prefab/Unpack")
                            {
                                foreach (var gameObject in Selection.gameObjects)
                                {
                                    if (PrefabDatabase.IsPrefabRoot(gameObject))
                                    {
                                        genericMenu.AddItem(content: content,
                                            @on: false,
                                            func: () =>
                                            {
                                                foreach (var o in Selection.gameObjects)
                                                {
                                                    if (PrefabDatabase.IsPrefabRoot(o))
                                                        PrefabUtility.UnpackPrefabInstance(o, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                                                }
                                            });
                                    }
                                }
                            }
                            else if (name == "Prefab/Unpack Completely")
                            {
                                foreach (var gameObject in Selection.gameObjects)
                                {
                                    if (PrefabDatabase.IsPrefabRoot(gameObject))
                                    {
                                        genericMenu.AddItem(content: content,
                                            @on: false,
                                            func: () =>
                                            {
                                                foreach (var o in Selection.gameObjects)
                                                {
                                                    if (PrefabDatabase.IsPrefabRoot(o))
                                                        PrefabUtility.UnpackPrefabInstance(o, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                                                }
                                            });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        genericMenu.AddItem(content: content, @on: false, func: () => EditorApplication.ExecuteMenuItem(menuItemPath));
                    }
                }
            }

            genericMenu.ShowAsContext();
        }
    }
}