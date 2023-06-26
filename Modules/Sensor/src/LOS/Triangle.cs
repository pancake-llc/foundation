using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public struct Triangle
    {
        public Vector3 P1, P2, P3;
        public Vector3 N => Vector3.Cross(P2 - P1, P3 - P2).normalized;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public float GetArea()
        {
            float res = Mathf.Pow(((P2.x * P1.y) - (P3.x * P1.y) - (P1.x * P2.y) + (P3.x * P2.y) + (P1.x * P3.y) - (P2.x * P3.y)), 2.0f);
            res += Mathf.Pow(((P2.x * P1.z) - (P3.x * P1.z) - (P1.x * P2.z) + (P3.x * P2.z) + (P1.x * P3.z) - (P2.x * P3.z)), 2.0f);
            res += Mathf.Pow(((P2.y * P1.z) - (P3.y * P1.z) - (P1.y * P2.z) + (P3.y * P2.z) + (P1.y * P3.z) - (P2.y * P3.z)), 2.0f);
            return Mathf.Sqrt(res) * 0.5f;
        }

        public Triangle ProjectSphere(Vector3 origin)
        {
            return new Triangle((P1 - origin).normalized + origin, (P2 - origin).normalized + origin, (P3 - origin).normalized + origin);
        }

        public Vector3 GetRandomPoint(double[] sobolPosition)
        {
            var a = P2 - P1;
            var b = P3 - P1;
            var u1 = (float) sobolPosition[0];
            var u2 = (float) sobolPosition[1];
            if (u1 + u2 > 1)
            {
                u1 = 1 - u1;
                u2 = 1 - u2;
            }

            var w = (u1 * a) + (u2 * b);
            return w + P1;
        }

        public void DrawGizmos()
        {
            Gizmos.DrawLine(P1, P2);
            Gizmos.DrawLine(P2, P3);
            Gizmos.DrawLine(P3, P1);
        }

        public int Slice(Vector3 planePoint, Vector3 planeNormal, out Triangle slice1, out Triangle slice2)
        {
            slice1 = default;
            slice2 = default;

            int nInside = 0, nOutside = 0;
            Vector3 inPt1 = default, inPt2 = default;
            Vector3 outPt1 = default, outPt2 = default;

            var p1Dist = Vector3.Dot(P1 - planePoint, planeNormal);
            ClassifyPoint(P1,
                p1Dist,
                ref nInside,
                ref nOutside,
                ref inPt1,
                ref inPt2,
                ref outPt1,
                ref outPt2);
            var p2Dist = Vector3.Dot(P2 - planePoint, planeNormal);
            ClassifyPoint(P2,
                p2Dist,
                ref nInside,
                ref nOutside,
                ref inPt1,
                ref inPt2,
                ref outPt1,
                ref outPt2);
            var p3Dist = Vector3.Dot(P3 - planePoint, planeNormal);
            ClassifyPoint(P3,
                p3Dist,
                ref nInside,
                ref nOutside,
                ref inPt1,
                ref inPt2,
                ref outPt1,
                ref outPt2);

            if (nOutside == 0)
            {
                // Completely inside plane
                slice1 = this;
                return 1;
            }

            if (nInside == 0)
            {
                // Completely outside plane
                return 0;
            }

            Vector3 edgeInt1, edgeInt2;
            LinePlaneIntersection(out edgeInt1,
                inPt1,
                (outPt1 - inPt1).normalized,
                planeNormal,
                planePoint);
            if (nInside > 1)
            {
                LinePlaneIntersection(out edgeInt2,
                    inPt2,
                    (outPt1 - inPt2).normalized,
                    planeNormal,
                    planePoint);
                slice1 = new Triangle {P1 = inPt1, P2 = edgeInt1, P3 = edgeInt2};
                slice2 = new Triangle {P1 = inPt1, P2 = inPt2, P3 = edgeInt2};
                return 2;
            }
            else
            {
                LinePlaneIntersection(out edgeInt2,
                    inPt1,
                    (outPt2 - inPt1).normalized,
                    planeNormal,
                    planePoint);
                slice1 = new Triangle {P1 = inPt1, P2 = edgeInt1, P3 = edgeInt2};
                return 1;
            }
        }

        // I had originally made all these refs private vars, but structs with >40 bytes have poor peformance.
        // So to get under 40 bytes I made them all refs.
        void ClassifyPoint(Vector3 p, float d, ref int nInside, ref int nOutside, ref Vector3 inPt1, ref Vector3 inPt2, ref Vector3 outPt1, ref Vector3 outPt2)
        {
            if (d >= 0f)
            {
                if (nInside == 0) inPt1 = p;
                else if (nInside == 1) inPt2 = p;
                nInside += 1;
            }
            else
            {
                if (nOutside == 0) outPt1 = p;
                else if (nOutside == 1) outPt2 = p;
                nOutside += 1;
            }
        }

        static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {
            float length;
            float dotNumerator;
            float dotDenominator;
            Vector3 vector;
            intersection = Vector3.zero;

            dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;
                vector = SetVectorLength(lineVec, length);
                intersection = linePoint + vector;
                return true;
            }
            else
            {
                return false;
            }
        }

        static Vector3 SetVectorLength(Vector3 vector, float size)
        {
            Vector3 vectorNormalized = Vector3.Normalize(vector);
            return vectorNormalized *= size;
        }
    }
}