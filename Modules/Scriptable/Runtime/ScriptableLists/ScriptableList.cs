using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    public abstract class ScriptableList<T> : ScriptableListBase, IReset, IEnumerable<T>, IDrawObjectsInInspector
    {
        [Tooltip("Clear the list when:" + " Scene Loaded : when a scene is loaded." +
                 " Application Start : Once, when the application starts. Modifications persists between scenes")]
        [SerializeField]
        private ResetType _resetOn = ResetType.SceneLoaded;

        [SerializeField] protected List<T> _list = new List<T>();

        public int Count => _list.Count;
        public bool IsEmpty => !_list.Any();
        public override Type GetElementType => typeof(T);

        //feel free to uncomment this property if you need to access the list for more functionalities.
        //public List<T> List => _list; 

        public T this[int index] { get => _list[index]; set => _list[index] = value; }

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
            if (_list.Contains(item))
                return;

            _list.Add(item);
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
            var itemList = items.ToList();
            foreach (var item in itemList.Where(item => !_list.Contains(item)))
                _list.Add(item);

            OnItemCountChanged?.Invoke();
            OnItemsAdded?.Invoke(itemList);
        }

        /// <summary>
        /// Removes an item from the list only if its in the list.
        /// Triggers OnItemCountChanged and OnItemRemoved event.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            if (!_list.Contains(item))
                return;

            _list.Remove(item);
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
            var items = _list.GetRange(index, count);
            _list.RemoveRange(index, count);
            OnItemCountChanged?.Invoke();
            OnItemsRemoved?.Invoke(items);
        }

        private void Clear()
        {
            _list.Clear();
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

            if (_resetOn == ResetType.SceneLoaded)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (_resetOn == ResetType.SceneLoaded)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                Clear();
        }

        public override void Reset()
        {
            _resetOn = ResetType.SceneLoaded;
            Clear();
        }

        public void ResetToInitialValue() => Clear();

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<Object> GetAllObjects()
        {
            var list = new List<Object>(Count);
            list.AddRange(_list.OfType<Object>());
            return list;
        }
    }
}