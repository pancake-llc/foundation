using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class LOSUtils
    {
        static Vector3[] points = new Vector3[8];

        public static float MinAngleToBounds(Vector3 viewPos, Vector3 viewDir, Vector3 viewRight, Bounds bounds)
        {
            var center = bounds.center;
            var extents = bounds.extents;
            points[0] = center + new Vector3(extents.x, extents.y, extents.z);
            points[1] = center + new Vector3(extents.x, extents.y, -extents.z);
            points[2] = center + new Vector3(-extents.x, extents.y, extents.z);
            points[3] = center + new Vector3(-extents.x, extents.y, -extents.z);
            points[4] = center + new Vector3(extents.x, -extents.y, extents.z);
            points[5] = center + new Vector3(extents.x, -extents.y, -extents.z);
            points[6] = center + new Vector3(-extents.x, -extents.y, extents.z);
            points[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);

            float minProj = Mathf.Infinity;
            float maxProj = Mathf.NegativeInfinity;

            foreach (var point in points)
            {
                var delta = (point - viewPos);
                var proj = Vector3.Dot(delta, viewRight);
                var dist = Vector3.Dot(delta, viewDir);

                if (dist <= 0f)
                {
                    continue;
                }

                var normProj = proj / dist;

                if (normProj < minProj)
                {
                    minProj = normProj;
                }

                if (normProj > maxProj)
                {
                    maxProj = normProj;
                }
            }

            if (minProj < 0f && maxProj > 0f)
            {
                return 0f;
            }

            var closest = Mathf.Min(Mathf.Abs(minProj), Mathf.Abs(maxProj));
            return Mathf.Rad2Deg * Mathf.Abs(Mathf.Atan(closest));
        }

        public static float AngleToPoint(Vector3 viewPos, Vector3 viewDir, Vector3 viewRight, Vector3 target)
        {
            var delta = (target - viewPos);
            var proj = Vector3.Dot(delta, viewRight);
            var dist = Vector3.Dot(delta, viewDir);
            var normProj = (dist > 0) ? (proj / dist) : Mathf.Infinity;
            return Mathf.Rad2Deg * Mathf.Abs(Mathf.Atan(normProj));
        }

        public static void MapBoundsToEdges(Vector2 viewPos, Bounds bounds, List<Edge2D> storeIn)
        {
            var center = (Vector2) bounds.center;
            var extents = (Vector2) bounds.extents;

            if (viewPos.x > bounds.max.x)
            {
                storeIn.Add(new Edge2D(center + new Vector2(extents.x, -extents.y), center + new Vector2(extents.x, extents.y)));
            }
            else if (viewPos.x < bounds.min.x)
            {
                storeIn.Add(new Edge2D(center - new Vector2(extents.x, -extents.y), center - new Vector2(extents.x, extents.y)));
            }

            if (viewPos.y > bounds.max.y)
            {
                storeIn.Add(new Edge2D(center + new Vector2(-extents.x, extents.y), center + new Vector2(extents.x, extents.y)));
            }
            else if (viewPos.y < bounds.min.y)
            {
                storeIn.Add(new Edge2D(center - new Vector2(-extents.x, extents.y), center - new Vector2(extents.x, extents.y)));
            }
        }

        public static void MapBoundsToTriangles(Vector3 viewPos, Bounds bounds, List<Triangle> storeIn)
        {
            var center = bounds.center;
            var extents = bounds.extents;
            var ltf = new Vector3(-extents.x, extents.y, extents.z) + center;
            var rtf = new Vector3(extents.x, extents.y, extents.z) + center;
            var rtb = new Vector3(extents.x, extents.y, -extents.z) + center;
            var ltb = new Vector3(-extents.x, extents.y, -extents.z) + center;
            var lbf = ltf + (Vector3.down * 2f * extents.y);
            var rbf = rtf + (Vector3.down * 2f * extents.y);
            var rbb = rtb + (Vector3.down * 2f * extents.y);
            var lbb = ltb + (Vector3.down * 2f * extents.y);

            if (Vector3.Dot(viewPos - (bounds.center + Vector3.right * bounds.extents.x), Vector3.right) > 0)
            {
                storeIn.Add(new Triangle(rtb, rtf, rbf));
                storeIn.Add(new Triangle(rtb, rbf, rbb));
            }
            else if (Vector3.Dot(viewPos - (bounds.center - Vector3.right * bounds.extents.x), Vector3.left) > 0)
            {
                storeIn.Add(new Triangle(ltb, ltf, lbf));
                storeIn.Add(new Triangle(ltb, lbf, lbb));
            }

            if (Vector3.Dot(viewPos - (bounds.center + Vector3.up * bounds.extents.y), Vector3.up) > 0)
            {
                storeIn.Add(new Triangle(ltb, ltf, rtf));
                storeIn.Add(new Triangle(ltb, rtf, rtb));
            }
            else if (Vector3.Dot(viewPos - (bounds.center - Vector3.up * bounds.extents.y), Vector3.down) > 0)
            {
                storeIn.Add(new Triangle(lbb, lbf, rbf));
                storeIn.Add(new Triangle(lbb, rbf, rbb));
            }

            if (Vector3.Dot(viewPos - (bounds.center + Vector3.forward * bounds.extents.z), Vector3.forward) > 0)
            {
                storeIn.Add(new Triangle(rtf, ltf, lbf));
                storeIn.Add(new Triangle(rtf, lbf, rbf));
            }
            else if (Vector3.Dot(viewPos - (bounds.center - Vector3.forward * bounds.extents.z), Vector3.back) > 0)
            {
                storeIn.Add(new Triangle(rtb, ltb, lbb));
                storeIn.Add(new Triangle(rtb, lbb, rbb));
            }
        }

        static List<float> triangleAreas = new List<float>();

        public static Vector3 GetRandomPointInTriangles(List<Triangle> triangles, double[] sobolPosition)
        {
            if (triangles.Count == 0)
            {
                return default;
            }

            var totalArea = 0f;
            triangleAreas.Clear();
            foreach (var triangle in triangles)
            {
                totalArea += triangle.GetArea();
                triangleAreas.Add(totalArea);
            }

            var r = sobolPosition[2] * totalArea;
            for (int i = 0; i < triangleAreas.Count; i++)
            {
                if (triangleAreas[i] >= r)
                {
                    return triangles[i].GetRandomPoint(sobolPosition);
                }
            }

            return triangles[triangles.Count - 1].GetRandomPoint(sobolPosition);
        }

        static List<float> edgeLengths = new List<float>();

        public static Vector3 GetRandomPointOnEdges(List<Edge2D> edges, double[] sobolPosition)
        {
            if (edges.Count == 0)
            {
                return default;
            }

            var totalLength = 0f;
            edgeLengths.Clear();
            foreach (var edge in edges)
            {
                totalLength += edge.GetLength();
                edgeLengths.Add(totalLength);
            }

            var r = sobolPosition[1] * totalLength;
            for (int i = 0; i < edgeLengths.Count; i++)
            {
                if (edgeLengths[i] >= r)
                {
                    return edges[i].GetRandomPoint(sobolPosition);
                }
            }

            return edges[edges.Count - 1].GetRandomPoint(sobolPosition);
        }

        public static Vector3 RaycastBoundsOutPoint(Vector3 rayPoint, Vector3 rayDir, Bounds bounds)
        {
            Vector3 intPoint = rayPoint;
            float bestDistance = Mathf.Infinity;

            if (rayPoint.x < bounds.max.x && Vector3.Dot(rayDir, Vector3.right) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.max, Vector3.right);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }
            else if (rayPoint.x > bounds.min.x && Vector3.Dot(rayDir, Vector2.left) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.min, Vector3.left);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }

            if (rayPoint.y < bounds.max.y && Vector3.Dot(rayDir, Vector3.up) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.max, Vector3.up);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }
            else if (rayPoint.y > bounds.min.y && Vector3.Dot(rayDir, Vector3.down) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.min, Vector3.down);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }

            if (rayPoint.z < bounds.max.z && Vector3.Dot(rayDir, Vector3.forward) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.max, Vector3.forward);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }
            else if (rayPoint.z > bounds.min.z && Vector3.Dot(rayDir, Vector3.back) > 0f)
            {
                var distance = DistanceRayToPlane(rayPoint, rayDir, bounds.min, Vector3.back);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    intPoint = rayPoint + rayDir * distance;
                }
            }

            return intPoint;
        }

        static float DistanceRayToPlane(Vector3 rayPoint, Vector3 rayDir, Vector3 planePoint, Vector3 planeNormal)
        {
            var distToPlane = Vector3.Dot(rayPoint - planePoint, planeNormal);
            return distToPlane / (Vector3.Dot(-rayDir, planeNormal));
        }
    }
}