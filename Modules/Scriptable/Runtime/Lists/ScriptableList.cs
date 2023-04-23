using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Adds an item to the list only if its not in the list.
        /// Triggers OnItemCountChanged and OnItemAdded event.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (list.Contains(item)) return;

            list.Add(item);
            OnItemCountChanged?.Invoke();
            OnItemAdded?.Invoke(item);
        }

        /// <summary>
        /// Adds a range of items to the list. An item is only added if its not in the list.
        /// Triggers OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            var enumerable = items.ToList();
            foreach (var item in enumerable)
            {
                if (list.Contains(item)) return;
                list.Add(item);
            }

            OnItemCountChanged?.Invoke();
            OnItemsAdded?.Invoke(enumerable);
        }

        /// <summary>
        /// Removes an item from the list only if its in the list.
        /// Triggers OnItemCountChanged and OnItemRemoved event.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            if (!list.Contains(item)) return;

            list.Remove(item);
            OnItemCountChanged?.Invoke();
            OnItemRemoved?.Invoke(item);
        }

        /// <summary>
        /// Removes a range of items from the list.
        /// Triggers OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
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

        public override void Reset()
        {
            resetOn = ResetType.SceneLoaded;
            Clear();
        }

        public void ResetToInitialValue() => Clear();

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<Object> GetAllObjects()
        {
            var l = new List<Object>(Count);
            l.AddRange(list.OfType<Object>());
            return l;
        }
    }
}