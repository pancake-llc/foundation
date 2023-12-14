using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryItemsModule : GreeneryEditorModule
    {
        [Serializable]
        public class ItemsModuleSettings
        {
            public List<GreeneryItem> greeneryItems = new();
            public List<GreeneryItem> selectedItems = new();
        }

        public enum ObjectPickingMode
        {
            None,
            ItemPicking,
            PalettePicking
        }

        public ItemsModuleSettings itemsModuleSettings;

        private const string ITEMS_SETTINGS_KEY = "GREENERY_ITEMS_SETTINGS";

        private const int BUTTON_SIZE = 50;
        private const int REMOVE_BUTTON_SIZE = 18;
        private const int COLUMNS = 5;

        private int _rows;
        private ObjectPickingMode _objectPickingMode;
        private GreeneryToolEditor _toolEditor;

        public override void Initialize(GreeneryToolEditor toolEditor)
        {
            _toolEditor = toolEditor;

            itemsModuleSettings = new ItemsModuleSettings();
            if (EditorPrefs.HasKey(ITEMS_SETTINGS_KEY))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(ITEMS_SETTINGS_KEY), itemsModuleSettings);
            }

            toolEditor.OnGUI += OnGUI;
        }

        public override void Release() { _toolEditor.OnGUI -= OnGUI; }

        public override void OnGUI()
        {
            int itemsAmount = itemsModuleSettings.greeneryItems.Count;
            var currentEvent = Event.current;
            var mousePos = currentEvent.mousePosition;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Greenery Items", EditorStyles.boldLabel, GUILayout.Width(100));

            if (GUILayout.Button("Load..."))
            {
                var loadMenu = new GenericMenu();
                loadMenu.AddItem(new GUIContent("Load from manager"), false, LoadItemsFromManager);
                loadMenu.AddItem(new GUIContent("Load from palette"), false, LoadItemsFromPalette);
                loadMenu.ShowAsContext();
            }

            if (GUILayout.Button("Clear"))
            {
                itemsModuleSettings.selectedItems.Clear();
                itemsModuleSettings.greeneryItems.Clear();
                SaveSettings();
                return;
            }

            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < itemsAmount + 1; i++)
            {
                if (i < itemsAmount && itemsModuleSettings.greeneryItems[i] == null) continue;

                _rows = Mathf.FloorToInt((float) itemsAmount / COLUMNS);
                var lastRect = GUILayoutUtility.GetLastRect();
                int posX = BUTTON_SIZE * (i - (Mathf.FloorToInt((float) i / COLUMNS)) * COLUMNS);
                int posY = Mathf.FloorToInt(lastRect.position.y + lastRect.height) + BUTTON_SIZE * Mathf.FloorToInt((float) i / COLUMNS) + 10;

                var rect = new Rect(posX, posY, BUTTON_SIZE, BUTTON_SIZE);
                var buttonRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                var removeRect = new Rect(buttonRect.position.x + BUTTON_SIZE / 2f, buttonRect.y, REMOVE_BUTTON_SIZE, REMOVE_BUTTON_SIZE);
                var borderRect = new Rect(buttonRect.x - 1, buttonRect.y - 1, buttonRect.width + 2, buttonRect.height + 2);

                if (i < itemsAmount)
                {
                    var item = itemsModuleSettings.greeneryItems[i];
                    Texture itemIcon = null;
                    switch (item)
                    {
                        case GreeneryLODInstance a:
                            if (a.instanceLODs[0] != null) itemIcon = AssetPreview.GetAssetPreview(a.instanceLODs[0].instancedMesh);
                            break;
                        case GreeneryInstance b:
                            if (b.instancedMesh != null) itemIcon = AssetPreview.GetAssetPreview(b.instancedMesh);
                            break;
                        case GreeneryGrassQuad c:
                            if (c.renderingMaterial != null) itemIcon = c.renderingMaterial.mainTexture;
                            break;
                    }

                    if (itemIcon == null)
                    {
                        string pathGreeneryIcon = ProjectDatabase.GetPathInCurrentEnvironent("Modules/Greenery/Editor/Icons/greenery_icon_1.png");
                        var icon = AssetDatabase.LoadAssetAtPath<Texture>(pathGreeneryIcon);
                        itemIcon = icon != null ? icon : EditorGUIUtility.IconContent("d_TreeEditor.Leaf On").image;
                    }

                    EditorGUI.DrawPreviewTexture(buttonRect,
                        itemIcon,
                        null,
                        ScaleMode.ScaleToFit,
                        0);

                    if (itemsModuleSettings.selectedItems.Contains(item))
                    {
                        EditorGUI.DrawRect(buttonRect, new Color(0f, 1f, 0f, 0.25f));
                    }

                    if (rect.Contains(mousePos))
                    {
                        EditorGUI.DrawRect(borderRect, new Color(1, 1, 1, 0.4f));
                        if (GreeneryEditorUtilities.IsLeftClicking(currentEvent))
                        {
                            if (currentEvent.shift)
                            {
                                SelectGreeneryItem(item);
                            }
                            else if (currentEvent.control)
                            {
                                Selection.activeObject = item;
                                EditorGUIUtility.PingObject(item);
                            }
                            else
                            {
                                itemsModuleSettings.selectedItems.Clear();
                                SelectGreeneryItem(item);
                            }
                        }
                    }

                    if (removeRect.Contains(mousePos))
                    {
                        EditorGUI.DrawRect(borderRect, new Color(1, 0, 0, 0.4f));
                        if (GreeneryEditorUtilities.IsLeftClicking(currentEvent))
                        {
                            if (itemsModuleSettings.selectedItems.Contains(item))
                            {
                                itemsModuleSettings.selectedItems.Remove(item);
                            }

                            itemsModuleSettings.greeneryItems.RemoveAt(i);
                            SaveSettings();

                            break;
                        }
                    }

                    GUI.Box(removeRect, EditorGUIUtility.IconContent("winbtn_win_close_a@2x"));
                }
                else
                {
                    var addButtonStyle = new GUIStyle(GUI.skin.label);
                    Color[] pix = {new(0.35f, 0.35f, 0.35f, 0.4f)};
                    var result = new Texture2D(1, 1);
                    result.SetPixels(pix);
                    result.Apply();
                    addButtonStyle.normal.background = result;
                    addButtonStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Box(buttonRect, EditorGUIUtility.IconContent("d_Toolbar Plus@2x"), addButtonStyle);

                    if (rect.Contains(mousePos))
                    {
                        EditorGUI.DrawRect(borderRect, new Color(1, 1, 1, 0.4f));
                        if (GUI.Button(buttonRect, "", addButtonStyle))
                        {
                            int controlID = GUIUtility.GetControlID(FocusType.Passive);
                            EditorGUIUtility.ShowObjectPicker<GreeneryItem>(null, false, "", controlID);
                            _objectPickingMode = ObjectPickingMode.ItemPicking;
                        }
                    }
                }
            }

            ObjectPicking();

            EditorGUILayout.Space((_rows + 1) * BUTTON_SIZE + 20);
        }

        public void LoadItemsFromManager()
        {
            var greeneryManager = GreeneryEditorUtilities.GetActiveManager();
            foreach (var item in greeneryManager.greeneryItems)
            {
                LoadGreeneryItem(item);
            }
        }

        public void LoadGreeneryItemPalette(GreeneryItemPalette palette)
        {
            foreach (var item in palette.greeneryItems)
            {
                LoadGreeneryItem(item);
            }
        }

        public void LoadItemsFromPalette()
        {
            _objectPickingMode = ObjectPickingMode.PalettePicking;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<GreeneryItemPalette>(null, false, "", controlID);
        }

        public void LoadGreeneryItem(GreeneryItem item)
        {
            if (!itemsModuleSettings.greeneryItems.Contains(item)) itemsModuleSettings.greeneryItems.Add(item);

            SaveSettings();
        }

        public void SelectGreeneryItem(GreeneryItem item)
        {
            if (!itemsModuleSettings.selectedItems.Contains(item)) itemsModuleSettings.selectedItems.Add(item);

            SaveSettings();
        }

        public void ObjectPicking()
        {
            string commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorClosed")
            {
                switch (_objectPickingMode)
                {
                    case ObjectPickingMode.None:
                        break;
                    case ObjectPickingMode.ItemPicking:
                        var pickedItem = (GreeneryItem) EditorGUIUtility.GetObjectPickerObject();
                        if (pickedItem != null) LoadGreeneryItem(pickedItem);

                        break;
                    case ObjectPickingMode.PalettePicking:
                        var pickedPalette = (GreeneryItemPalette) EditorGUIUtility.GetObjectPickerObject();
                        if (pickedPalette != null) LoadGreeneryItemPalette(pickedPalette);

                        break;
                }
            }
        }

        public void ItemDropArea(Rect guiRect)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.DragPerform || currentEvent.type == EventType.DragUpdated)
            {
                if (guiRect.Contains(currentEvent.mousePosition)) DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GreeneryItem item) LoadGreeneryItem(item);

                        if (draggedObject is GreeneryItemPalette palette) LoadGreeneryItemPalette(palette);
                    }
                }
            }
        }

        public override void SaveSettings() { EditorPrefs.SetString(ITEMS_SETTINGS_KEY, EditorJsonUtility.ToJson(itemsModuleSettings)); }

        public override float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight * 2;
            height += (_rows + 1) * BUTTON_SIZE;
            return height;
        }
    }
}