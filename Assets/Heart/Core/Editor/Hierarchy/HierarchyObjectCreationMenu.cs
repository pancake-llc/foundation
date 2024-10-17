using UnityEditor;
using UnityEngine;
using Pancake;

namespace PancakeEditor
{
    internal static class HierarchyObjectCreationMenu
    {
        [MenuItem("GameObject/Pancake/Header", false)]
        private static void CreateHeader(MenuCommand menuCommand)
        {
            var obj = new GameObject("Header");
            obj.AddComponent<HierarchyHeader>();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }

        [MenuItem("GameObject/Pancake/Separator", false)]
        private static void CreateSeparator(MenuCommand menuCommand)
        {
            var obj = new GameObject("Separator");
            obj.AddComponent<HierarchySeparator>();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeObject = obj;
        }
    }
}