using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PancakeEditor
{
    [InitializeOnLoad]
    public static class HierarchyGUI
    {
        private static double nextWindowsUpdate;
        private static EditorWindow[] windowsCache;
        private static readonly FieldInfo SceneHierarchyField;
        private static readonly FieldInfo TreeViewField;
        private static readonly PropertyInfo TreeViewDataProperty;
        private static readonly MethodInfo TreeViewItemsMethod;

        private static IEnumerable<EditorWindow> GetAllWindowsByType(string type)
        {
            var objectList = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            var windows = from obj in objectList where obj.GetType().ToString() == type select (EditorWindow) obj;
            return windows;
        }

        private static IEnumerable<EditorWindow> GetAllHierarchyWindows(bool forceUpdate = false)
        {
            if (forceUpdate || nextWindowsUpdate < EditorApplication.timeSinceStartup)
            {
                nextWindowsUpdate = EditorApplication.timeSinceStartup + 2;
                windowsCache = GetAllWindowsByType("UnityEditor.SceneHierarchyWindow").ToArray();
            }

            return windowsCache;
        }

        static HierarchyGUI()
        {
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));

            var hierarchyWindowType = assembly.GetType("UnityEditor.SceneHierarchyWindow");
            SceneHierarchyField = hierarchyWindowType.GetField("m_SceneHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);

            var sceneHierarchyType = assembly.GetType("UnityEditor.SceneHierarchy");
            TreeViewField = sceneHierarchyType.GetField("m_TreeView", BindingFlags.Instance | BindingFlags.NonPublic);

            var treeViewType = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
            TreeViewDataProperty = treeViewType.GetProperty("data", BindingFlags.Instance | BindingFlags.Public);

            var treeViewDataType = assembly.GetType("UnityEditor.GameObjectTreeViewDataSource");
            TreeViewItemsMethod = treeViewDataType.GetMethod("GetRows", BindingFlags.Instance | BindingFlags.Public);

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (gameObject == null) return;

            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject) != null) return;
            if (EditorSceneManager.IsPreviewSceneObject(gameObject)) return;
            var components = gameObject.GetComponents<Component>();
            if (components.IsNullOrEmpty()) return;

            var component = components.Length > 1 ? components[1] : components[0];
            var type = component.GetType();
            var content = EditorGUIUtility.ObjectContent(component, type);
            if (content.image == null) return;

            ApplyIconByInstanceId(instanceId, (Texture2D) content.image);
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ApplyIconByInstanceId(int instanceId, Texture2D icon)
        {
            var hierarchyWindows = GetAllHierarchyWindows();

            foreach (var window in hierarchyWindows)
            {
                var treeViewItems = GetTreeViewItems(window);
                var treeViewItem = treeViewItems.FirstOrDefault(item => item.id == instanceId);
                if (treeViewItem != null) treeViewItem.icon = icon;
            }
        }

        private static IEnumerable<TreeViewItem> GetTreeViewItems(EditorWindow window)
        {
            object sceneHierarchy = SceneHierarchyField.GetValue(window);
            object treeView = TreeViewField.GetValue(sceneHierarchy);
            object treeViewData = TreeViewDataProperty.GetValue(treeView, null);
            var treeViewItems = (IEnumerable<TreeViewItem>) TreeViewItemsMethod.Invoke(treeViewData, null);

            return treeViewItems;
        }
    }
}