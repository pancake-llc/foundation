using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class InspectorWindowManager
    {
        private static readonly Type InspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        private static EditorWindow inspectorWindow;

        public static bool TryGet(out EditorWindow inspectorWindow)
        {
            if (InspectorWindowManager.inspectorWindow != null)
            {
                inspectorWindow = InspectorWindowManager.inspectorWindow;
                return true;
            }

            InspectorWindowManager.inspectorWindow = (EditorWindow) Resources.FindObjectsOfTypeAll(InspectorWindowType).FirstOrDefault();

            inspectorWindow = InspectorWindowManager.inspectorWindow;

            return InspectorWindowManager.inspectorWindow != null;
        }
    }
}