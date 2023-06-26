using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public class CircleGrid : DirectionalGrid
    {
        [SerializeField] int gridSize;
        [SerializeField] float[] vs;
        [SerializeField] Vector3 axis;

        public int GridSize => gridSize;
        public override int CellCount => GridSize;

        protected override float[] Values => vs;

        protected override Vector3 GetDirection(int i)
        {
            var a = (i + .5f) * CellSize;
            return Quaternion.FromToRotation(Vector3.up, axis) * new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
        }

        float CellExtents => Mathf.PI / GridSize;
        float CellSize => 2 * Mathf.PI / GridSize;

        public CircleGrid(Vector3 axis, int gridSize)
        {
            this.axis = axis;
            this.gridSize = gridSize;
            vs = new float[gridSize];
        }

        public override Cell GetCell(Vector3 dir)
        {
            var localDir = Vector3.ProjectOnPlane(dir, axis);
            var a = Vector3.SignedAngle(Vector3.up, localDir, axis);
            if (a < 0)
            {
                a += 360;
            }

            return GetCell(Mathf.FloorToInt(a / CellSize));
        }

        public override Vector3 GetMaxContinuous()
        {
            if (CellCount == 0)
            {
                return Vector3.zero;
            }

            var max = GetMaxCell();
            var value = max.Value;
            var coords = new Coords(GridSize, max.I);
            var offset = SubCellOffset(value, GetCell(coords.ShiftLeft().I).Value, GetCell(coords.ShiftRight().I).Value, out value);
            var dir = max.Direction;
            var right = Vector3.Cross(axis, dir);
            var offdir = (dir + (right * offset)).normalized;
            return offdir * value;
        }

        float SubCellOffset(float v, float prev, float next, out float subValue)
        {
            float xmin, xmax, xdir;
            if (prev < next)
            {
                xmin = prev;
                xmax = next;
                xdir = 1;
            }
            else
            {
                xmin = next;
                xmax = prev;
                xdir = -1;
            }

            var xinterp = Mathf.InverseLerp(0, v - xmin, xmax - xmin);
            subValue = Mathf.LerpUnclamped(prev, v, 1f + (xinterp / 2f));
            var xoffset = xdir * xinterp * CellExtents;
            return xoffset;
        }

        public struct Coords
        {
            public int Size;
            public int I;

            public Coords(int size, int i)
            {
                Size = size;
                I = i;
            }

            public Coords ShiftLeft()
            {
                var i = I - 1;
                if (i < 0)
                {
                    i = Size - 1;
                }

                return new Coords(Size, i);
            }

            public Coords ShiftRight()
            {
                var i = I + 1;
                if (i >= Size)
                {
                    i = 0;
                }

                return new Coords(Size, i);
            }
        }
    }
}