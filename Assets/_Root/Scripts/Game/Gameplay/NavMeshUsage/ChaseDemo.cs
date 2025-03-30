using System.Threading;
using Pancake.AI;
using Sirenix.OdinInspector;
using UnityEngine.AI;

namespace Pancake.Game.NavMeshUsage
{
    using UnityEngine;

    public class ChaseDemo : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Transform player;
        public bool alwayChase;
        [HideIf(nameof(alwayChase))] public float distancePlayerToChase = 10;

        private CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new CancellationTokenSource();

            agent.Chase(target: () => player,
                minDistanceKeep: () => 2f, // Keep a minimum distance of 2
                maxDistanceKeep: () => 5f, // Distance Not more than 5
                delayBetweenSettingDestination: () => 0.5f, // Update route every 0.5 seconds
                loopWhile: () => alwayChase || Vector3.Distance(agent.transform.position, player.position) <= distancePlayerToChase,
                distanceToPlayer: () => Vector3.Distance(agent.transform.position, player.position),
                stopImmediatelyWhenLoseTarget: ()=> true,
                onUpdate: () => Debug.Log("Chasing the player..."),
                cancellationToken: _cts.Token);
        }
        
        private void OnDestroy() { CancelChase(); }

        public void CancelChase() { _cts.Cancel(); }
    }
}