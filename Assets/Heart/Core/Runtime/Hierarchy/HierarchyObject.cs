using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// A component used to decorate the appearance of a hierarchy.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Pancake/Hierarchy Object")]
    public class HierarchyObject : MonoBehaviour
    {
        public enum Mode
        {
            UseSettings = 0,
            None = 1,
            RemoveInPlayMode = 2,
            RemoveInBuild = 3
        }

        [SerializeField] Mode hierarchyObjectMode = Mode.UseSettings;
        public Mode HierarchyObjectMode => hierarchyObjectMode;
    }
}