using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public struct Edge2D
    {
        public Vector2 P1, P2;

        public Edge2D(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public float GetLength() => (P2 - P1).magnitude;

        public Edge2D ProjectCircle(Vector2 origin) => new Edge2D((P1 - origin).normalized + origin, (P2 - origin).normalized + origin);

        public Vector2 GetRandomPoint(double[] sobolPosition) => Vector2.Lerp(P1, P2, (float) sobolPosition[0]);

        public void DrawGizmos(float z) { Gizmos.DrawLine(new Vector3(P1.x, P1.y, z), new Vector3(P2.x, P2.y, z)); }

        public int Slice(Vector2 linePoint, Vector2 lineNormal, out Edge2D slice)
        {
            slice = default;

            var plane = new Plane(lineNormal, linePoint);

            var p1Dist = plane.GetDistanceToPoint(P1);
            var isP1Inside = p1Dist >= 0f;
            var p2Dist = plane.GetDistanceToPoint(P2);
            var isP2Inside = p2Dist >= 0f;

            if (isP1Inside && isP2Inside)
            {
                slice = this;
                return 1;
            }
            else if (!isP1Inside && !isP2Inside)
            {
                return 0;
            }

            Vector2 intersectPoint;
            EdgePlaneIntersection(out intersectPoint, this, lineNormal, linePoint);
            slice = new Edge2D(isP1Inside ? P1 : P2, intersectPoint);
            return 1;
        }

        static bool EdgePlaneIntersection(out Vector2 intersection, Edge2D edge, Vector2 planeNormal, Vector2 planePoint)
        {
            float length;
            float dotNumerator;
            float dotDenominator;
            Vector2 vector;
            intersection = Vector3.zero;

            var lineVec = (edge.P2 - edge.P1).normalized;

            dotNumerator = Vector3.Dot((planePoint - edge.P1), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;
                vector = SetVectorLength(lineVec, length);
                intersection = edge.P1 + vector;
                return true;
            }
            else
            {
                return false;
            }
        }

        static Vector2 SetVectorLength(Vector2 vector, float size)
        {
            Vector2 vectorNormalized = vector.normalized;
            return vectorNormalized *= size;
        }
    }
}