namespace Pancake.ApexEditor
{
    public interface ITabContainer
    {
        /// <summary>
        /// Add new entity to the tab container.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="entity">Visual entity reference.</param>
        void AddEntity(string tabName, VisualEntity entity);

        /// <summary>
        /// Remove entity from the tab container by reference.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="entity">Visual entity reference.</param>
        void RemoveEntity(string tabName, VisualEntity entity);

        /// <summary>
        /// Remove entity from the tab container by index.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        /// <param name="index">Visual entity index in tab.</param>
        void RemoveEntity(string tabName, int index);
    }
}