using UnityEngine;

namespace Pancake.Common
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

        /// <summary>
        /// Return LayerMask value of name layer
        /// </summary>
        /// <param name="nameLayer"></param>
        /// <returns></returns>
        public static int AsLayerMask(this string nameLayer)
        {
            var resultMask = 0;
            if (LayerMask.NameToLayer(nameLayer) != -1) resultMask |= 1 << LayerMask.NameToLayer(nameLayer);
            return resultMask;
        }
    }
}