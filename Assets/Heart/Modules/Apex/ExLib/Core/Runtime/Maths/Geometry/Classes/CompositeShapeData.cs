using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.ExLib.Maths
{
    public class CompositeShapeData
    {
        private struct LineSegment
        {
            public readonly Vector2 a;
            public readonly Vector2 b;

            public LineSegment(Vector2 a, Vector2 b)
            {
                this.a = a;
                this.b = b;
            }
        }

        private readonly Vector2[] points;
        private readonly Polygon polygon;
        private readonly int[] triangles;

        private List<CompositeShapeData> parents = new List<CompositeShapeData>();
        private List<CompositeShapeData> holes = new List<CompositeShapeData>();

        public bool IsValidShape { get; private set; }

        public CompositeShapeData(Vector3[] points)
        {
            this.points = points.Select(v => new Vector2(v.x, v.z)).ToArray();
            IsValidShape = points.Length >= 3 && !IntersectsWithSelf();

            if (IsValidShape)
            {
                polygon = new Polygon(this.points);
                Triangulator t = new Triangulator(polygon);
                triangles = t.Triangulate();
            }
        }

        /// <summary>
        /// Removes any holes which overlap with another hole.
        /// </summary>
        public void ValidateHoles()
        {
            for (int i = 0; i < holes.Count; i++)
            {
                for (int j = i + 1; j < holes.Count; j++)
                {
                    bool overlap = holes[i].OverlapsPartially(holes[j]);

                    if (overlap)
                    {
                        holes[i].IsValidShape = false;
                        break;
                    }
                }
            }

            for (int i = holes.Count - 1; i >= 0; i--)
            {
                if (!holes[i].IsValidShape)
                {
                    holes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// A parent is a shape which fully contains another shape
        /// </summary>
        public bool IsParentOf(CompositeShapeData otherShape)
        {
            if (otherShape.parents.Contains(this))
            {
                return true;
            }

            if (parents.Contains(otherShape))
            {
                return false;
            }

            // check if first point in otherShape is inside this shape. If not, parent test fails.
            // if yes, then continue to line seg intersection test between the two shapes

            // (this point test is important because without it, if all line seg intersection tests fail,
            // we wouldn't know if otherShape is entirely inside or entirely outside of this shape)
            bool pointInsideShape = false;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (Math2D.PointInTriangle(polygon.GetPoints()[triangles[i]],
                        polygon.GetPoints()[triangles[i + 1]],
                        polygon.GetPoints()[triangles[i + 2]],
                        otherShape.points[0]))
                {
                    pointInsideShape = true;
                    break;
                }
            }

            if (!pointInsideShape)
            {
                return false;
            }

            // Check for intersections between line segs of this shape and otherShape (any intersections will fail the parent test)
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment parentSeg = new LineSegment(points[i], points[(i + 1) % points.Length]);
                for (int j = 0; j < otherShape.points.Length; j++)
                {
                    LineSegment childSeg = new LineSegment(otherShape.points[j], otherShape.points[(j + 1) % otherShape.points.Length]);
                    if (Math2D.LineSegmentsIntersect(parentSeg.a, parentSeg.b, childSeg.a, childSeg.b))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Test if the shapes overlap partially (test will fail if one shape entirely contains other shape, i.e. one is parent of the other).
        /// </summary>
        public bool OverlapsPartially(CompositeShapeData otherShape)
        {
            // Check for intersections between line segs of this shape and otherShape (any intersection will validate the overlap test)
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment segA = new LineSegment(points[i], points[(i + 1) % points.Length]);
                for (int j = 0; j < otherShape.points.Length; j++)
                {
                    LineSegment segB = new LineSegment(otherShape.points[j], otherShape.points[(j + 1) % otherShape.points.Length]);
                    if (Math2D.LineSegmentsIntersect(segA.a, segA.b, segB.a, segB.b))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if any of the line segments making up this shape intersect
        /// </summary>
        public bool IntersectsWithSelf()
        {
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment segA = new LineSegment(points[i], points[(i + 1) % points.Length]);
                for (int j = i + 2; j < points.Length; j++)
                {
                    if ((j + 1) % points.Length == i)
                    {
                        continue;
                    }

                    LineSegment segB = new LineSegment(points[j], points[(j + 1) % points.Length]);
                    if (Math2D.LineSegmentsIntersect(segA.a, segA.b, segB.a, segB.b))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region [Getter / Setter]

        public Polygon GetPolygon() { return polygon; }

        public List<CompositeShapeData> GetParents() { return parents; }

        public List<CompositeShapeData> GetHoles() { return holes; }

        #endregion
    }
}