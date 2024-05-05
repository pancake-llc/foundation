using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace PancakeEditor.ComponentHeader
{
    internal static class VisualElementFinder
    {
        public static VisualElement[] FindHeaderElementArray(EditorWindow inspectorWindow)
        {
            var rootVisualElement = inspectorWindow.rootVisualElement;
            var visualElementList = rootVisualElement.Query<VisualElement>().ToList();

            if (visualElementList.Any(x => x.name == "Prefab ImporterHeader"))
            {
                return Array.Empty<VisualElement>();
            }

            return visualElementList.Where(x => x.name == "TextMeshPro - Text (UI)Header" || (!x.name.StartsWith("Inspector Component Header GUI") &&
                                                                                              ((x.name.EndsWith("Header") && !x.name.EndsWith(")Header")) ||
                                                                                               x.name.EndsWith("(Script)Header")) && x.name != "HeaderHeader"))
                .ToList()
                .Skip(1)
                .ToArray();
        }
    }
}