using UnityEngine;

namespace Pancake.Sound
{
    public class VolumeAttribute : PropertyAttribute
    {
        public readonly bool canBoost;

        public VolumeAttribute() { canBoost = false; }

        public VolumeAttribute(bool canBoost) { this.canBoost = canBoost; }
    }
}