using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class Container : VisualEntity, IContainer
    {
        protected Container(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Find visual entity in root container.
        /// </summary>
        /// <param name="entityPath">Path to visual entity.</param>
        public VisualEntity FindEntity(string entityPath)
        {
            VisualEntity visualEntity = this;
            string[] names = entityPath.Split('/');
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (visualEntity is Container container)
                {
                    foreach (VisualEntity entity in container.Entities)
                    {
                        if (entity.GetName() == name)
                        {
                            visualEntity = entity;
                            break;
                        }
                    }
                }
                else if (i < names.Length - 1)
                {
                    Debug.LogError($"The entity on the specified path was not found. <i>({entityPath})</i>");
                    return null;
                }
            }

            return visualEntity;
        }

        /// <summary>
        /// Find visual entity in root container.
        /// </summary>
        /// <param name="entityPath">Path to visual entity.</param>
        public T FindEntity<T>(string entityPath) where T : VisualEntity { return FindEntity(entityPath) as T; }

        #region [IContainer Implementation]

        public abstract IEnumerable<VisualEntity> Entities { get; }

        #endregion

        #region [IEntityVisibility Implementation]

        /// <summary>
        /// Container visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            foreach (VisualEntity entity in Entities)
            {
                if (entity.IsVisible())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

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
                VisualEntity entity = entities[i];
                if (entity.IsVisible())
                {
                    height += entities[i].GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height - EditorGUIUtility.standardVerticalSpacing;
        }

        #endregion
    }
}