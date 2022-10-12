using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class EntityContainer : VisualEntity, IEnumerable<VisualEntity>
    {
        private string name;

        public EntityContainer(string name) { this.name = name; }

        public void DrawEntities(Rect position, in List<VisualEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (position.height == 0)
                {
                    break;
                }

                VisualEntity entity = entities[i];
                if (position.height > 0)
                {
                    if (entity.IsVisible())
                    {
                        float height = entity.GetHeight();
                        if (position.height < height)
                        {
                            height = position.height;
                            position.height = 0;
                        }
                        else
                        {
                            position.height -= height;
                        }

                        if (height > 0)
                        {
                            Rect entityPosition = new Rect(position.x, position.y, position.width, height);
                            entity.OnGUI(entityPosition);
                            position.y += height + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }
        }

        #region [Abstract Methods]

        public abstract IEnumerator<VisualEntity> GetEnumerator();

        #endregion

        #region [IEnumerable<VisualElement> Implementation]

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

        #region [Getter / Setter]

        public string GetName() { return name; }

        public void SetName(string value) { name = value; }

        #endregion
    }
}