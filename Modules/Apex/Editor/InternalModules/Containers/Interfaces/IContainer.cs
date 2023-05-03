using System.Collections.Generic;

namespace Pancake.ApexEditor
{
    public interface IContainer
    {
        /// <summary>
        /// Loop through all entities of the entity container.
        /// </summary>
        IEnumerable<VisualEntity> Entities { get; }
    }
}