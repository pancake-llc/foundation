using Pancake;
using UnityEngine;

public class PoolDespawner : Passenger
{
    [SerializeField] private float time;

    private Timer _timer;
    private PoolMember _poolMember;

    protected override void OnEnabled()
    {
        if (_poolMember == null) _poolMember = Get<PoolMember>();
        if (_poolMember != null) _timer = Timer.Register(time, OnDespawn);
    }

    private void OnDespawn() { _poolMember.Pool.Despawn(_poolMember); }

    protected override void OnDisabled() { _timer?.Cancel(); }
}