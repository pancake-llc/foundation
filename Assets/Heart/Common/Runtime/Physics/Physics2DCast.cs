using UnityEngine;

namespace Pancake.Common
{
    public static class Physics2DCast
    {
        #region Raycast

        private static ContactFilter2D CreateLegacyFilter()
        {
            var legacyFilter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
            legacyFilter.SetLayerMask((LayerMask) (-5));
            legacyFilter.SetDepth(float.NegativeInfinity, float.PositiveInfinity);
            return legacyFilter;
        }

        public static IReadonlyDynamicArray<RaycastHit2D> RaycastNonAlloc(Vector2 origin, Vector2 direction, DynamicArray<RaycastHit2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return RaycastNonAlloc(origin, direction, legacyFilter, results);
        }

        public static IReadonlyDynamicArray<RaycastHit2D> RaycastNonAlloc(
            Vector2 origin,
            Vector2 direction,
            ContactFilter2D contactFilter,
            DynamicArray<RaycastHit2D> results,
            float maxDistance = float.PositiveInfinity)
        {
            int count;

            while (true)
            {
                count = Physics2D.Raycast(origin,
                    direction,
                    contactFilter,
                    results.items,
                    maxDistance);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> RaycastNonAlloc(Vector2 origin, Vector2 direction)
        {
            return RaycastNonAlloc(origin, direction, DynamicArray<RaycastHit2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.RaycastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> RaycastNonAlloc(
            Vector2 origin,
            Vector2 direction,
            ContactFilter2D contactFilter,
            float maxDistance = float.PositiveInfinity)
        {
            return RaycastNonAlloc(origin,
                direction,
                contactFilter,
                DynamicArray<RaycastHit2D>.Get(),
                maxDistance);
        }

        #endregion

        #region Linecast

        public static IReadonlyDynamicArray<RaycastHit2D> LinecastNonAlloc(Vector2 start, Vector2 end, DynamicArray<RaycastHit2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return LinecastNonAlloc(start, end, legacyFilter, results);
        }

        public static IReadonlyDynamicArray<RaycastHit2D> LinecastNonAlloc(Vector2 start, Vector2 end, ContactFilter2D contactFilter, DynamicArray<RaycastHit2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.Linecast(start, end, contactFilter, results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.LinecastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> LinecastNonAlloc(Vector2 start, Vector2 end)
        {
            return LinecastNonAlloc(start, end, DynamicArray<RaycastHit2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.LinecastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> LinecastNonAlloc(Vector2 start, Vector2 end, ContactFilter2D contactFilter)
        {
            return LinecastNonAlloc(start, end, contactFilter, DynamicArray<RaycastHit2D>.Get());
        }

        #endregion

        #region BoxCast

        public static IReadonlyDynamicArray<RaycastHit2D> BoxCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            float angle,
            Vector2 direction,
            DynamicArray<RaycastHit2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return BoxCastNonAlloc(origin,
                size,
                angle,
                direction,
                legacyFilter,
                results);
        }

        public static IReadonlyDynamicArray<RaycastHit2D> BoxCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            float angle,
            Vector2 direction,
            ContactFilter2D contactFilter,
            DynamicArray<RaycastHit2D> results,
            float distance = float.PositiveInfinity)
        {
            int count;

            while (true)
            {
                count = Physics2D.BoxCast(origin,
                    size,
                    angle,
                    direction,
                    contactFilter,
                    results.items,
                    distance);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.BoxCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction)
        {
            return BoxCastNonAlloc(origin,
                size,
                angle,
                direction,
                DynamicArray<RaycastHit2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.BoxCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> BoxCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            float angle,
            Vector2 direction,
            ContactFilter2D contactFilter,
            float distance = float.PositiveInfinity)
        {
            return BoxCastNonAlloc(origin,
                size,
                angle,
                direction,
                contactFilter,
                DynamicArray<RaycastHit2D>.Get(),
                distance);
        }

        #endregion

        #region CapsuleCast

        public static IReadonlyDynamicArray<RaycastHit2D> CapsuleCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            CapsuleDirection2D capsuleDirection,
            float angle,
            Vector2 direction,
            DynamicArray<RaycastHit2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return CapsuleCastNonAlloc(origin,
                size,
                capsuleDirection,
                angle,
                direction,
                legacyFilter,
                results);
        }

        public static IReadonlyDynamicArray<RaycastHit2D> CapsuleCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            CapsuleDirection2D capsuleDirection,
            float angle,
            Vector2 direction,
            ContactFilter2D contactFilter,
            DynamicArray<RaycastHit2D> results,
            float distance = float.PositiveInfinity)
        {
            int count;

            while (true)
            {
                count = Physics2D.CapsuleCast(origin,
                    size,
                    capsuleDirection,
                    angle,
                    direction,
                    contactFilter,
                    results.items,
                    distance);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.CapsuleCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> CapsuleCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            CapsuleDirection2D capsuleDirection,
            float angle,
            Vector2 direction)
        {
            return CapsuleCastNonAlloc(origin,
                size,
                capsuleDirection,
                angle,
                direction,
                DynamicArray<RaycastHit2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.CapsuleCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> CapsuleCastNonAlloc(
            Vector2 origin,
            Vector2 size,
            CapsuleDirection2D capsuleDirection,
            float angle,
            Vector2 direction,
            ContactFilter2D contactFilter,
            float distance = float.PositiveInfinity)
        {
            return CapsuleCastNonAlloc(origin,
                size,
                capsuleDirection,
                angle,
                direction,
                contactFilter,
                DynamicArray<RaycastHit2D>.Get(),
                distance);
        }

        #endregion

        #region OverlapBox

        public static IReadonlyDynamicArray<Collider2D> OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, DynamicArray<Collider2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return OverlapBoxNonAlloc(point,
                size,
                angle,
                legacyFilter,
                results);
        }

        public static IReadonlyDynamicArray<Collider2D> OverlapBoxNonAlloc(
            Vector2 point,
            Vector2 size,
            float angle,
            ContactFilter2D contactFilter,
            DynamicArray<Collider2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.OverlapBox(point,
                    size,
                    angle,
                    contactFilter,
                    results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapBoxNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle)
        {
            return OverlapBoxNonAlloc(point, size, angle, DynamicArray<Collider2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapBoxNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter)
        {
            return OverlapBoxNonAlloc(point,
                size,
                angle,
                contactFilter,
                DynamicArray<Collider2D>.Get());
        }

        #endregion

        #region CircleCast

        public static IReadonlyDynamicArray<RaycastHit2D> CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, DynamicArray<RaycastHit2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return CircleCastNonAlloc(origin,
                radius,
                direction,
                legacyFilter,
                results);
        }

        public static IReadonlyDynamicArray<RaycastHit2D> CircleCastNonAlloc(
            Vector2 origin,
            float radius,
            Vector2 direction,
            ContactFilter2D contactFilter,
            DynamicArray<RaycastHit2D> results,
            float distance = float.PositiveInfinity)
        {
            int count;

            while (true)
            {
                count = Physics2D.CircleCast(origin,
                    radius,
                    direction,
                    contactFilter,
                    results.items,
                    distance);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.CircleCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction)
        {
            return CircleCastNonAlloc(origin, radius, direction, DynamicArray<RaycastHit2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.CircleCastNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<RaycastHit2D> CircleCastNonAlloc(
            Vector2 origin,
            float radius,
            Vector2 direction,
            ContactFilter2D contactFilter,
            float distance = float.PositiveInfinity)
        {
            return CircleCastNonAlloc(origin,
                radius,
                direction,
                contactFilter,
                DynamicArray<RaycastHit2D>.Get(),
                distance);
        }

        #endregion

        #region OverlapArea

        public static IReadonlyDynamicArray<Collider2D> OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, DynamicArray<Collider2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return OverlapAreaNonAlloc(pointA, pointB, legacyFilter, results);
        }

        public static IReadonlyDynamicArray<Collider2D> OverlapAreaNonAlloc(
            Vector2 pointA,
            Vector2 pointB,
            ContactFilter2D contactFilter,
            DynamicArray<Collider2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.OverlapArea(pointA, pointB, contactFilter, results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapAreaNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB)
        {
            return OverlapAreaNonAlloc(pointA, pointB, DynamicArray<Collider2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapAreaNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, ContactFilter2D contactFilter)
        {
            return OverlapAreaNonAlloc(pointA, pointB, contactFilter, DynamicArray<Collider2D>.Get());
        }

        #endregion

        #region OverlapCapsule

        public static IReadonlyDynamicArray<Collider2D> OverlapCapsuleNonAlloc(
            Vector2 point,
            Vector2 size,
            CapsuleDirection2D direction,
            float angle,
            DynamicArray<Collider2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return OverlapCapsuleNonAlloc(point,
                size,
                direction,
                angle,
                legacyFilter,
                results);
        }

        public static IReadonlyDynamicArray<Collider2D> OverlapCapsuleNonAlloc(
            Vector2 point,
            Vector2 size,
            CapsuleDirection2D direction,
            float angle,
            ContactFilter2D contactFilter,
            DynamicArray<Collider2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.OverlapCapsule(point,
                    size,
                    direction,
                    angle,
                    contactFilter,
                    results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapCapsuleNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle)
        {
            return OverlapCapsuleNonAlloc(point,
                size,
                direction,
                angle,
                DynamicArray<Collider2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapCapsuleNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapCapsuleNonAlloc(
            Vector2 point,
            Vector2 size,
            CapsuleDirection2D direction,
            float angle,
            ContactFilter2D contactFilter)
        {
            return OverlapCapsuleNonAlloc(point,
                size,
                direction,
                angle,
                contactFilter,
                DynamicArray<Collider2D>.Get());
        }

        #endregion

        #region OverlapCircle

        public static IReadonlyDynamicArray<Collider2D> OverlapCircleNonAlloc(Vector2 point, float radius, DynamicArray<Collider2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return OverlapCircleNonAlloc(point, radius, legacyFilter, results);
        }

        public static IReadonlyDynamicArray<Collider2D> OverlapCircleNonAlloc(
            Vector2 point,
            float radius,
            ContactFilter2D contactFilter,
            DynamicArray<Collider2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.OverlapCircle(point, radius, contactFilter, results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapCircleNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapCircleNonAlloc(Vector2 point, float radius)
        {
            return OverlapCircleNonAlloc(point, radius, DynamicArray<Collider2D>.Get());
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapCircleNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapCircleNonAlloc(Vector2 point, float radius, ContactFilter2D contactFilter)
        {
            return OverlapCircleNonAlloc(point, radius, contactFilter, DynamicArray<Collider2D>.Get());
        }

        #endregion

        #region OverlapPoint

        public static IReadonlyDynamicArray<Collider2D> OverlapPointNonAlloc(Vector2 point, DynamicArray<Collider2D> results)
        {
            var legacyFilter = CreateLegacyFilter();
            return OverlapPointNonAlloc(point, legacyFilter, results);
        }

        public static IReadonlyDynamicArray<Collider2D> OverlapPointNonAlloc(Vector2 point, ContactFilter2D contactFilter, DynamicArray<Collider2D> results)
        {
            int count;

            while (true)
            {
                count = Physics2D.OverlapPoint(point, contactFilter, results.items);

                if (results.Capacity > count) break;

                results.ResizeMaintain(results.Capacity + results.CalculatePadding(results.Capacity));
            }

            results.Length = count;
            return results;
        }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapPointNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapPointNonAlloc(Vector2 point) { return OverlapPointNonAlloc(point, DynamicArray<Collider2D>.Get()); }

        /// <summary>
        /// Remember call dispose when DynamicArray create auto using pool
        /// <code>var results = Physics2DCast.OverlapPointNonAlloc(transform.position, Vector3.forward);
        /// ...
        /// results.Dispose();</code>
        /// </summary>
        public static IReadonlyDynamicArray<Collider2D> OverlapPointNonAlloc(Vector2 point, ContactFilter2D contactFilter)
        {
            return OverlapPointNonAlloc(point, contactFilter, DynamicArray<Collider2D>.Get());
        }

        #endregion
    }
}