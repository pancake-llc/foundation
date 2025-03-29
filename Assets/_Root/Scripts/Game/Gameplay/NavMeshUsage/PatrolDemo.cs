using System;
using System.Collections.Generic;
using System.Threading;
using Pancake.AI;
using UnityEngine.AI;

namespace Pancake.Game.NavMeshUsage
{
    using UnityEngine;

    public class PatrolDemo : MonoBehaviour
    {
        public NavMeshAgent agent;
        public List<Transform> patrolPoints;

        private CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new CancellationTokenSource();
            agent.Patroll(waypoints: () => patrolPoints,
                tolerance: 0.5f,
                waitTime: () => 2f, // Wait 2 seconds at each point
                followWaypointOrder: () => true, // Patrol in order
                loopWhile: () => true, // Infinite loop
                ignoreYAxis: () => true, // Ignore the Y axis when calculating distance to destination
                onStartMoving: () => Debug.Log("Start moving to patrol point"),
                onStopMoving: () => Debug.Log("Stop at patrol point"),
                onUpdate: () => Debug.Log("On patrol..."),
                cancellationToken: _cts.Token);
        }
        
        private void OnDestroy() { CancelPatrol(); }

        public void CancelPatrol() { _cts.Cancel(); }
    }
}