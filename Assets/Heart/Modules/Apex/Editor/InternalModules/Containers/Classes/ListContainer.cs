using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class ListContainer : Container, IListContainer
    {
        protected List<VisualEntity> entities;

        protected ListContainer(string name)
            : base(name)
        {
            this.entities = new List<VisualEntity>();
        }

        protected ListContainer(string name, List<VisualEntity> entities)
            : base(name)
        {
            this.entities = entities;
        }

        public void Sort()
        {
            entities.Sort();
            for (int i = 0; i < entities.Count; i++)
            {
                VisualEntity entity = entities[i];
                if (entity is ListContainer container)
                {
                    container.Sort();
                }
            }
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling list entities.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position) { DrawEntities(position, in entities); }

        /// <summary>
        /// Total height required to drawing list entities.
        /// </summary>
        public override float GetHeight() { return GetEntitiesHeight(in entities); }

        #endregion

        #region [IContainer Implementation]

        /// <summary>
        /// Loop through all entities of the entity container.
        /// </summary>
        public override IEnumerable<VisualEntity> Entities { get { return entities; } }

        #endregion

        #region [IListContainer Implementation]

        /// <summary>
        /// Add new entity to the list container.
        /// </summary>
        /// <param name="entity">Visual entity reference.</param>
        public void AddEntity(VisualEntity entity) { entities.Add(entity); }

        /// <summary>
        /// Remove entity from the list container by reference.
        /// </summary>
        /// <param name="entity">Visual entity reference.</param>
        public void RemoveEntity(VisualEntity entity) { entities.Remove(entity); }

        /// <summary>
        /// Remove entity from the list container by index.
        /// </summary>
        /// <param name="index">Visual entity index in container.</param>
        public void RemoveEntity(int index) { entities.RemoveAt(index); }

        #endregion

        #region [IEntityVisibility Implementation]

        /// <summary>
        /// Container visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].IsVisible())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region [Getter / Setter]

        public List<VisualEntity> GetEntities() { return entities; }

        public void SetEntities(List<VisualEntity> entities) { this.entities = entities; }

        public VisualEntity GetEntity(int index) { return entities[index]; }

        public void SetEntity(int index, VisualEntity entity) { entities[index] = entity; }

        public int GetEntityCount() { return entities.Count; }

        #endregion
    }
}