using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Pancake;

namespace PancakeEditor
{
    // ReSharper disable once UnusedType.Global
    public sealed class HierarchyToggleDrawer : HierarchyDrawer
    {
        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            try
            {
                _ = HierarchySettings.Instance;
            }
            catch (Exception)
            {
                return;
            }
            
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            if (gameObject.TryGetComponent<HierarchyObject>(out _)) return;

            if (HierarchySettings.ShowHierarchyToggles)
            {
                var rect = selectionRect;
                rect.x = rect.xMax - 2.7f;
                rect.width = 16f;

                var active = GUI.Toggle(rect, gameObject.activeSelf, string.Empty);
                if (active != gameObject.activeSelf)
                {
                    Undo.RecordObject(gameObject, $"{(active ? "Activate" : "Deactivate")} GameObject '{gameObject.name}'");
                    gameObject.SetActive(active);
                    EditorUtility.SetDirty(gameObject);
                }
            }

            if (HierarchySettings.ShowComponentIcons)
            {
                var rect = selectionRect;
                rect.x = rect.xMax - (HierarchySettings.ShowHierarchyToggles ? 18.7f : 2.7f);
                rect.y += 1f;
                rect.width = 14f;
                rect.height = 14f;

                var components = gameObject
                    .GetComponents<Component>()
                    .AsEnumerable()
                    .Reverse();

                var existsScriptIcon = false;
                foreach (var component in components)
                {
                    var image = AssetPreview.GetMiniThumbnail(component);
                    if (image == null) continue;

                    if (image == EditorGUIUtility.IconContent("cs Script Icon").image)
                    {
                        if (existsScriptIcon) continue;
                        existsScriptIcon = true;
                    }

                    DrawIcon(ref rect, image, IsEnabled(component) ? Color.white : new(1f, 1f, 1f, 0.5f));
                }
            }
        }

        private static void DrawIcon(ref Rect rect, Texture image, Color color)
        {
            var defaultColor = GUI.color;
            GUI.color = color;

            GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
            rect.x -= rect.width;

            GUI.color = defaultColor;
        }

        private static bool IsEnabled(Component component)
        {
            var property = component.GetType().GetProperty("enabled", typeof(bool));
            return (bool)(property?.GetValue(component, null) ?? true);
        }
    }

}