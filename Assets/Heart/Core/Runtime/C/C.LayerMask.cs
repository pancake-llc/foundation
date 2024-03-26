using UnityEngine;

namespace Pancake
{
    public partial class C
    {
        /// <summary>
        /// Returns bool if layer is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, int layer) { return (mask.value & (1 << layer)) > 0; }

        /// <summary>
        /// Returns true if gameObject is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, GameObject gameobject) { return (mask.value & (1 << gameobject.layer)) > 0; }
    }
}