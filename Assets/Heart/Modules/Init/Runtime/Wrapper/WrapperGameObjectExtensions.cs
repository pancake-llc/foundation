using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to wrap plain old class objects with
	/// <see cref="Wrapper{}"/> components and attach them to GameObjects.
	/// </summary>
	public static class WrapperGameObjectExtensions
	{
		/// <summary>
		/// Wraps the plain old class object of type <typeparamref name="TWrapped"/> in a <see cref="IWrapper{TWrapped}">wrapper component</see>
		/// and adds it to the <paramref name="gameObject"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> The type of the object to wrap and add to the <paramref name="gameObject"/>. </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to which the object is added. </param>
		/// <param name="wrapped"> The object to wrap add to the <paramref name="gameObject"/>. </param>
		/// <returns> The wrapper that was added to the <paramref name="gameObject"/> and was made to wrap the <paramref name="wrapped"/> object. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> or the <paramref name="wrapped"/> is <see langword="null"/>. 
		/// </exception>
		public static Wrapper Add<TWrapped>([DisallowNull] this GameObject gameObject, TWrapped wrapped)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(wrapped is null)
			{
				if(!gameObject)
				{
					throw new ArgumentNullException($"The wrapped object which you want to add to the GameObject is null.");
				}

				throw new ArgumentNullException($"The wrapped object which you want to add to the GameObject \"{gameObject.name}\" is null.");
			}

			if(!gameObject)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the wrapped object {wrapped.GetType().Name} is null.");
			}
			#endif

			var wrappedType = wrapped.GetType();

			if(Find.typesToWrapperTypes.TryGetValue(wrappedType, out var wrapperTypes))
			{
				var wrapperType = wrapperTypes[0];

				if(typeof(Wrapper).IsAssignableFrom(wrapperType))
				{
					InitArgs.Set(wrapperType, wrapped);

					var wrapper = gameObject.AddComponent(wrapperType) as Wrapper;

					if(!InitArgs.Clear<TWrapped>(wrapperType))
					{
						return wrapper;
					}

					if(wrapper is IInitializable<TWrapped> initializable)
					{
						initializable.Init(wrapped);
						return wrapper;
					}

					for(var type = wrappedType; type != null; type = wrappedType.BaseType)
					{
						var initializableType = typeof(IInitializable<>).MakeGenericType(type);
						if(initializableType.IsAssignableFrom(wrapperType))
						{
							initializableType.GetMethod(nameof(IInitializable<object>.Init)).Invoke(wrapper, new object[] { wrapped });
							return wrapper;
						}
					}

					#if UNITY_EDITOR
					if(!Application.isPlaying)
					{
						Object.DestroyImmediate(wrapper);
					}
					else
					#endif
					{
						Object.Destroy(wrapper);
					}
				}
			}

			InitArgs.Set<DefaultWrapper, object>(wrapped);
			var defaultWrapper = gameObject.AddComponent<DefaultWrapper>();

			if(InitArgs.Clear<TWrapped>(typeof(DefaultWrapper)))
			{
				((IInitializable<object>)defaultWrapper).Init(wrapped);
			}

			return defaultWrapper;
		}

		[return: MaybeNull]
		public static TWrapper Add<TWrapper, TWrapped>([DisallowNull] this GameObject gameObject)
			where TWrapper : MonoBehaviour, IWrapper<TWrapped> where TWrapped : new()
		{
			return gameObject.AddComponent<TWrapper, TWrapped>(new TWrapped());
		}
	}
}