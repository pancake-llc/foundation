using UnityEngine;

namespace Pancake
{
    public class PoolDespawner : Mono
    {
        [SerializeField] private float time;

        private Timer _timer;

        protected override void OnEnabled() { _timer = Timer.Register(time, OnDespawn, isLooped: true); }

        private void OnDespawn() { MagicPool.Despawn(gameObject); }

        protected override void OnDisabled() { _timer?.Cancel(); }
    }
}