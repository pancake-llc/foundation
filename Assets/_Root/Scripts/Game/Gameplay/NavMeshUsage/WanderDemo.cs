using System;
using Pancake.AI;
using Pancake.Draw;

namespace Pancake.Game.NavMeshUsage
{
    using UnityEngine;
    using UnityEngine.AI;
    using System.Threading;

    public class WanderDemo : MonoBehaviour
    {
        public NavMeshAgent agent;
        public bool isStayInDefaultArea;
        public float increaseAngularSpeed = 1500f;
        private CancellationTokenSource _cts;

        private Vector3 _defaultPosition;

        private void Start()
        {
            _cts = new CancellationTokenSource();
            agent.IncreaseAngularSpeed(increaseAngularSpeed)
                .Wander(radius: () => 5f, // Wandering radius
                    isContinues: true, // Infinite loop
                    isStayInDefaultArea: isStayInDefaultArea, // Move only within the initial radius area
                    waitTime: () => 2f, // Stop 2 seconds after arriving
                    loopWhile: () => gameObject.activeSelf, // Continue moving while gameObject exists
                    onStartMoving: () => Debug.Log("Start moving"),
                    onStopMoving: () => Debug.Log("Stop moving"),
                    onUpdate: () => Debug.Log("Updating"),
                    cancellationToken: _cts.Token);

            _defaultPosition = transform.position;
        }

        private void OnDestroy() { CancelWander(); }

        public void CancelWander() { _cts.Cancel(); }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) _defaultPosition = transform.position;
            ImGizmos.WireSphere3D(_defaultPosition, Quaternion.identity, 5, Color.yellow);
        }
    }
}