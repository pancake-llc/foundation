using UnityEngine;

namespace Pancake
{
    public class PoolEntry : Mono
    {
        [SerializeField] private PoolPreset poolPreset;

        private void Awake() { MagicPool.InstallPoolPreset(poolPreset); }

        private void OnDestroy() { MagicPool.Reset(); }
    }
}