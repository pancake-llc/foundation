using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PancakeEditor.ComponentHeader
{
    [InitializeOnLoad]
    internal static class InspectorComponentHeaderGUI
    {
        private static readonly List<VisualElement> ContainerList = new();

        static InspectorComponentHeaderGUI()
        {
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            Selection.selectionChanged -= Refresh;
            Selection.selectionChanged += Refresh;
            ObjectFactory.componentWasAdded -= OnComponentWasAdded;
            ObjectFactory.componentWasAdded += OnComponentWasAdded;
            Undo.undoRedoPerformed -= Refresh;
            Undo.undoRedoPerformed += Refresh;

            EditorApplication.delayCall += () => EditorApplication.delayCall += Refresh;
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; i++)
            {
                var eventType = stream.GetEventType(i);

                if (eventType is ObjectChangeKind.ChangeGameObjectStructureHierarchy or ObjectChangeKind.ChangeGameObjectStructure)
                {
                    Refresh();
                }
            }
        }

        private static void OnComponentWasAdded(Component component)
        {
            Refresh();
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            foreach (var container in ContainerList)
            {
                container.parent.Remove(container);
            }

            ContainerList.Clear();
            if (!InspectorWindowManager.TryGet(out var inspectorWindow)) return;
            var headerElementArray = VisualElementFinder.FindHeaderElementArray(inspectorWindow);

            if (headerElementArray.Length <= 0) return;

            foreach (var headerElement in headerElementArray)
            {
                string headerElementName = headerElement.name;

                if (headerElementName is "TransformHeader" or "Rect TransformHeader") continue;
              
                var container = VisualElementCreator.CreateContainer(headerElementName: headerElementName, onRefresh: () => EditorApplication.delayCall += Refresh);

                headerElement.Add(container);
                ContainerList.Add(container);
            }
        }
    }
}