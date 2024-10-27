using System.Diagnostics;
using UnityEngine;

namespace Pancake.Common
{
    /// <summary> </summary>
    /// <remarks> Only available in the Editor. </remarks>
    public static class DebugDrawExtensions
    {
        /// <summary> Draw a point with a three-axis cross. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Vector3 self, float? size = null, Color? color = null) => DebugDraw.Point(self, size, Quaternion.identity, color);

        /// <summary> Draw an array of points using three-axis crosshairs. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Vector3[] self, float? size, Color? color = null) => DebugDraw.Points(self, size, Quaternion.identity, color);

        /// <summary>  Draw an arrow indicating the forward direction. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Transform self, float length = 1.0f, Color? color = null) =>
            DebugDraw.Arrow(self.position,
                self.rotation,
                length,
                DebugDraw.ARROW_TIP_SIZE,
                DebugDraw.ARROW_WIDTH,
                color);

        /// <summary> Draw bounds. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Bounds self, Color? color = null) => DebugDraw.Bounds(self, color);

        /// <summary> Draw bounds. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this BoundsInt self, Color? color = null) => DebugDraw.Bounds(new Bounds(self.center, self.size), color);

        /// <summary> Draw a ray. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Ray self, Color? color = null) => DebugDraw.Ray(self.origin, Quaternion.LookRotation(self.direction), color);

        /// <summary> Draw a ray with marks where there are impacts. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Ray self, RaycastHit hit, Color? color = null)
        {
            Quaternion rotation = Quaternion.LookRotation(hit.normal);

            DebugDraw.Circle(hit.point, DebugDraw.HIT_RADIUS * 0.5f, rotation, color ?? DebugDraw.HitColor);
            DebugDraw.Circle(hit.point, DebugDraw.HIT_RADIUS, rotation, color ?? DebugDraw.HitColor);
            DebugDraw.Line(hit.point, hit.point + hit.normal * DebugDraw.HIT_LENGTH, Quaternion.identity, color ?? DebugDraw.HitColor);
        }

        /// <summary> Draw a ray with marks where there are impacts. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Ray self, RaycastHit[] hits, int maxHits = 0, Color? color = null)
        {
            if (hits.Length > 0)
            {
                if (maxHits <= 0) maxHits = hits.Length;

                for (int i = 0; i < maxHits; ++i)
                {
                    Quaternion rotation = hits[i].normal.sqrMagnitude > Mathf.Epsilon ? Quaternion.LookRotation(hits[i].normal) : Quaternion.identity;

                    DebugDraw.Circle(hits[i].point, DebugDraw.HIT_RADIUS * 0.5f, rotation, color ?? DebugDraw.HitColor);
                    DebugDraw.Circle(hits[i].point, DebugDraw.HIT_RADIUS, rotation, color ?? DebugDraw.HitColor);
                    DebugDraw.Line(hits[i].point, hits[i].point + hits[i].normal * DebugDraw.HIT_LENGTH, Quaternion.identity, color ?? DebugDraw.HitColor);
                }
            }
        }

        /// <summary> Draw a collision with marks where there are impacts. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Collision self, Color? color = null)
        {
            int contacts = self.contactCount;
            for (int i = 0; i < contacts; ++i)
            {
                Quaternion rotation = self.contacts[i].normal.sqrMagnitude > Mathf.Epsilon ? Quaternion.LookRotation(self.contacts[i].normal) : Quaternion.identity;

                DebugDraw.Circle(self.contacts[i].point, DebugDraw.HIT_RADIUS * 0.5f, rotation, color ?? DebugDraw.HitColor);
                DebugDraw.Circle(self.contacts[i].point, DebugDraw.HIT_RADIUS, rotation, color ?? DebugDraw.HitColor);
                DebugDraw.Line(self.gameObject.transform.position, self.contacts[i].point, Quaternion.identity, color ?? DebugDraw.HitColor);
            }
        }

        /// <summary> Draw the bounds of the collider. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Draw(this Collider self) => DebugDraw.Bounds(self.bounds);

        /// <summary> Draw the name of the GameObject. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawName(this GameObject self, GUIStyle style = null) => DebugDraw.Text(self.transform.position, self.name, style);
    }
}