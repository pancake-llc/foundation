namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary>
        /// Returns bool if layer is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, int layer) => (mask.value & (1 << layer)) > 0;

        /// <summary>
        /// Returns true if gameObject is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, GameObject gameobject) => (mask.value & (1 << gameobject.layer)) > 0;
    }
}