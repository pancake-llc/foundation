using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [System.Obsolete("Use Container instead.")]
    public abstract class EntityContainer : VisualEntity
    {
        private string name;

        protected EntityContainer(string name)
            : base(name)
        {
            this.name = name;
        }

        #region [Static Methods]

        public static void DrawEntities(Rect position, in List<VisualEntity> entities)
        {
            float totalHeight = position.height;
            for (int i = 0; i < entities.Count; i++)
            {
                if (totalHeight <= 0)
                {
                    break;
                }

                VisualEntity entity = entities[i];
                if (entity.IsVisible())
                {
                    position.height = entity.GetHeight();
                    entity.OnGUI(position);
                    position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        public static float GetEntitiesHeight(in List<VisualEntity> entities)
        {
            float height = 0;
            for (int i = 0; i < entities.Count; i++)
            {
                height += entities[i].GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            return height - EditorGUIUtility.standardVerticalSpacing;
        }

        #endregion
    }
}