using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pancake.GrowMeshEditor
{
    public static class GrowMesh
    {
        private static Vector3[] iVertices; //save vertices data info
        private static int[] iFaces; // face
        private static Vector2[] iUVs; // uvs
        private static Vertex[] mVertices; //Vertices
        private static Dictionary<string, Edge> EdgesDic; // edge info
        private static Dictionary<string, int> Map_Edge_Vertex;
        private static Dictionary<string, List<EdgeVertex>> Map_Pos_EdgeVertex;
        private static readonly string edgekey = "{0}_{1}";
        private static bool hasUvs = false;

        private static Dictionary<string, List<int>> PointMap;

        //Concatenate string data.
        private static string Vector2String(Vector3 v)
        {
            var str = new StringBuilder();
            str.Append(v.x).Append(",").Append(v.y).Append(",").Append(v.z);
            return str.ToString();
        }

        //to convert
        private static Vector3 String2Vector(string vstr)
        {
            try
            {
                string[] strings = vstr.Split(',');
                return new Vector3(float.Parse(strings[0]), float.Parse(strings[1]), float.Parse(strings[2]));
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return Vector3.zero;
            }
        }

        private static float sourceVertexWeight = 0;
        private static float connectingVertexWeight = 0;
        private static float edgeVertexWeight = 0.375f;
        private static float adjacentVertexWeight = 0.125f;

        //Add each vertices to the map.
        private static void BuildMapInfo(Mesh mesh)
        {
            PointMap = new Dictionary<string, List<int>>(0);
            for (int i = 0, l = mesh.vertices.Length; i < l; i++)
            {
                string vstr = Vector2String(mesh.vertices[i]);
                if (!PointMap.ContainsKey(vstr))
                {
                    PointMap.Add(vstr, new List<int>());
                }

                PointMap[vstr].Add(i);
            }
        }

        //Here is the code for the generation.
        public static void Gen(Mesh mesh)
        {
            iVertices = mesh.vertices;
            iFaces = mesh.triangles;
            iUVs = mesh.uv;
            if (iUVs.Length > 0)
                hasUvs = true;
            mVertices = new Vertex[iVertices.Length];
            EdgesDic = new Dictionary<string, Edge>(0);
            BuildMapInfo(mesh);
            GenLook();
            Map_Pos_EdgeVertex = new Dictionary<string, List<EdgeVertex>>(0);
            Map_Edge_Vertex = new Dictionary<string, int>();
            var newEdgeVertices = new Vector3[EdgesDic.Count];

            var index = 0;
            foreach (var e in EdgesDic)
            {
                var currentEdge = e.Value;
                var newEdgeVertex = Vector3.zero;
                newEdgeVertex = iVertices[currentEdge.a] + iVertices[currentEdge.b];
                Map_Edge_Vertex[e.Key] = index;

                string vstr = Vector2String(newEdgeVertex);
                if (!Map_Pos_EdgeVertex.ContainsKey(vstr))
                {
                    Map_Pos_EdgeVertex.Add(vstr, new List<EdgeVertex>());
                }

                Map_Pos_EdgeVertex[vstr].Add(new EdgeVertex(index, currentEdge));

                newEdgeVertices[index++] = newEdgeVertex;
            }

            foreach (var evs in Map_Pos_EdgeVertex)
            {
                var edgeVertices = evs.Value;
                var connectedFaces = 0;
                var tmp1 = String2Vector(evs.Key);
                var tmp2 = Vector3.zero;
                foreach (var ev in edgeVertices)
                {
                    int c = ev.edge.adjVertex.Count;
                    connectedFaces += c;
                    for (var j = 0; j < c; j++)
                    {
                        int v = ev.edge.adjVertex[j];
                        tmp2 += iVertices[v];
                    }
                }

                SetWeight1(connectedFaces);
                var newPos = tmp1 * edgeVertexWeight + tmp2 * adjacentVertexWeight;
                foreach (var ev in edgeVertices)
                {
                    newEdgeVertices[ev.edgeVertexIndex] = newPos;
                }
            }

            var newSourceVertices = new Vector3[iVertices.Length];

            foreach (var point in PointMap)
            {
                var PointsAtPos = point.Value;
                var oldVertex = iVertices[PointsAtPos[0]];
                var ConnPointPosAve = Vector3.zero;
                var n = 0;
                foreach (var p in PointsAtPos)
                {
                    var mVertex = mVertices[p];
                    var vlist = mVertex.conVertexIndices;
                    n += vlist.Count;
                    foreach (var v in vlist)
                    {
                        ConnPointPosAve += iVertices[v];
                    }
                }

                W2(n);

                var newSourceVertex = Vector3.zero;
                newSourceVertex += (oldVertex * sourceVertexWeight);

                ConnPointPosAve *= connectingVertexWeight;
                newSourceVertex += ConnPointPosAve;

                foreach (var p in PointsAtPos)
                {
                    newSourceVertices[p] = newSourceVertex;
                }
            }

            int sl = newSourceVertices.Length;
            var newVertices = newSourceVertices.Concat(newEdgeVertices).ToArray<Vector3>();
            var newsUVInfo = new Vector2[newVertices.Length];
            var newTriangles = new int[iFaces.Length * 4];
            for (int i = 0, l = iFaces.Length; i < l; i += 3)
            {
                int va = iFaces[i];
                int vb = iFaces[i + 1];
                int vc = iFaces[i + 2];

                int v1 = Map_Edge_Vertex[EdgeInfoGetKey(va, vb)] + sl;
                int v2 = Map_Edge_Vertex[EdgeInfoGetKey(vb, vc)] + sl;
                int v3 = Map_Edge_Vertex[EdgeInfoGetKey(vc, va)] + sl;
                BuildFace(newTriangles,
                    i * 4,
                    v1,
                    v2,
                    v3);
                BuildFace(newTriangles,
                    i * 4 + 3,
                    va,
                    v1,
                    v3);
                BuildFace(newTriangles,
                    i * 4 + 6,
                    vb,
                    v2,
                    v1);
                BuildFace(newTriangles,
                    i * 4 + 9,
                    vc,
                    v3,
                    v2);
                if (hasUvs)
                {
                    newsUVInfo[va] = iUVs[va];
                    newsUVInfo[vb] = iUVs[vb];
                    newsUVInfo[vc] = iUVs[vc];
                    newsUVInfo[v1] = new Vector2((iUVs[va].x + iUVs[vb].x) * 0.5f, (iUVs[va].y + iUVs[vb].y) * 0.5f);
                    newsUVInfo[v2] = new Vector2((iUVs[vb].x + iUVs[vc].x) * 0.5f, (iUVs[vb].y + iUVs[vc].y) * 0.5f);
                    newsUVInfo[v3] = new Vector2((iUVs[vc].x + iUVs[va].x) * 0.5f, (iUVs[vc].y + iUVs[va].y) * 0.5f);
                }
            }

            mesh.vertices = newVertices;
            mesh.triangles = newTriangles;
            if (hasUvs)
            {
                mesh.uv = newsUVInfo;
            }

            mesh.SetIndices(newTriangles, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        //Make sure the weight is correct
        private static void SetWeight1(int n)
        {
            edgeVertexWeight = 0.375f;
            adjacentVertexWeight = 0.125f;
            if (n != 2)
            {
                edgeVertexWeight = 0.5f;
                adjacentVertexWeight = 0;
                if (n != 1)
                {
                }
            }
        }

        //Execute encryption algorithm
        private static void W2(int n)
        {
            var beta = 0f;
            if (n == 3)
            {
                beta = 0.1875f;
            }
            else if (n > 3)
            {
                beta = 0.375f / n;
            }

            // Loop's original beta formula
            // beta = 1 / n * ( 5/8 - Math.pow( 3/8 + 1/4 * Math.cos( 2 * Math. PI / n ), 2) );

            sourceVertexWeight = 1f - n * beta;
            connectingVertexWeight = beta;

            if (n <= 2)
            {
                if (n == 2)
                {
                    sourceVertexWeight = 0.75f;
                    connectingVertexWeight = 0.125f;
                }
                else if (n == 1)
                {
                }
                else if (n == 0)
                {
                }
            }
        }

        //Establish surface information.
        private static void BuildFace(int[] newTri, int index, int a, int b, int c)
        {
            newTri[index] = a;
            newTri[index + 1] = b;
            newTri[index + 2] = c;
        }

        //convert
        private static string EdgeInfoGetKey(int a, int b)
        {
            var vertexIndexA = Mathf.Min(a, b);
            var vertexIndexB = Mathf.Max(a, b);
            return string.Format(edgekey, vertexIndexA, vertexIndexB);
        }

        //Analyze orientation information.
        private static void GenLook()
        {
            for (int i = 0, l = mVertices.Length; i < l; i++)
            {
                mVertices[i].conVertexIndices = new List<int>(0);
            }

            for (int i = 0, l = iFaces.Length; i < l; i += 3)
            {
                BuildEdge(iFaces[i], iFaces[i + 1], iFaces[i + 2]);
                BuildEdge(iFaces[i + 1], iFaces[i + 2], iFaces[i]);
                BuildEdge(iFaces[i + 2], iFaces[i], iFaces[i + 1]);
            }
        }

        //Build Edge 
        private static void BuildEdge(int a, int b, int c)
        {
            string key = EdgeInfoGetKey(a, b);

            Edge edge;

            if (EdgesDic.ContainsKey(key))
            {
                edge = EdgesDic[key];
            }
            else
            {
                edge = new Edge(a, b);
                EdgesDic[key] = edge;
            }

            edge.adjVertex.Add(c);
            mVertices[a].conVertexIndices.Add(b);
            mVertices[b].conVertexIndices.Add(a);
        }

        struct Vertex
        {
            public List<int> conVertexIndices;
        }

        struct EdgeVertex
        {
            public EdgeVertex(int n, Edge e)
            {
                edgeVertexIndex = n;
                edge = e;
            }

            public int edgeVertexIndex;
            public Edge edge;
        }

        struct Edge
        {
            public Edge(int _a, int _b)
            {
                a = _a;
                b = _b;
                adjVertex = new List<int>(0);
            }

            public int a;
            public int b;
            public List<int> adjVertex;
        }
    }
}