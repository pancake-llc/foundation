#if PANCAKE_AI
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    public static class NavMeshExtension
    {
        public static bool RandomPositionNavInsideUnitSphere(Vector3 center, float range, out Vector3 result, int numberQuery = 3, float maxDistance = 1f)
        {
            for (int i = 0; i < numberQuery; i++)
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
            for (int i = 0; i < numberQuery; i++)
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
    }
}
#endif