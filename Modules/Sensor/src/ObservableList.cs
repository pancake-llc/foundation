using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [Serializable]
    public class ObservableList<T> : Observable, IList<T>
    {
        public event Action<T> ItemAdded;
        public event Action<T> ItemRemoved;
        public override event Action OnChanged;

        [SerializeField] List<T> m_list = new List<T>();

        List<T> prevList = new List<T>();
        bool prevListInitialized = false;

        List<T> tempList = new List<T>();

        protected override string ValuePropName { get { return "m_list"; } }

        #region IList[T] implementation

        public int IndexOf(T value) { return m_list.IndexOf(value); }

        public void Insert(int index, T value)
        {
            m_list.Insert(index, value);
            syncToPrevList();
            ItemAdded?.Invoke(value);
            OnChanged?.Invoke();
        }

        public void RemoveAt(int index)
        {
            var item = m_list[index];
            m_list.RemoveAt(index);
            syncToPrevList();
            ItemRemoved?.Invoke(item);
            OnChanged?.Invoke();
        }

        public T this[int index]
        {
            get { return m_list[index]; }
            set
            {
                var prevItem = m_list[index];
                if (IsEqual(prevItem, value))
                {
                    return;
                }

                m_list[index] = value;
                syncToPrevList();
                ItemAdded?.Invoke(value);
                ItemRemoved?.Invoke(prevItem);
                OnChanged?.Invoke();
            }
        }

        #endregion

        #region IEnumerable implementation

        // Return List<T> explicit Enumerator to avoid garbage allocated in foreach loops
        public List<T>.Enumerator GetEnumerator() { return m_list.GetEnumerator(); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

        #region ICollection[T] implementation

        public void Add(T item)
        {
            m_list.Add(item);
            syncToPrevList();
            ItemAdded?.Invoke(item);
            OnChanged?.Invoke();
        }

        public void Clear()
        {
            tempList.Clear();
            tempList.AddRange(m_list);
            m_list.Clear();
            foreach (var item in tempList)
            {
                ItemRemoved?.Invoke(item);
            }

            syncToPrevList();
            OnChanged?.Invoke();
            tempList.Clear();
        }

        public bool Contains(T item) { return m_list.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { m_list.CopyTo(array, arrayIndex); }

        public bool Remove(T item)
        {
            var isRemoved = m_list.Remove(item);
            if (isRemoved)
            {
                syncToPrevList();
                ItemRemoved?.Invoke(item);
                OnChanged?.Invoke();
            }

            return isRemoved;
        }

        public int Count { get { return m_list.Count; } }

        public bool IsReadOnly { get { return false; } }

        #endregion

        bool IsEqual(T v1, T v2)
        {
            if (v1 is UnityEngine.Object || v2 is UnityEngine.Object)
            {
                return ReferenceEquals(v1, v2);
            }

            return (v1 == null && v2 == null) || v1 != null && v1.Equals(v2);
        }

        protected override void OnBeginGui() { syncToPrevList(); }

        public override void OnValidate()
        {
            if (prevListInitialized)
            {
                var nextList = m_list;
                m_list = prevList;
                prevList = tempList;
                tempList = nextList;

                for (int i = 0; i < nextList.Count; i++)
                {
                    if (i < m_list.Count)
                    {
                        this[i] = nextList[i];
                    }
                    else
                    {
                        Add(nextList[i]);
                    }
                }

                for (int i = m_list.Count - 1; i >= nextList.Count; i--)
                {
                    RemoveAt(i);
                }
            }
            else
            {
                syncToPrevList();
            }
        }

        void syncToPrevList()
        {
            // No need to do this stuff if not in the editor
            if (!Application.isEditor)
            {
                return;
            }

            for (int i = 0; i < m_list.Count; i++)
            {
                if (i < prevList.Count)
                {
                    prevList[i] = m_list[i];
                }
                else
                {
                    prevList.Add(m_list[i]);
                }
            }

            for (int i = prevList.Count - 1; i >= m_list.Count; i--)
            {
                prevList.RemoveAt(i);
            }

            prevListInitialized = true;
        }
    }
}