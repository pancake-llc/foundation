#if PANCAKE_AI
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Common;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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

        public static void PatrolDestination(this NavMeshAgent agent, List<Vector3> patrolPath, float tolerance = 1f)
        {
            if (agent == null || patrolPath == null || patrolPath.Count == 0) return;

            var currentWaypoint = patrolPath[0];
            if (Vector3.Distance(agent.transform.position, currentWaypoint) <= tolerance)
            {
                int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
                currentWaypoint = patrolPath[nextIndex];
            }

            agent.SetDestination(currentWaypoint);
        }

        /// <summary>
        /// Call it from a loop
        /// </summary>
        public static void PatrolDestination(this NavMeshAgent agent, List<Transform> patrolPath, float tolerance = 1f)
        {
            if (agent == null || patrolPath == null || patrolPath.Count == 0) return;

            var currentWaypoint = patrolPath[0];
            if (Vector3.Distance(agent.transform.position, currentWaypoint.position) <= tolerance)
            {
                int nextIndex = (patrolPath.IndexOf(currentWaypoint) + 1) % patrolPath.Count;
                currentWaypoint = patrolPath[nextIndex];
            }

            agent.SetDestination(currentWaypoint.position);
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

        public static async void SetTemporarySpeed(this NavMeshAgent agent, float temporarySpeed, float duration, CancellationToken cancellationToken = default)
        {
            if (agent == null || !agent.isActiveAndEnabled) return;
            await TemporarySpeedCoroutine(agent, temporarySpeed, duration);
            return;

            async Task TemporarySpeedCoroutine(NavMeshAgent agent, float temporarySpeed, float duration)
            {
                float originalSpeed = agent.speed;
                agent.speed = temporarySpeed;

                await Awaitable.WaitForSecondsAsync(duration, cancellationToken);

                if (agent != null && agent.isActiveAndEnabled) agent.speed = originalSpeed;
            }
        }

        public static void Wander(
            this NavMeshAgent agent,
            Func<float> radius,
            bool isContinues = true,
            Func<float> waitTime = null,
            Func<bool> loopWhile = null,
            Action onStartMoving = null,
            Action onStopMoving = null,
            Action onUpdate = null,
            CancellationToken cancellationToken = default)
        {
            if (agent == null || !agent.isActiveAndEnabled) return;

            if (isContinues) WanderCoroutine();
            else agent.SetRandomDestination(radius());


            async void WanderCoroutine()
            {
                while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true))
                {
                    if (agent.SetRandomDestination(radius()))
                    {
                        onStartMoving?.Invoke();
                        while (!agent.HasReachedDestination() && agent.isActiveAndEnabled && !cancellationToken.IsCancellationRequested)
                        {
                            onUpdate?.Invoke();
                            await Awaitable.EndOfFrameAsync(cancellationToken);
                        }

                        onStopMoving?.Invoke();
                    }

                    await Awaitable.WaitForSecondsAsync(waitTime?.Invoke() ?? 1, cancellationToken);
                }
            }
        }

        public static void Chase(
            this NavMeshAgent agent,
            Func<Transform> target,
            Func<float> minDistanceKeep,
            Func<float> maxDistanceKeep,
            Func<float> delayBetweenSettingDestination = null,
            Func<bool> loopWhile = null,
            Func<float> distanceToPlayer = null,
            Action onUpdate = null,
            CancellationToken cancellationToken = default)
        {
            if (agent == null || !agent.isActiveAndEnabled || target == null) return;
            ChaseTargetCoroutine();
            return;

            async void ChaseTargetCoroutine()
            {
                while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true) && !cancellationToken.IsCancellationRequested)
                {
                    var currentTarget = target();
                    if (currentTarget == null) return;

                    var selfPosition = agent.transform.position;
                    float distance = distanceToPlayer?.Invoke() ?? Vector3.Distance(selfPosition, currentTarget.position);

                    if (minDistanceKeep == null || maxDistanceKeep == null) agent.SetDestination(currentTarget.position);
                    else
                    {
                        if (distance < minDistanceKeep())
                        {
                            var moveDirection = (currentTarget.position - selfPosition).normalized;
                            var positionToMove = selfPosition + moveDirection * -1 * (maxDistanceKeep() - distance);
                            var path = new NavMeshPath();
                            if (agent.CalculatePath(positionToMove, path) && path.status == NavMeshPathStatus.PathComplete) agent.SetDestination(positionToMove);
                        }
                        else agent.SetDestination(currentTarget.position);
                    }

                    onUpdate?.Invoke();

                    float delay = delayBetweenSettingDestination?.Invoke() ?? 0;
                    if (delay > 0) await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
                    else await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
        }

        public static void Flee(
            this NavMeshAgent agent,
            Func<Transform> target = null,
            Func<float> fleeDistance = null,
            Func<bool> loopWhile = null,
            Action onUpdate = null,
            CancellationToken cancellationToken = default)
        {
            if (agent == null || !agent.isActiveAndEnabled || target == null) return;
            FleeFromTargetCoroutine();

            return;

            async void FleeFromTargetCoroutine()
            {
                while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true) && !cancellationToken.IsCancellationRequested)
                {
                    var currentTarget = target();
                    if (currentTarget == null) return;

                    float fleeDistanceValue = fleeDistance?.Invoke() ?? 10;
                    var fleeDirection = (agent.transform.position - currentTarget.position).normalized;
                    var fleePosition = agent.transform.position + fleeDirection * fleeDistanceValue;

                    if (NavMesh.SamplePosition(fleePosition, out var hit, fleeDistanceValue, NavMesh.AllAreas)) agent.SetDestination(hit.position);

                    onUpdate?.Invoke();

                    await Awaitable.EndOfFrameAsync(cancellationToken);
                }
            }
        }

        public static void Patroll(
            this NavMeshAgent agent,
            Func<List<Transform>> waypoints,
            float tolerance = 0.1f,
            Func<float> waitTime = null,
            Func<bool> followWaypointOrder = null,
            Func<bool> loopWhile = null,
            Action onStartMoving = null,
            Action onStopMoving = null,
            Action onUpdate = null,
            CancellationToken cancellationToken = default)
        {
            PatrolWaypointsCoroutine();
            return;

            async void PatrolWaypointsCoroutine()
            {
                var waypointsList = waypoints?.Invoke();
                if (waypointsList == null || waypointsList.Count == 0) return;
                var currentWaypointIndex = 0;

                while (agent != null && agent.isActiveAndEnabled && (loopWhile?.Invoke() ?? true) && !cancellationToken.IsCancellationRequested)
                {
                    var currentWaypoint = waypointsList[currentWaypointIndex];
                    agent.SetDestination(currentWaypoint.position);
                    onStartMoving?.Invoke();
                    while (!agent.HasReachedDestination(currentWaypoint.position, tolerance) && agent.isActiveAndEnabled && !cancellationToken.IsCancellationRequested)
                    {
                        onUpdate?.Invoke();
                        await Awaitable.EndOfFrameAsync(cancellationToken);
                    }

                    onStopMoving?.Invoke();
                    await Awaitable.WaitForSecondsAsync(waitTime?.Invoke() ?? 1, cancellationToken);
                    currentWaypointIndex = followWaypointOrder?.Invoke() ?? true ? (currentWaypointIndex + 1) % waypoints().Count : Random.Range(0, waypoints().Count);
                }
            }
        }

        public static void ContinuesAvoidObstaclesWhile(
            this NavMeshAgent agent,
            LayerMask obstacleMask,
            float avoidanceRadius,
            Func<bool> condition = null,
            CancellationToken cancellationToken = default)
        {
            if (agent == null || !agent.isActiveAndEnabled) return;
            AvoidObstaclesCoroutine();
            return;

            async void AvoidObstaclesCoroutine()
            {
                while (agent != null && agent.isActiveAndEnabled && (condition?.Invoke() ?? true))
                {
                    var obstacles = PhysicsCast.OverlapSphereNonAlloc(agent.transform.position, avoidanceRadius, obstacleMask);
                    if (obstacles.Length > 0)
                    {
                        var avoidanceDirection = Vector3.zero;
                        foreach (var obstacle in obstacles)
                        {
                            avoidanceDirection += (agent.transform.position - obstacle.transform.position).normalized;
                        }

                        avoidanceDirection /= obstacles.Length;

                        var newDestination = agent.transform.position + avoidanceDirection * avoidanceRadius;
                        if (NavMesh.SamplePosition(newDestination, out var hit, avoidanceRadius, NavMesh.AllAreas)) agent.SetDestination(hit.position);
                    }

                    await Awaitable.EndOfFrameAsync(cancellationToken);
                }
            }
        }
    }
}
#endif