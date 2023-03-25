using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;

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

        public virtual void OnDisable()
        {
            container.Clear();
            IsPrewarmed = false;
        }
    }
}