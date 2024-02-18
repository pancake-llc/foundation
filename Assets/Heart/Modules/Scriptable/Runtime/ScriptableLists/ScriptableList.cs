using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using Pancake.Apex;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    public abstract class ScriptableList<T> : ScriptableListBase, IReset, IList<T>, IDrawObjectsInInspector
    {
        [Message("Clear the list when:" + "\nScene Loaded : when the scene is loaded by LoadSceneMode.Single" +
                 "\nAdditive Scene Loaded : when the scene is loaded by LoadSceneMode.Additive" +
                 "\nApplication Start : Once, when the application starts. Modifications persists between scenes",
            Height = 58)]
        [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

        [SerializeField] protected List<T> list = new List<T>();
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public int Count => list.Count;
        public bool IsEmpty => list.Count == 0;
        public bool IsReadOnly => false;
        public override Type GetGenericType => typeof(T);

        //feel free to uncomment this property if you need to access the list for more functionalities.
        //public List<T> List => list; 

        /// <summary>
        /// Access an item in the list by index.
        /// </summary>
        public T this[int index] { get => list[index]; set => list[index] = value; }

        /// <summary> Event raised when an item is added or removed from the list. </summary>
        public event Action OnItemCountChanged;

        /// <summary> Event raised  when an item is added to the list. </summary>
        public event Action<T> OnItemAdded;

        /// <summary> Event raised  when an item is removed from the list. </summary>
        public event Action<T> OnItemRemoved;

        /// <summary> Event raised  when multiple item are added to the list. </summary>
        public event Action<IEnumerable<T>> OnItemsAdded;

        /// <summary> Event raised  when multiple items are removed from the list. </summary>
        public event Action<IEnumerable<T>> OnItemsRemoved;

        /// <summary> Event raised  when the list is cleared. </summary>
        public event Action OnCleared;

        public int IndexOf(T item) => list.IndexOf(item);
        public bool Contains(T item) => _hashSet.Contains(item);

        /// <summary>
        /// Adds an item to the list.
        /// Raises OnItemCountChanged and OnItemAdded event.
        /// </summary>
        public void Add(T item)
        {
            list.Add(item);
            AddItemToHashAndRaiseEvents(item);
        }

        /// <summary>
        /// Adds an item to the list only if it's not in the list.
        /// If success, raises OnItemCountChanged and OnItemAdded event.
        /// </summary>
        public bool TryAdd(T item)
        {
            if (!_hashSet.Contains(item))
            {
                list.Add(item);
                AddItemToHashAndRaiseEvents(item);
                return true;
            }

            return false;
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            AddItemToHashAndRaiseEvents(item);
        }

        private void AddItemToHashAndRaiseEvents(T item)
        {
            _hashSet.Add(item);
            OnItemCountChanged?.Invoke();
            OnItemAdded?.Invoke(item);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        /// <summary>
        /// Adds a range of items to the list.
        /// Raises OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            var collection = items.ToArray();
            if (collection.Length == 0) return;

            list.AddRange(collection);
            _hashSet.UnionWith(collection);

            OnItemCountChanged?.Invoke();
            OnItemsAdded?.Invoke(collection);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        /// <summary>
        /// Adds a range of items to the list. An item is only added if its not in the list.
        /// Raises OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        public bool TryAddRange(IEnumerable<T> items)
        {
            if (items == null) return false;

            var uniqueItems = items.Where(item => !_hashSet.Contains(item)).ToList();
            if (uniqueItems.Count > 0)
            {
                AddRange(uniqueItems);
                return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) { list.CopyTo(array, arrayIndex); }

        /// <summary>
        /// Removes an item from the list only if it's in the list.
        /// If Success, raises OnItemCountChanged and OnItemRemoved event.
        /// </summary>
        /// <param name="item"></param>
        public bool Remove(T item)
        {
            if (!_hashSet.Contains(item))
                return false;

            bool removedFromList = list.Remove(item);
            if (removedFromList)
            {
                _hashSet.Remove(item);
                OnItemCountChanged?.Invoke();
                OnItemRemoved?.Invoke(item);
#if UNITY_EDITOR
                repaintRequest?.Invoke();
#endif
                return true;
            }

            return false;
        }

        bool ICollection<T>.Remove(T item) { return list.Remove(item); }

        /// <summary>
        /// Removes an item from the list at a specific index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            _hashSet.Remove(item);
            OnItemCountChanged?.Invoke();
            OnItemRemoved?.Invoke(item);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }


        /// <summary>
        /// Removes a range of items from the list.
        /// Raises OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        /// <param name="index">Starting Index</param>
        /// <param name="count">Amount of Items</param>
        public bool RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0) return false;

            if (index + count > list.Count) return false;

            var itemsToRemove = list.GetRange(index, count);

            foreach (var itemToRemove in itemsToRemove) _hashSet.Remove(itemToRemove);

            list.RemoveRange(index, count);
            OnItemCountChanged?.Invoke();
            OnItemsRemoved?.Invoke(itemsToRemove);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
            return true;
        }

        public void Clear()
        {
            _hashSet.Clear();
            list.Clear();
            OnCleared?.Invoke();
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            Clear();

            if (resetOn is ResetType.SceneLoaded or ResetType.AdditiveSceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
            if (resetOn is ResetType.SceneLoaded or ResetType.AdditiveSceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if ((resetOn == ResetType.SceneLoaded && mode == LoadSceneMode.Single) || resetOn == ResetType.AdditiveSceneLoaded && mode == LoadSceneMode.Additive) Clear();
        }

        public override void Reset()
        {
            resetOn = ResetType.SceneLoaded;
            Clear();
        }

#if UNITY_EDITOR
        public void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == UnityEditor.PlayModeStateChange.EnteredEditMode) Clear();
        }
#endif

        public void ResetToInitialValue() => Clear();

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<T> action)
        {
            for (var i = list.Count - 1; i >= 0; i--)
                action(list[i]);
        }

        public List<Object> GetAllObjects()
        {
            var _ = new List<Object>(Count);
            _.AddRange(list.OfType<Object>());
            return _;
        }
    }
}