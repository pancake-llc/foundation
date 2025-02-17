using UnityEngine;

namespace Pancake.Common
{
    public static class PhysicsCast
    {
        #region Raycast

        public static IReadonlyDynamicArray<RaycastHit> RaycastNonAlloc(
            Ray ray,
            DynamicArray<RaycastHit> results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.RaycastNonAlloc(ray,
                    results.items,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        public static IReadonlyDynamicArray<RaycastHit> RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            DynamicArray<RaycastHit> results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.RaycastNonAlloc(origin,
                    direction,
                    results.items,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> RaycastNonAlloc(
            Ray ray,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return RaycastNonAlloc(ray,
                DynamicArray<RaycastHit>.Get(),
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return RaycastNonAlloc(origin,
                direction,
                DynamicArray<RaycastHit>.Get(),
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        #endregion

        #region BoxCast

        public static IReadonlyDynamicArray<RaycastHit> BoxCastNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            Vector3 direction,
            DynamicArray<RaycastHit> results,
            Quaternion orientation,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.BoxCastNonAlloc(center,
                    halfExtents,
                    direction,
                    results.items,
                    orientation,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        public static IReadonlyDynamicArray<RaycastHit> BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, DynamicArray<RaycastHit> results)
        {
            return BoxCastNonAlloc(center,
                halfExtents,
                direction,
                results,
                Quaternion.identity);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.BoxCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> BoxCastNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            Vector3 direction,
            Quaternion orientation,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return BoxCastNonAlloc(center,
                halfExtents,
                direction,
                DynamicArray<RaycastHit>.Get(),
                orientation,
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.BoxCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction)
        {
            return BoxCastNonAlloc(center, halfExtents, direction, DynamicArray<RaycastHit>.Get());
        }

        #endregion

        #region CapsuleCast

        public static IReadonlyDynamicArray<RaycastHit> CapsuleCastNonAlloc(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Vector3 direction,
            DynamicArray<RaycastHit> results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.CapsuleCastNonAlloc(point1,
                    point2,
                    radius,
                    direction,
                    results.items,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.CapsuleCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> CapsuleCastNonAlloc(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Vector3 direction,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return CapsuleCastNonAlloc(point1,
                point2,
                radius,
                direction,
                DynamicArray<RaycastHit>.Get(),
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        #endregion

        #region SphereCast

        public static IReadonlyDynamicArray<RaycastHit> SphereCastNonAlloc(
            Ray ray,
            float radius,
            DynamicArray<RaycastHit> results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.SphereCastNonAlloc(ray,
                    radius,
                    results.items,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        public static IReadonlyDynamicArray<RaycastHit> SphereCastNonAlloc(
            Vector3 origin,
            float radius,
            Vector3 direction,
            DynamicArray<RaycastHit> results,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.SphereCastNonAlloc(origin,
                    radius,
                    direction,
                    results.items,
                    maxDistance,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> SphereCastNonAlloc(
            Ray ray,
            float radius,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return SphereCastNonAlloc(ray,
                radius,
                DynamicArray<RaycastHit>.Get(),
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit> SphereCastNonAlloc(
            Vector3 origin,
            float radius,
            Vector3 direction,
            float maxDistance = float.PositiveInfinity,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return SphereCastNonAlloc(origin,
                radius,
                direction,
                DynamicArray<RaycastHit>.Get(),
                maxDistance,
                layerMask,
                queryTriggerInteraction);
        }

        #endregion

        #region OverlapBox

        public static IReadonlyDynamicArray<Collider> OverlapBoxNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            DynamicArray<Collider> results,
            Quaternion orientation,
            int mask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.OverlapBoxNonAlloc(center,
                    halfExtents,
                    results.items,
                    orientation,
                    mask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        public static IReadonlyDynamicArray<Collider> OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, DynamicArray<Collider> results)
        {
            return OverlapBoxNonAlloc(center, halfExtents, results, Quaternion.identity);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.OverlapBoxNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider> OverlapBoxNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion orientation,
            int mask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return OverlapBoxNonAlloc(center,
                halfExtents,
                DynamicArray<Collider>.Get(),
                orientation,
                mask,
                queryTriggerInteraction);
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider> OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents)
        {
            return OverlapBoxNonAlloc(center, halfExtents, DynamicArray<Collider>.Get());
        }

        #endregion

        #region OverlapCapsule

        public static IReadonlyDynamicArray<Collider> OverlapCapsuleNonAlloc(
            Vector3 point0,
            Vector3 point1,
            float radius,
            DynamicArray<Collider> results,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.OverlapCapsuleNonAlloc(point0,
                    point1,
                    radius,
                    results.items,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.OverlapCapsuleNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider> OverlapCapsuleNonAlloc(
            Vector3 point0,
            Vector3 point1,
            float radius,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return OverlapCapsuleNonAlloc(point0,
                point1,
                radius,
                DynamicArray<Collider>.Get(),
                layerMask,
                queryTriggerInteraction);
        }

        #endregion

        #region OverlapSphere

        public static IReadonlyDynamicArray<Collider> OverlapSphereNonAlloc(
            Vector3 position,
            float radius,
            DynamicArray<Collider> results,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int count;

            while (true)
            {
                count = Physics.OverlapSphereNonAlloc(position,
                    radius,
                    results.items,
                    layerMask,
                    queryTriggerInteraction);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = PhysicsCast.OverlapSphereNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider> OverlapSphereNonAlloc(
            Vector3 position,
            float radius,
            int layerMask = -1,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return OverlapSphereNonAlloc(position,
                radius,
                DynamicArray<Collider>.Get(),
                layerMask,
                queryTriggerInteraction);
        }

        #endregion
    }
}