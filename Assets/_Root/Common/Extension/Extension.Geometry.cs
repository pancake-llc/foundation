using System.Collections.Generic;

namespace Pancake
{
    /// <summary>
    /// A helper class to handle geometry related operations    
    /// </summary>    
    public static partial class C
    {
        // Based on https://answers.unity.com/questions/1019436/get-outeredge-vertices-c.html
        public struct Edge
        {
            public int vertice1;
            public int vertice2;
            public int triangleIndex;

            public Edge(int aV1, int aV2, int aIndex)
            {
                vertice1 = aV1;
                vertice2 = aV2;
                triangleIndex = aIndex;
            }
        }

        public static List<Edge> GetEdges(int[] indices)
        {
            List<Edge> edgeList = new List<Edge>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                int vertice1 = indices[i];
                int vertice2 = indices[i + 1];
                int vertice3 = indices[i + 2];
                edgeList.Add(new Edge(vertice1, vertice2, i));
                edgeList.Add(new Edge(vertice2, vertice3, i));
                edgeList.Add(new Edge(vertice3, vertice1, i));
            }

            return edgeList;
        }

        public static List<Edge> FindBoundary(this List<Edge> edges)
        {
            List<Edge> edgeList = new List<Edge>(edges);
            for (int i = edgeList.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    // if we find a shared edge we remove both
                    if (edgeList[i].vertice1 == edgeList[n].vertice2 && edgeList[i].vertice2 == edgeList[n].vertice1)
                    {
                        edgeList.RemoveAt(i);
                        edgeList.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }

            return edgeList;
        }

        public static List<Edge> SortEdges(this List<Edge> edges)
        {
            List<Edge> edgeList = new List<Edge>(edges);
            for (int i = 0; i < edgeList.Count - 2; i++)
            {
                Edge e = edgeList[i];
                for (int n = i + 1; n < edgeList.Count; n++)
                {
                    Edge a = edgeList[n];
                    if (e.vertice2 == a.vertice1)
                    {
                        if (n == i + 1)
                        {
                            // if they're already in order, we move on
                            break;
                        }
                        else
                        {
                            // otherwise we swap
                            edgeList[n] = edgeList[i + 1];
                            edgeList[i + 1] = a;
                            break;
                        }
                    }
                }
            }

            return edgeList;
        }
    }
}