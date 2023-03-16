using UnityEngine;

namespace Pancake
{
    public class PoolEntry : Passenger
    {
        [SerializeField] private PoolPreset poolPreset;

        private void Awake() { MagicPool.InstallPoolPreset(poolPreset); }

        private void OnDestroy() { MagicPool.Reset(); }
    }
}