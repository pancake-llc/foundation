using UnityEngine;
using UnityEditor;
using System;

namespace Sisus.Init.EditorOnly
{
    [InitializeOnLoad]
    public static class DragAndDropUtility
    {
        //public static bool IsDraggingObject => DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] != null;
        //public static bool IsDraggingComponent => DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is Component;
        public static UnityEngine.Object DraggedObject => DragAndDrop.objectReferences.Length > 0 ? DragAndDrop.objectReferences[0] : null;
        //public static Component DraggedComponent => DragAndDrop.objectReferences.Length > 0 ? DragAndDrop.objectReferences[0] as Component : null;

        public static Type DragAndDroppedComponentType { get; private set; }
        public static Component DragAndDroppedComponent { get; private set; }
        public static GameObject DragSourceGameObject { get; private set; }
        public static GameObject DropTargetGameObject { get; private set; }

        static DragAndDropUtility()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DetectDragAndDrop;
        }
 
        private static void DetectDragAndDrop(int instanceId, Rect itemRect)
		{
            if(Event.current.rawType != EventType.DragPerform || !itemRect.Contains(Event.current.mousePosition) || DragAndDrop.objectReferences.Length <= 0 || !(DragAndDrop.objectReferences[0] is Component component))
			{
				return;
			}

            DragSourceGameObject = component == null ? null : component.gameObject; // safe? need to null check first?
            DragAndDroppedComponent = component;
			DragAndDroppedComponentType = component.GetType();
			DropTargetGameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

            EditorApplication.delayCall += ClearState;
		}

        private static void ClearState()
		{
            DragAndDroppedComponentType = null;
            DragAndDroppedComponent = null; 
            DragSourceGameObject = null;
            DropTargetGameObject = null;
        }
    }
}