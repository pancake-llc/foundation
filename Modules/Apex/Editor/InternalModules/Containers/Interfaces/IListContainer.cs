namespace Pancake.ApexEditor
{
    public interface IListContainer
    {
        /// <summary>
        /// Add new entity to the list container.
        /// </summary>
        /// <param name="entity">Visual entity reference.</param>
        void AddEntity(VisualEntity entity);

        /// <summary>
        /// Remove entity from the list container by reference.
        /// </summary>
        /// <param name="entity">Visual entity reference.</param>
        void RemoveEntity(VisualEntity entity);

        /// <summary>
        /// Remove entity from the list container by index.
        /// </summary>
        /// <param name="index">Visual entity index in container.</param>
        void RemoveEntity(int index);
    }
}