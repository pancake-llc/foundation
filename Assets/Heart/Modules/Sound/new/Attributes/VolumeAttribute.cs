using UnityEngine;

namespace Pancake.Sound
{
    public class VolumeAttribute : PropertyAttribute
    {
        public bool canBoost;

        public VolumeAttribute() { canBoost = false; }

        public VolumeAttribute(bool canBoost) { this.canBoost = canBoost; }
    }
}