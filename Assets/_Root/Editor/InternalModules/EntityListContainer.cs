using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class EntityListContainer : EntityContainer
    {
        private static readonly List<VisualEntity> NoneChildren = new List<VisualEntity>();

        private List<VisualEntity> children = NoneChildren;

        /// <summary>
        /// Element container constructor.
        /// </summary>
        /// <param name="children">Container children.</param>
        public EntityListContainer(string name, List<VisualEntity> children)
            : base(name)
        {
            if (children != null && children.Count > 0)
            {
                this.children = children;
            }
        }

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position) { DrawEntities(position, in children); }

        /// <summary>
        /// Area container elements height.
        /// </summary>
        public override float GetHeight()
        {
            float height = 0;
            for (int i = 0; i < GetChildrenCount(); i++)
            {
                height += GetChild(i).GetHeight();
                if (i < GetChildrenCount() - 1)
                {
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        public override IEnumerator<VisualEntity> GetEnumerator() { return children.GetEnumerator(); }

        /// <summary>
        /// Add new child.
        /// </summary>
        /// <param name="element"></param>
        public virtual void Add(VisualEntity element)
        {
            if (children == NoneChildren)
            {
                children = new List<VisualEntity>();
            }

            children.Add(element);
        }

        /// <summary>
        /// Remove child at index.
        /// </summary>
        /// <param name="index"></param>
        public virtual void Remove(int index) { children.RemoveAt(index); }

        /// <summary>
        /// Remove child by reference.
        /// </summary>
        /// <param name="field"></param>
        public virtual void Remove(VisualEntity field) { children.Remove(field); }

        #region [Getter / Setter]

        public List<VisualEntity> GetChildren() { return children; }

        public void SetChildren(List<VisualEntity> children)
        {
            if (children != null && children.Count > 0)
            {
                this.children = children;
            }
            else
            {
                this.children = NoneChildren;
            }
        }

        public VisualEntity GetChild(int index) { return children[index]; }

        public void SetChild(int index, VisualEntity value) { children[index] = value; }

        public int GetChildrenCount() { return children.Count; }

        #endregion
    }
}