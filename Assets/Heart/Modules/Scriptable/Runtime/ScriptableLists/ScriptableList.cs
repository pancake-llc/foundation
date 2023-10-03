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
        private ResetType resetOn = ResetType.SceneLoaded;

        [SerializeField] protected List<T> list = new List<T>();

        public int Count => list.Count;
        public bool IsEmpty => list.Count == 0;
        public override Type GetGenericType => typeof(T);

        //feel free to uncomment this property if you need to access the list for more functionalities.
        //public List<T> List => list; 

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

        /// <summary>
        /// Adds an item to the list only if its not in the list.
        /// Raises OnItemCountChanged and OnItemAdded event.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (list.Contains(item)) return;

            list.Add(item);
            OnItemCountChanged?.Invoke();
            OnItemAdded?.Invoke(item);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        /// <summary>
        /// Adds a range of items to the list. An item is only added if its not in the list.
        /// Triggers OnItemCountChanged and OnItemsAdded event once, after all items have been added.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            var itemList = items.ToList();
            foreach (var item in itemList.Where(item => !list.Contains(item)))
                list.Add(item);

            OnItemCountChanged?.Invoke();
            OnItemsAdded?.Invoke(itemList);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        /// <summary>
        /// Removes an item from the list only if its in the list.
        /// Raises OnItemCountChanged and OnItemRemoved event.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            if (!list.Contains(item)) return;

            list.Remove(item);
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
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            var items = list.GetRange(index, count);
            list.RemoveRange(index, count);
            OnItemCountChanged?.Invoke();
            OnItemsRemoved?.Invoke(items);
#if UNITY_EDITOR
            repaintRequest?.Invoke();
#endif
        }

        private void Clear()
        {
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

            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
            if (resetOn == ResetType.SceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
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

#if UNITY_EDITOR
        public void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == UnityEditor.PlayModeStateChange.EnteredEditMode) Clear();
        }
#endif

        public void ResetToInitialValue() => Clear();

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<Object> GetAllObjects()
        {
            var _ = new List<Object>(Count);
            _.AddRange(list.OfType<Object>());
            return _;
        }
    }
}