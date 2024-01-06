using System.Collections.Generic;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public enum HierarchyLayoutStatus
    {
        Success,
        Partly,
        Failed,
    }

    public class BaseHierarchy
    {
        protected Rect rect = new Rect(0, 0, 16, 16);
        protected virtual bool Enabled => false;
        protected bool showComponentDuringPlayMode;

        public virtual bool IsEnabled()
        {
            if (!Enabled) return false;

            if (Application.isPlaying) return showComponentDuringPlayMode;
            return true;
        }

        public virtual HierarchyLayoutStatus Layout(GameObject gameObject, Rect selectionRect, ref Rect currentRect, float maxWidth)
        {
            return HierarchyLayoutStatus.Success;
        }

        public virtual void Draw(GameObject gameObject, Rect selectionRect) { }

        public virtual void EventHandler(GameObject gameObject, Event currentEvent) { }

        public virtual void DisabledHandler(GameObject gameObject) { }

        protected void GetListGameObjectRecursive(GameObject gameObject, ref List<GameObject> result, int maxDepth = int.MaxValue)
        {
            result.Add(gameObject);
            if (maxDepth > 0)
            {
                var transform = gameObject.transform;
                for (int i = transform.childCount - 1; i >= 0; i--) GetListGameObjectRecursive(transform.GetChild(i).gameObject, ref result, maxDepth - 1);
            }
        }
    }
}