using UnityEngine;

namespace Pancake.ExLib.Maths
{
    public class Polygon
    {
        private readonly Vector2[] points;
        private readonly int numPoints;

        private readonly int numHullPoints;

        private readonly int[] numPointsPerHole;
        private readonly int numHoles;

        private readonly int[] holeStartIndices;

        public Polygon(Vector2[] hull, Vector2[][] holes)
        {
            numHullPoints = hull.Length;
            numHoles = holes.GetLength(0);

            numPointsPerHole = new int[numHoles];
            holeStartIndices = new int[numHoles];
            int numHolePointsSum = 0;

            for (int i = 0; i < holes.GetLength(0); i++)
            {
                numPointsPerHole[i] = holes[i].Length;

                holeStartIndices[i] = numHullPoints + numHolePointsSum;
                numHolePointsSum += numPointsPerHole[i];
            }

            numPoints = numHullPoints + numHolePointsSum;
            points = new Vector2[numPoints];


            // add hull points, ensuring they wind in counterclockwise order
            bool reverseHullPointsOrder = !PointsAreCounterClockwise(hull);
            for (int i = 0; i < numHullPoints; i++)
            {
                points[i] = hull[(reverseHullPointsOrder) ? numHullPoints - 1 - i : i];
            }

            // add hole points, ensuring they wind in clockwise order
            for (int i = 0; i < numHoles; i++)
            {
                bool reverseHolePointsOrder = PointsAreCounterClockwise(holes[i]);
                for (int j = 0; j < holes[i].Length; j++)
                {
                    points[IndexOfPointInHole(j, i)] = holes[i][(reverseHolePointsOrder) ? holes[i].Length - j - 1 : j];
                }
            }
        }

        public Polygon(Vector2[] hull)
            : this(hull, new Vector2[0][])
        {
        }

        private bool PointsAreCounterClockwise(Vector2[] testPoints)
        {
            float signedArea = 0;
            for (int i = 0; i < testPoints.Length; i++)
            {
                int nextIndex = (i + 1) % testPoints.Length;
                signedArea += (testPoints[nextIndex].x - testPoints[i].x) * (testPoints[nextIndex].y + testPoints[i].y);
            }

            return signedArea < 0;
        }

        public int IndexOfFirstPointInHole(int holeIndex) { return holeStartIndices[holeIndex]; }

        public int IndexOfPointInHole(int index, int holeIndex) { return holeStartIndices[holeIndex] + index; }

        public Vector2 GetHolePoint(int index, int holeIndex) { return points[holeStartIndices[holeIndex] + index]; }

        #region [Getter / Setter]

        public Vector2[] GetPoints() { return points; }

        public int GetPointsCount() { return numPoints; }

        public int GetHullPointsCount() { return numHoles; }

        public int[] GetPointsPerHoleCount() { return numPointsPerHole; }

        public int GetHolesCount() { return numHoles; }

        #endregion
    }
}