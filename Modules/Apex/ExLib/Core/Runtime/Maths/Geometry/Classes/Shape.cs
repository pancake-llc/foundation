using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ExLib.Maths
{
    [Serializable]
    public class Shape : IEnumerable, IList<Vector2>
    {
        [SerializeField] private List<Vector2> points = new List<Vector2>();

        public bool IsPointInside(Vector2 point) { return Math2D.PointInPolygon(point, points); }

        #region [IEnumerable Implementation]

        public IEnumerator GetEnumerator() { yield return GetEnumerator(); }

        IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator()
        {
            foreach (Vector2 point in points)
            {
                yield return point;
            }
        }

        #endregion

        #region [ICollection Implementation]

        public int Count => points.Count;

        private bool IsReadOnly => false;

        bool ICollection<Vector2>.IsReadOnly => throw new NotImplementedException();

        public void Add(Vector2 point) { points.Add(point); }

        public void Clear() { points.Clear(); }

        public bool Contains(Vector2 point) { return points.Contains(point); }

        public void CopyTo(Vector2[] array, int arrayIndex) { points.CopyTo(array, arrayIndex); }

        public bool Remove(Vector2 point) { return points.Remove(point); }

        #endregion

        #region [IList Implementation]

        public Vector2 this[int index] { get { return points[index]; } set { points[index] = value; } }

        public int IndexOf(Vector2 point) { return points.IndexOf(point); }

        public void Insert(int index, Vector2 point) { points.Insert(index, point); }

        public void RemoveAt(int index) { points.RemoveAt(index); }

        #endregion

        #region [Getter / Setter]

        public List<Vector2> GetPoints() { return points; }

        #endregion
    }
}