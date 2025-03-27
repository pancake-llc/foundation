#if PANCAKE_AI
using System.Collections.Generic;
using System.Threading.Tasks;
using Pancake.Common;
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    public static class NavMeshExtension
    {
        public static bool RandomPositionNavInsideUnitSphere(Vector3 center, float range, out Vector3 result, int numberQuery = 3, float maxDistance = 1f)
        {
            for (var i = 0; i < numberQuery; i++)
            {
                var randomPoint = center + Random.insideUnitSphere * range;
                if (NavMesh.SamplePosition(randomPoint, out var hit, maxDistance, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        public static bool RandomPositionNavOnUnitSphere(Vector3 center, float range, out Vector3 result, int numberQuery = 3, float maxDistance = 1f)
        {
            for (var i = 0; i < numberQuery; i++)
            {
                var randomPoint = center + Random.onUnitSphere * range;
                if (NavMesh.SamplePosition(randomPoint, out var hit, maxDistance, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        public static bool TryGetNearestNavMeshPoint(Vector3 checkPosition, out Vector3 nearestNavMeshPoint, Vector3 direction, int step, float stepValue)
        {
            for (int i = step - 1; i >= 0; i--)
            {
                if (NavMesh.SamplePosition(checkPosition, out var hit, stepValue, NavMesh.AllAreas))
                {
                    nearestNavMeshPoint = hit.position;
                    return true;
                }

                checkPosition -= direction * stepValue;
            }

            nearestNavMeshPoint = checkPosition;
            return false;
        }

        public static NavMeshAgent IncreaseAngularSpeed(this NavMeshAgent agent, float angularSpeed = 1000)
        {
            agent.angularSpeed = angularSpeed;
            return agent;
        }

        public static bool HasReachedDestination(this NavMeshAgent agent)
        {
            if (agent == null) return false;
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        }

        public static bool HasReachedDestination(this NavMeshAgent agent, Transform destination, float tolerence = 0.1f) =>
            agent.transform.Distance(destination) < tolerence;

        public static bool HasReachedDestination(this NavMeshAgent agent, Vector3 destination, float tolerence = 0.1f) =>
            agent.transform.Distance(destination) < tolerence;

        public static bool SetRandomDestination(this NavMeshAgent agent, float radius, Vector3? origin = null, int areaMask = NavMesh.AllAreas, int numberQuery = 3)
        {
            if (agent == null) return false;

            for (var i = 0; i < numberQuery; i++)
            {
                var randomDirection = Random.insideUnitSphere * radius;
                randomDirection += origin ?? agent.transform.position;
                if (NavMesh.SamplePosition(randomDirection, out var hit, radius, areaMask))
                {
                    agent.SetDestination(hit.position);
                    return true;
                }
            }

            return false;
        }

        public static async Task SmoothSpeedChange(this NavMeshAgent agent, float targetSpeed, float duration)
        {
            if (agent == null) return;
            float startSpeed = agent.speed;
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                if (agent == null) return;
                agent.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                await Awaitable.EndOfFrameAsync();
            }

            agent.speed = targetSpeed;
        }

        public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Vector3> patrolPath, float tolerance = 1f)
        {
            if (agent == null || patrolPath == null || patrolPath.Count == 0) return null;

            var currentWaypoint = patrolPath[0];
            if (Vector3.Distance(agent.transform.position, currentWaypoint) <= tolerance)
            {
                int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
                currentWaypoint = patrolPath[nextIndex];
            }

            agent.SetDestination(currentWaypoint);
            return agent;
        }

        /// <summary>
        /// Call it from a loop
        /// </summary>
        public static NavMeshAgent PatrolDestination(this NavMeshAgent agent, List<Transform> patrolPath, float tolerance = 1f)
        {
            if (agent == null || patrolPath == null || patrolPath.Count == 0) return null;

            var currentWaypoint = patrolPath[0];
            if (Vector3.Distance(agent.transform.position, currentWaypoint.position) <= tolerance)
            {
                int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
                currentWaypoint = patrolPath[nextIndex];
            }

            agent.SetDestination(currentWaypoint.position);
            return agent;
        }

        public static NavMeshAgent AddKnockBack(this NavMeshAgent agent, Transform target, float force, bool useSetDestination = false)
        {
            if (agent == null || target == null) return agent;

            agent.ResetPath();
            var selfPosition = agent.transform.position;
            var knockBackDirection = target.position.WithY(0) - selfPosition.WithY(0);
            if (knockBackDirection.sqrMagnitude > 0.01f)
            {
                if (useSetDestination) agent.SetDestination(selfPosition + knockBackDirection.normalized * force);
                else agent.velocity = knockBackDirection.normalized * force;
            }

            return agent;
        }
    }
}
#endif