using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public abstract class DirectionalGrid
    {
        public abstract int CellCount { get; }

        protected abstract float[] Values { get; }
        protected abstract Vector3 GetDirection(int i);

        public Cell GetCell(int i) => new Cell(this, i);
        public abstract Cell GetCell(Vector3 direction);

        public abstract Vector3 GetMaxContinuous();

        public Cell GetMaxCell()
        {
            var res = GetCell(0);
            for (int i = 1; i < CellCount; i++)
            {
                var cell = GetCell(i);
                if (cell.Value > res.Value)
                {
                    res = cell;
                }
            }

            return res;
        }

        public Cell GetMinCell()
        {
            var res = GetCell(0);
            for (int i = 1; i < CellCount; i++)
            {
                var cell = GetCell(i);
                if (cell.Value < res.Value)
                {
                    res = cell;
                }
            }

            return res;
        }

        public void Fill(float v)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = v;
            }
        }

        public void GradientFill(Vector3 value, float falloff)
        {
            if (value == Vector3.zero)
            {
                return;
            }

            var direction = value.normalized;
            var v = value.magnitude;
            for (int i = 0; i < CellCount; i++)
            {
                var cell = GetCell(i);
                var dot = Vector3.Dot(direction, cell.Direction);
                var interp = Mathf.Lerp(0, 1, (dot - falloff) / (1f - falloff));
                cell.Value = Mathf.Max(cell.Value, v * interp);
            }
        }

        public void DrawGizmos(Vector3 p, float rayOffset, float rayScale)
        {
            for (int i = 0; i < CellCount; i++)
            {
                var cell = GetCell(i);
                var dir = cell.Direction;
                var start = p + dir * rayOffset;
                Gizmos.DrawLine(start, start + (dir * cell.Value * rayScale));
            }
        }

        public void Copy(DirectionalGrid other)
        {
            if (!CheckIsCompatible(this, other))
            {
                return;
            }

            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = other.Values[i];
            }
        }

        public void MaskUnder(DirectionalGrid mask, float max)
        {
            if (!CheckIsCompatible(this, mask))
            {
                return;
            }

            for (int i = 0; i < Values.Length; i++)
            {
                if (mask.Values[i] > max)
                {
                    Values[i] = 0;
                }
            }
        }

        public void InterpolateTo(DirectionalGrid target, float t)
        {
            if (!CheckIsCompatible(this, target))
            {
                return;
            }

            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = Mathf.Lerp(Values[i], target.Values[i], t);
            }
        }

        public void MultiplyScalar(float x)
        {
            for (int i = 0; i < this.Values.Length; i++)
            {
                Values[i] = Values[i] * x;
            }
        }

        public static bool CheckIsCompatible(DirectionalGrid g1, DirectionalGrid g2)
        {
            if (g1.GetType() != g2.GetType() || g1.Values.Length != g2.Values.Length)
            {
                Debug.LogError("Incompatible directional grids");
                return false;
            }

            return true;
        }

        public struct Cell
        {
            public DirectionalGrid Grid;
            public int I;

            public Cell(DirectionalGrid grid, int i)
            {
                Grid = grid;
                I = i;
            }

            public float Value { get => Grid.Values[I]; set => Grid.Values[I] = value; }
            public void SetValue(float v) => Value = v;
            public Vector3 Direction => Grid.GetDirection(I);
            public Vector3 VectorValue => Direction * Value;
        }
    }
}