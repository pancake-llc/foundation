using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Localization;
using Pancake.LocalizationEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesLocalizationDrawer
    {
        private struct EditorCommands
        {
            public const string DUPLICATE = "Duplicate";
            public const string DELETE = "Delete";
            public const string FRAME_SELECTED = "FrameSelected";
        }

        public static void OnInspectorGUI(
            ref TreeViewState treeViewState,
            ref LocaleTreeView treeView,
            ref MultiColumnHeaderState multiColumnHeaderState,
            Rect bodyViewRect,
            Rect toolBarRect,
            ref SearchField searchField,
            ref bool initialized,
            ref Wizard.LocaleTabType index)
        {
#if PANCAKE_LOCALIZATION
            DrawTab(ref index);
            if (index == 0) DrawTabSetting();
            else
                DrawTabExplore(ref treeViewState,
                    ref treeView,
                    ref multiColumnHeaderState,
                    bodyViewRect,
                    toolBarRect,
                    ref searchField,
                    ref initialized);
#endif

#if !PANCAKE_LOCALIZATION
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Enable Localization", GUILayout.MaxHeight(40f)))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_LOCALIZATION");
                AssetDatabase.Refresh();
                RegistryManager.Resolve();
            }W
            GUI.enabled = true;
#endif
        }

        private static void DrawTab(ref Wizard.LocaleTabType index)
        {
            EditorGUILayout.BeginHorizontal();

            DrawButtonSetting(ref index);
            DrawButtonExplore(ref index);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawButtonSetting(ref Wizard.LocaleTabType index)
        {
            bool clicked = GUILayout.Toggle(index == Wizard.LocaleTabType.Setting, "Settings", GUI.skin.button, GUILayout.ExpandWidth(true));
            if (clicked && index != Wizard.LocaleTabType.Setting)
            {
                index = Wizard.LocaleTabType.Setting;
            }
        }

        private static void DrawButtonExplore(ref Wizard.LocaleTabType index)
        {
            bool clicked = GUILayout.Toggle(index == Wizard.LocaleTabType.Explore, "Explore", GUI.skin.button, GUILayout.ExpandWidth(true));
            if (clicked && index != Wizard.LocaleTabType.Explore)
            {
                index = Wizard.LocaleTabType.Explore;
            }
        }


        private static void DrawTabSetting()
        {
            var setting = Resources.Load<LocaleSettings>(nameof(LocaleSettings));
            if (setting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Localization Settings", GUILayout.Height(40f)))
                {
                    setting = ScriptableObject.CreateInstance<LocaleSettings>();
                    if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(LocaleSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(LocaleSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(LocaleSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editor = UnityEditor.Editor.CreateEditor(setting);
                editor.OnInspectorGUI();
            }
        }

        private static void DrawTabExplore(
            ref TreeViewState treeViewState,
            ref LocaleTreeView treeView,
            ref MultiColumnHeaderState multiColumnHeaderState,
            Rect bodyViewRect,
            Rect toolBarRect,
            ref SearchField searchField,
            ref bool initialized)
        {
            InitializeIfNeeded(ref treeViewState,
                ref treeView,
                ref multiColumnHeaderState,
                bodyViewRect,
                ref searchField,
                ref initialized);

            HandleEditorCommands(ref treeView);

            SearchBarView(ref treeView, ref searchField, ref toolBarRect);
            BodyView(ref treeView, ref bodyViewRect);
        }

        private static void InitializeIfNeeded(
            ref TreeViewState treeViewState,
            ref LocaleTreeView treeView,
            ref MultiColumnHeaderState multiColumnHeaderState,
            Rect bodyViewRect,
            ref SearchField searchField,
            ref bool initialized)
        {
            if (!initialized)
            {
                if (treeViewState == null) treeViewState = new TreeViewState();
                bool firstInit = multiColumnHeaderState == null;
                var headerState = LocaleTreeView.CreateDefaultMultiColumnHeaderState(bodyViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, headerState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, headerState);
                }

                multiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit) multiColumnHeader.ResizeToFit();

                treeView = new LocaleTreeView(treeViewState, multiColumnHeader);
                searchField = new SearchField();
                searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

                initialized = true;
            }
        }

        private static void HandleEditorCommands(ref LocaleTreeView treeView)
        {
            var selectedItems = GetSelectedAssetItems(ref treeView).ToList();
            if (selectedItems.Any())
            {
                var e = Event.current;
                if (e.type == EventType.ValidateCommand && (e.commandName == EditorCommands.DELETE || e.commandName == EditorCommands.DUPLICATE ||
                                                            e.commandName == EditorCommands.FRAME_SELECTED))
                {
                    e.Use();
                }

                if (e.type == EventType.ExecuteCommand)
                {
                    switch (e.commandName)
                    {
                        case EditorCommands.DELETE:
                            DeleteAssetItems(selectedItems);
                            break;
                        case EditorCommands.DUPLICATE:
                            DuplicateAssetItems(selectedItems);
                            break;
                        case EditorCommands.FRAME_SELECTED:
                            RevealLocalizedAsset(selectedItems.FirstOrDefault());
                            break;
                    }
                }
            }
        }

        private static void DeleteAssetItems(IEnumerable<AssetTreeViewItem> items)
        {
            foreach (var item in items)
            {
                var assetPath = AssetDatabase.GetAssetPath(item.Asset.GetInstanceID());
                AssetDatabase.MoveAssetToTrash(assetPath);
            }
        }

        private static void DuplicateAssetItems(IEnumerable<AssetTreeViewItem> items)
        {
            foreach (var item in items)
            {
                var assetPath = AssetDatabase.GetAssetPath(item.Asset.GetInstanceID());
                var newPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                AssetDatabase.CopyAsset(assetPath, newPath);
            }
        }

        private static void RevealLocalizedAsset(AssetTreeViewItem assetTreeViewItem)
        {
            Debug.Assert(assetTreeViewItem != null);
            EditorGUIUtility.PingObject(assetTreeViewItem.Asset);
        }

        private static IEnumerable<AssetTreeViewItem> GetSelectedAssetItems(ref LocaleTreeView treeView) { return GetSelectedItemsAs<AssetTreeViewItem>(ref treeView); }

        private static IEnumerable<T> GetSelectedItemsAs<T>(ref LocaleTreeView treeView) where T : TreeViewItem
        {
            var selection = treeView.GetSelection();
            var items = treeView.GetRows().Where(item => item as T != null && selection.Contains(item.id));
            return items.Cast<T>();
        }

        private static void SearchBarView(ref LocaleTreeView treeView, ref SearchField searchField, ref Rect rect)
        {
            treeView.searchString = searchField.OnGUI(rect, treeView.searchString);
        }

        private static void BodyView(ref LocaleTreeView treeView, ref Rect rect)
        {
            treeView.OnGUI(rect);
            OnContextMenu(ref treeView, rect);
        }

        private static void OnContextMenu(ref LocaleTreeView treeView, Rect rect)
        {
            var currentEvent = Event.current;
            var mousePosition = currentEvent.mousePosition;
            if (rect.Contains(mousePosition) && currentEvent.type == EventType.ContextClick)
            {
                AssetTreeViewItem assetTreeViewItem;
                LocaleTreeViewItem localeTreeViewItem;
                TryGetSelectedTreeViewItem(ref treeView, out assetTreeViewItem, out localeTreeViewItem);

                if (assetTreeViewItem != null && localeTreeViewItem != null)
                {
                    OnLocaleItemContextMenu(assetTreeViewItem, localeTreeViewItem);
                    currentEvent.Use();
                }
                else
                {
                    OnAssetItemContextMenu(ref assetTreeViewItem, ref mousePosition);
                    currentEvent.Use();
                }
            }
        }

        private static void TryGetSelectedTreeViewItem(ref LocaleTreeView treeView, out AssetTreeViewItem assetTreeViewItem, out LocaleTreeViewItem localeTreeViewItem)
        {
            var selectedItem = treeView.GetSelectedItem();
            assetTreeViewItem = selectedItem as AssetTreeViewItem;
            localeTreeViewItem = selectedItem as LocaleTreeViewItem;

            if (assetTreeViewItem == null && selectedItem != null)
            {
                assetTreeViewItem = ((LocaleTreeViewItem) selectedItem).Parent;
            }
        }

        private static void OnAssetItemContextMenu(ref AssetTreeViewItem assetTreeViewItem, ref Vector2 mousePosition)
        {
            var itemCreate = "Create";
            var itemRename = "Rename";
            var itemDelete = "Delete";

            if (Event.current != null)
            {
                mousePosition = Event.current.mousePosition;
            }

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(itemCreate), false, AssetItemContextMenu_Create, mousePosition);

            if (assetTreeViewItem == null)
            {
                menu.AddDisabledItem(new GUIContent(itemRename));
                menu.AddDisabledItem(new GUIContent(itemDelete));
            }
            else
            {
                menu.AddItem(new GUIContent(itemRename), false, AssetItemContextMenu_Rename);
                menu.AddItem(new GUIContent(itemDelete), false, AssetItemContextMenu_Delete);
            }

            menu.ShowAsContext();
        }

        private static void AssetItemContextMenu_Create(object mousePosition) { CreateLocalizedAssetPopup((Vector2) mousePosition); }

        private static void CreateLocalizedAssetPopup(Vector2 mousePosition)
        {
            var popupPosition = new Rect(mousePosition, Vector2.zero);
            EditorUtility.DisplayPopupMenu(popupPosition, "Assets/Create/Pancake/Localization/", null);
        }

        private static void AssetItemContextMenu_Rename()
        {
            AssetTreeViewItem assetTreeViewItem;
            LocaleTreeViewItem localeTreeViewItem;
            var window = EditorWindow.GetWindow<Wizard>();
            TryGetSelectedTreeViewItem(ref window.localeTreeView, out assetTreeViewItem, out localeTreeViewItem);
            RenameLocalizedAsset(ref window.localeTreeView, assetTreeViewItem);
        }

        private static void RenameLocalizedAsset(ref LocaleTreeView treeView, AssetTreeViewItem assetTreeViewItem)
        {
            Debug.Assert(assetTreeViewItem != null);
            treeView.BeginRename(assetTreeViewItem);
        }

        private static void AssetItemContextMenu_Delete()
        {
            var window = EditorWindow.GetWindow<Wizard>();
            DeleteAssetItems(GetSelectedAssetItems(ref window.localeTreeView));
        }

        private static void OnLocaleItemContextMenu(AssetTreeViewItem assetTreeViewItem, LocaleTreeViewItem localeTreeViewItem)
        {
            Debug.Assert(assetTreeViewItem != null);
            Debug.Assert(localeTreeViewItem != null);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Make default"), false, LocaleItemContextMenu_MakeDefault);
            menu.AddItem(new GUIContent("Remove"), false, LocaleItemContextMenu_Remove);
            menu.ShowAsContext();
        }

        private static void LocaleItemContextMenu_MakeDefault()
        {
            AssetTreeViewItem assetTreeViewItem;
            LocaleTreeViewItem localeTreeViewItem;
            var window = EditorWindow.GetWindow<Wizard>();
            TryGetSelectedTreeViewItem(ref window.localeTreeView, out assetTreeViewItem, out localeTreeViewItem);
            MakeLocaleDefault(ref window.localeTreeView, assetTreeViewItem, localeTreeViewItem);
        }

        private static void LocaleItemContextMenu_Remove()
        {
            AssetTreeViewItem assetTreeViewItem;
            LocaleTreeViewItem localeTreeViewItem;
            var window = EditorWindow.GetWindow<Wizard>();
            TryGetSelectedTreeViewItem(ref window.localeTreeView, out assetTreeViewItem, out localeTreeViewItem);
            RemoveLocale(ref window.localeTreeView, assetTreeViewItem.Asset, localeTreeViewItem.LocaleItem);
        }

        private static void MakeLocaleDefault(ref LocaleTreeView treeView, AssetTreeViewItem assetTreeViewItem, LocaleTreeViewItem localeTreeViewItem)
        {
            var localizedAsset = assetTreeViewItem.Asset;
            var language = localeTreeViewItem.LocaleItem.Language;

            var serializedObject = new SerializedObject(localizedAsset);
            serializedObject.Update();
            var elements = serializedObject.FindProperty("items");
            if (elements != null && elements.arraySize > 1)
            {
                var index = Array.FindIndex(localizedAsset.LocaleItems, x => x.Language == language);
                if (index >= 0)
                {
                    language = new Language(language.Name, language.Code, language.Custom);
                    elements.MoveArrayElement(index, 0);
                    serializedObject.ApplyModifiedProperties();
                    treeView.Reload();
                    Debug.Log(localizedAsset.name + ":" + language + " was set as the default language.");
                }
            }
        }

        private static void RemoveLocale(ref LocaleTreeView treeView, ScriptableLocaleBase scriptable, LocaleItemBase localeItem)
        {
            if (ScriptableLocaleEditor.RemoveLocale(scriptable, localeItem))
            {
                treeView.Reload();
            }
        }
    }
}