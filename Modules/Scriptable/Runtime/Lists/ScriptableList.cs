using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    public class ScriptableList<T> : ScriptableListBase, IReset, IEnumerable<T>, IDrawObjectsInInspector
    {
        [Tooltip(
            "Clear the list when:  Scene Loaded : when a scene is loaded.  Application Start : Once, when the application starts. Modifications persists between scenes")]
        [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

        [SerializeField] protected List<T> list = new List<T>();

        public int Count => list.Count;
        public bool IsEmpty => !(list.Count > 0);
        public override Type GetElementType => typeof(T);

        public T this[int index] { get => list[index]; set => list[index] = value; }

        public event Action OnItemCountChanged;
        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;
        public event Action<IEnumerable<T>> OnItemsAdded;
        public event Action<IEnumerable<T>> OnItemsRemoved;
        public event Action OnCleared;

        public void Add(T item)
        {
            if (list.Contains(item)) return;

            list.Add(item);
            OnItemCountChanged?.Invoke();
            OnItemAdded?.Invoke(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (list.Contains(item)) return;
                list.Add(item);
            }

            OnItemCountChanged?.Invoke();
            OnItemsAdded?.Invoke(items);
        }

        public void Remove(T item)
        {
            if (!list.Contains(item)) return;

            list.Remove(item);
            OnItemCountChanged?.Invoke();
            OnItemRemoved?.Invoke(item);
        }

        public void RemoveRange(int index, int count)
        {
            var items = list.GetRange(index, count);
            list.RemoveRange(index, count);
            OnItemCountChanged?.Invoke();
            OnItemsRemoved?.Invoke(items);
        }


        public void Clear()
        {
            list.Clear();
            OnCleared?.Invoke();
        }

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            Clear();

            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) Clear();
        }

        public void Reset() { Clear(); }

        public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public List<Object> GetAllObjects()
        {
            var l = new List<Object>(Count);
            foreach (var t in list)
            {
                var obj = t as Object;
                if (obj != null) l.Add(obj);
            }

            return l;
        }
    }
}