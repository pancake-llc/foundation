using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.ExLib.Maths
{
    public class CompositeShape
    {
        private Vector3[] vertices;
        private int[] triangles;

        private Shape[] shapes;
        private float height = 0;

        public CompositeShape(IEnumerable<Shape> shapes) { this.shapes = shapes.ToArray(); }

        public Mesh GetMesh()
        {
            Process();

            return new Mesh() {vertices = vertices, triangles = triangles, normals = vertices.Select(x => Vector3.up).ToArray()};
        }

        public void Process()
        {
            // Generate array of valid shape data
            CompositeShapeData[] eligibleShapes = shapes.Select(s => new CompositeShapeData(s.GetPoints().Select(v => new Vector3(v.x, 0, v.y)).ToArray()))
                .Where(c => c.IsValidShape)
                .ToArray();

            // Set parents for all shapes. A parent is a shape which completely contains another shape.
            for (int i = 0; i < eligibleShapes.Length; i++)
            {
                for (int j = 0; j < eligibleShapes.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (eligibleShapes[i].IsParentOf(eligibleShapes[j]))
                    {
                        eligibleShapes[j].GetParents().Add(eligibleShapes[i]);
                    }
                }
            }

            // Holes are shapes with an odd number of parents.
            CompositeShapeData[] holeShapes = eligibleShapes.Where(x => x.GetParents().Count % 2 != 0).ToArray();
            foreach (CompositeShapeData holeShape in holeShapes)
            {
                // The most immediate parent (i.e the smallest parent shape) will be the one that has the highest number of parents of its own. 
                CompositeShapeData immediateParent = holeShape.GetParents().OrderByDescending(x => x.GetParents().Count).First();
                immediateParent.GetHoles().Add(holeShape);
            }

            // Solid shapes have an even number of parents
            CompositeShapeData[] solidShapes = eligibleShapes.Where(x => x.GetParents().Count % 2 == 0).ToArray();
            foreach (CompositeShapeData solidShape in solidShapes)
            {
                solidShape.ValidateHoles();
            }

            // Create polygons from the solid shapes and their associated hole shapes
            Polygon[] polygons = solidShapes.Select(x => new Polygon(x.GetPolygon().GetPoints(), x.GetHoles().Select(h => h.GetPolygon().GetPoints()).ToArray()))
                .ToArray();

            // Flatten the points arrays from all polygons into a single array, and convert the vector2s to vector3s.
            vertices = polygons.SelectMany(x => x.GetPoints().Select(v2 => new Vector3(v2.x, height, v2.y))).ToArray();

            // Triangulate each polygon and flatten the triangle arrays into a single array.
            List<int> allTriangles = new List<int>();
            int startVertexIndex = 0;
            for (int i = 0; i < polygons.Length; i++)
            {
                Triangulator triangulator = new Triangulator(polygons[i]);
                int[] polygonTriangles = triangulator.Triangulate();

                for (int j = 0; j < polygonTriangles.Length; j++)
                {
                    allTriangles.Add(polygonTriangles[j] + startVertexIndex);
                }

                startVertexIndex += polygons[i].GetPointsCount();
            }

            triangles = allTriangles.ToArray();
        }
    }
}