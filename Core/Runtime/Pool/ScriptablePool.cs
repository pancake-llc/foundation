using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake
{
    /// <summary>
    /// A generic pool that generates members of type T on-demand via a factory.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements to pool.</typeparam>
    [EditorIcon("script_pool")]
    public abstract class ScriptablePool<T> : ScriptableObject, IPool<T>
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;

        [Tooltip("Reset flag IsPrewarmed." + " Scene Loaded : when the scene is loaded." + " Application Start : Once, when the application starts.")] [SerializeField]
        private ResetType resetOn = ResetType.SceneLoaded;

        /// <summary>
        /// The factory which will be used to create <typeparamref name="T"/> on demand.
        /// </summary>
        public abstract IFactory<T> Factory { get; set; }

        protected bool IsPrewarmed { get; set; }
        protected readonly Stack<T> container = new Stack<T>();

        /// <summary>
        /// Prewarms the pool with a <paramref name="size"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="size">The number of members to create as a part of this pool.</param>
        /// <remarks>NOTE: This method can be called at any time, but only once for the lifetime of the pool.</remarks>
        public virtual void Prewarm(int size)
        {
            if (IsPrewarmed)
            {
                Debug.LogWarning($"Pool {name} has already been prewarmed.");
                return;
            }

            for (var i = 0; i < size; i++)
            {
                container.Push(Create());
            }

            IsPrewarmed = true;
        }

        /// <summary>
        /// Requests a <typeparamref name="T"/> from this pool.
        /// </summary>
        /// <returns>The requested <typeparamref name="T"/>.</returns>
        public virtual T Request() { return container.Count > 0 ? container.Pop() : Create(); }

        /// <summary>
        /// Batch requests a <typeparamref name="T"/> collection from this pool.
        /// </summary>
        /// <param name="count"></param>
        /// <returns>A <typeparamref name="T"/> collection.</returns>
        public virtual IEnumerable<T> Request(int count)
        {
            var members = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                members.Add(Request());
            }

            return members;
        }

        /// <summary>
        /// Returns a <typeparamref name="T"/> to the pool.
        /// </summary>
        /// <param name="member">The <typeparamref name="T"/> to return.</param>
        public virtual void Return(T member) { container.Push(member); }

        /// <summary>
        /// Returns a <typeparamref name="T"/> collection to the pool.
        /// </summary>
        /// <param name="members">The <typeparamref name="T"/> collection to return.</param>
        public virtual void Return(IEnumerable<T> members)
        {
            foreach (var member in members)
            {
                Return(member);
            }
        }

        protected virtual T Create() { return Factory.Create(); }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredEditMode:
                    IsPrewarmed = false;
                    break;
            }
        }
#endif

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (resetOn != ResetType.SceneLoaded) return;

            if (mode == LoadSceneMode.Single)
            {
                container.Clear();
                IsPrewarmed = false;
            }
        }

        public virtual void OnDisable()
        {
            container.Clear();
            IsPrewarmed = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }
    }
}