using System.Threading;
using Pancake.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.Game.NavMeshUsage
{
    public class FleeDemo : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Transform player;
        public float fleeDistance = 10f; // Distance to run away from player
        public float detectionRange = 10f; // Range to start running away
        public float stopRange = 15f; // Stop when out of this range.

        private CancellationTokenSource _cts;

        private void StartFlee()
        {
            _cts = new CancellationTokenSource();
            agent.Flee(target: () => player,
                fleeDistance: () => fleeDistance,
                loopWhile: () => Vector3.Distance(transform.position, player.position) < stopRange,
                cancellationToken: _cts.Token);
        }

        private void Update()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance < detectionRange && (_cts == null || _cts.IsCancellationRequested))
            {
                StartFlee();
            }
            else if (distance > stopRange && _cts != null)
            {
                StopFlee();
            }
        }

        private void StopFlee()
        {
            _cts.Cancel();
            agent.ResetPath();
        }
    }
}