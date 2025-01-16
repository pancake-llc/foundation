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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
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

                if (results.TotalLength > count) break;

                results.ResizeMaintain(results.TotalLength + results.CalculatePadding(results.TotalLength));
            }

            results.Length = count;
            return results;
        }

        #endregion
    }
}