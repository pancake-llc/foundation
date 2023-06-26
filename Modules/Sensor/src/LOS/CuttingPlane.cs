using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public struct CuttingPlane
    {
        public Vector3 Point;
        public Vector3 Normal;

        public void Cut(List<Triangle> triangleList)
        {
            for (int i = triangleList.Count - 1; i >= 0; i--)
            {
                var tri = triangleList[i];
                Triangle slice1, slice2;
                var nSlices = tri.Slice(Point, Normal, out slice1, out slice2);
                if (nSlices == 0)
                {
                    triangleList.RemoveAt(i);
                }
                else
                {
                    triangleList[i] = slice1;
                    if (nSlices > 1)
                    {
                        triangleList.Add(slice2);
                    }
                }
            }
        }

        public void Cut(List<Edge2D> edgeList)
        {
            for (int i = edgeList.Count - 1; i >= 0; i--)
            {
                var edge = edgeList[i];
                Edge2D slicedEdge;
                var nSlices = edge.Slice(Point, ((Vector2) Normal).normalized, out slicedEdge);
                if (nSlices == 0)
                {
                    edgeList.RemoveAt(i);
                }
                else
                {
                    edgeList[i] = slicedEdge;
                }
            }
        }
    }
}