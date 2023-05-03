using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class HorizontalContainer : ListContainer
    {
        /// <summary>
        /// Horizontal container constructor.
        /// </summary>
        /// <param name="entities">Horizontal container entities.</param>
        public HorizontalContainer(string name, List<VisualEntity> entities)
            : base(name, entities)
        {
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling layout element.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            int visibleCount = 0;
            for (int i = 0; i < GetEntityCount(); i++)
            {
                VisualEntity entity = GetEntity(i);
                if (entity.IsVisible())
                {
                    visibleCount++;
                }
            }

            position.width /= visibleCount;
            position.width -= ApexGUIUtility.HorizontalSpacing;
            for (int i = 0; i < GetEntityCount(); i++)
            {
                VisualEntity entity = GetEntity(i);
                if (entity.IsVisible())
                {
                    position.height = entity.GetHeight();
                    entity.OnGUI(position);
                    position.x = position.xMax + ApexGUIUtility.HorizontalSpacing;
                }
            }
        }

        /// <summary>
        /// Horizontal button group height.
        /// </summary>
        public override float GetHeight()
        {
            float height = 0;
            for (int i = 0; i < GetEntityCount(); i++)
            {
                VisualEntity entity = GetEntity(i);
                if (entity.IsVisible())
                {
                    height = Mathf.Max(height, entity.GetHeight());
                }
            }

            return height;
        }

        /// <summary>
        /// Horizontal group visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            for (int i = 0; i < GetEntityCount(); i++)
            {
                if (GetEntity(i).IsVisible())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}