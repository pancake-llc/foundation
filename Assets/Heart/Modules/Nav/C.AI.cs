#if PANCAKE_AI
using UnityEngine;
using UnityEngine.AI;

namespace Pancake.AI
{
    public static partial class C
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
    }
}
#endif