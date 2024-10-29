using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// A list of components acquired using <see cref="GetComponentExtensions.GetComponentsNonAlloc{TComponent}"/>.
	/// <para>
	/// This object should never be cached or reused; it should either be used with a using statement,
	/// so that it gets disposed when leaving the method scope, or iterated once with a foreach statement,
	/// in which case it gets disposed automatically at the end of the iteration.
	/// </para>
	/// </summary>
	/// <typeparam name="TComponent"> The type of elements in the list; either a <see cref="Component"/> or interface type. </typeparam>
	/// <example>
	/// <code>
	/// foreach(var component in gameObject.GetComponentsNonAlloc{Component}())
	/// {
	///		// do something with component
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// using var components = gameObject.GetComponentsNonAlloc{Component}();
	/// // do something with components
	/// </code>
	/// </example>
	internal sealed class ComponentCollection<TComponent> : List<TComponent>, IDisposable
	{
		private static readonly List<ComponentCollection<TComponent>> cache = new(1);

		private ComponentCollection() { }
		
		internal static ComponentCollection<TComponent> Empty()
		{
			ComponentCollection<TComponent> collection;

			int count = cache.Count;
			if(count > 0)
			{
				int lastIndex = count - 1;
				collection = cache[lastIndex];
				cache.RemoveAt(lastIndex);
			}
			else
			{
				collection = new();
			}

			return collection;
		}

		internal static ComponentCollection<TComponent> GetFrom(GameObject gameObject)
		{
			ComponentCollection<TComponent> collection;

			int count = cache.Count;
			if(count > 0)
			{
				int lastIndex = count - 1;
				collection = cache[lastIndex];
				cache.RemoveAt(lastIndex);
			}
			else
			{
				collection = new();
			}

			gameObject.GetComponents(collection);

			return collection;
		}

		internal static ComponentCollection<Component> GetFrom(GameObject gameObject, Type type)
		{
			ComponentCollection<Component> collection;

			int count = cache.Count;
			if(count > 0)
			{
				int lastIndex = count - 1;
				collection = ComponentCollection<Component>.cache[lastIndex];
				cache.RemoveAt(lastIndex);
			}
			else
			{
				collection = new();
			}

			gameObject.GetComponents(type, collection);

			return collection;
		}

		public void Dispose()
		{
			if(!cache.Contains(this))
			{
				Clear();
				cache.Add(this);
			}
		}

		public new Enumerator GetEnumerator() => Enumerator.Get(this);

		public new sealed class Enumerator : IDisposable
		{
			private static readonly List<Enumerator> cache = new(1);

			private ComponentCollection<TComponent> collection;
			private int currentIndex;

			internal static Enumerator Get(ComponentCollection<TComponent> collection)
			{
				int count = cache.Count;
				if(count == 0)
				{
					return new(collection);
				}

				int lastIndex = count - 1;
				var enumerator = cache[lastIndex];
				cache.RemoveAt(lastIndex);
				enumerator.Setup(collection);
				return enumerator;
			}

			private Enumerator(ComponentCollection<TComponent> collection) => Setup(collection);

			private void Setup(ComponentCollection<TComponent> collection)
			{
				#if DEBUG || DEV_MODE || INIT_ARGS_SAFE_MODE
				if(collection is null)
				{
					Debug.LogError($"Attempted to iterate over a null {nameof(ComponentCollection<object>)}<{typeof(TComponent).Name}>.");
					collection = new();
				}
				#endif

				this.collection = collection;
				currentIndex = -1;
			}

			public bool MoveNext()
			{
				#if DEBUG || DEV_MODE || INIT_ARGS_SAFE_MODE
				if(collection is null)
				{
					Debug.LogError($"Attempted to execute {nameof(MoveNext)} on {nameof(ComponentCollection<object>)}<{typeof(TComponent).Name}>.{nameof(Enumerator)} after already reaching end of the list.");
					return default;
				}
				#endif

				currentIndex++;
				return currentIndex < collection.Count;
			}

			public TComponent Current
			{
				get
				{
					#if DEBUG || DEV_MODE || INIT_ARGS_SAFE_MODE
					if(collection is null || currentIndex < 0 || currentIndex >= collection.Count)
					{
						Debug.LogError($"Attempted to access {nameof(Current)} property of {nameof(ComponentCollection<object>)}<{typeof(TComponent).Name}>.{nameof(Enumerator)} after already reaching end of the list.", collection is not null && collection.Count > 0 ? collection[0] as Object : null);
						return default;
					}
					#endif

					return collection[currentIndex];
				}
			}

			public void Dispose()
			{
				if(!cache.Contains(this))
				{
					collection.Dispose();
					collection = null;
					cache.Add(this);
				}
			}
		}
	}
}