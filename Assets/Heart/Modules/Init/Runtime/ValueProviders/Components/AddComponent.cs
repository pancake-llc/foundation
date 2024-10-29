using System;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Attaches a new instance of the given type to the client <see cref="GameObject"/> returns it.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAny = Is.Component | Is.WrappedObject, WhereAll = Is.Concrete, Not = typeof(GameObject), Order = 1.5f)]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1001)]
	#endif
	internal sealed class AddComponent : ScriptableObject, IValueByTypeProvider
	#if UNITY_EDITOR
	, INullGuardByType
	#endif
	{
		private const string MENU_NAME = "Hierarchy/Add Component";

		/// <summary>
		/// Attaches a new instance of the type <typeparamref name="TValue"/> to the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TValue"> Type of object to find. </typeparam>
		/// <param name="client"> The <see cref="GameObject"/> to search. </param>
		/// <param name="value">
		/// When this method returns, contains an object <typeparamref name="TValue"/> if one was sucessfully
		/// attached to the <paramref name="client"/>; otherwise, the <see langword="null"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the object was successfully attached to the <paramref name="client"/>;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>(Component client, out TValue value) => TryAdd(client, out value);

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => CanProvideValue<TValue>(client);

		internal static bool CanProvideValue<TValue>(Component client)
		{
			if(client == null)
			{
				return false;
			}

			if(typeof(TValue).IsAssignableFrom(typeof(Component)) && !typeof(TValue).IsAbstract)
			{
				return true;
			}

			if(!Find.typesToComponentTypes.TryGetValue(typeof(TValue), out Type[] componentTypes))
			{
				return false;
			}

			if(componentTypes.Length == 1)
			{
				return true;
			}

			Type typeToAdd = null;
			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				var componentType = componentTypes[i];
				if(componentType.IsAbstract)
				{
					continue;
				}

				if(typeToAdd is null)
				{
					typeToAdd = componentType;
					continue;
				}

				const int RIGHT = 1;
				const int NEITHER = 0;
				int closestMatch = NamespaceMatchComparer(client.GetType().Namespace, typeToAdd.Namespace, componentType.Namespace);
				if(closestMatch == NEITHER)
				{
					return false;
				}

				if(closestMatch == RIGHT)
				{
					typeToAdd = componentType;
				}
			}

			return typeToAdd != null;
		}

		internal static bool TryAdd<TValue>(Component client, out TValue value)
		{
			if(client == null)
			{
				value = default;
				return false;
			}

			if(typeof(Component).IsAssignableFrom(typeof(TValue)) && !typeof(TValue).IsAbstract)
			{
				if(client.gameObject.AddComponent(typeof(TValue)) is TValue addedComponent)
				{
					value = addedComponent;
					return true;
				}

				value = default;
				return false;
			}

			if(!Find.typesToComponentTypes.TryGetValue(typeof(TValue), out Type[] componentTypes))
			{
				value = default;
				return false;
			}

			if(componentTypes.Length == 1)
			{
				if(client.gameObject.AddComponent(componentTypes[0]) is TValue addedComponent)
				{
					value = addedComponent;
					return true;
				}

				return Find.In(client.gameObject, out value);
			}

			Type typeToAdd = null;
			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				var componentType = componentTypes[i];
				if(componentType.IsAbstract)
				{
					continue;
				}

				if(typeToAdd is null)
				{
					typeToAdd = componentType;
					continue;
				}

				const int RIGHT = 1;
				const int NEITHER = 0;
				int closestMatch = NamespaceMatchComparer(client.GetType().Namespace, typeToAdd.Namespace, componentType.Namespace);
				if(closestMatch == NEITHER)
				{
					if(typeof(TValue).IsInterface)
					{
						Debug.LogWarning($"Ambiguous Match Exception: unable to determine type of the component to attach to '{client.name}'; both {typeToAdd.Name} and {componentType.Name} implement the interface {typeof(TValue)}.", client);
					}
					else if(Find.typesToWrapperTypes.ContainsKey(typeof(TValue)))
					{
						Debug.LogWarning($"Ambiguous Match Exception: unable to determine type of the component to attach to '{client.name}'; both {typeToAdd.Name} and {componentType.Name} can wrap an object of type {typeof(TValue)}.", client);
					}
					else
					{
						Debug.LogWarning($"Ambiguous Match Exception: unable to determine type of the component to attach to '{client.name}'; both {typeToAdd.Name} and {componentType.Name} derive from the base class {typeof(TValue)}.", client);
					}

					value = default;
					return false;
				}

				if(closestMatch == RIGHT)
				{
					typeToAdd = componentType;
				}
			}

			if(typeToAdd != null)
			{
				if(client.gameObject.AddComponent(typeToAdd) is TValue addedComponent)
				{
					value = addedComponent;
					return true;
				}

				return Find.In(client.gameObject, out value);
			}

			value = default;
			return false;
		}

		internal static int NamespaceMatchComparer(string target, string a, string b)
		{
			if(string.IsNullOrEmpty(target))
			{
				return 0;
			}

			if(string.IsNullOrEmpty(a))
			{
				return string.IsNullOrEmpty(b) ? 0 : 1;
			}

			if(string.IsNullOrEmpty(b))
			{
				return -1;
			}

			int count = target.Length;
			if(a.Length < count) count = a.Length;
			if(b.Length < count) count = b.Length;

			for(int i = 0; i < count; i++)
			{
				if(target[i] == a[i])
				{
					if(target[i] != b[i])
					{
						return -1;
					}
				}
				else if(target[i] == b[i])
				{
					return 1;
				}
			}

			return 0;
		}

		#if UNITY_EDITOR
		NullGuardResult INullGuardByType.EvaluateNullGuard<TValue>(Component client)
		{
			if(client == null)
			{
				return NullGuardResult.ClientNotSupported;
			}

			if(typeof(TValue).IsAssignableFrom(typeof(Component)) && !typeof(TValue).IsAbstract && !TypeUtility.IsBaseType(typeof(TValue)))
			{
				return NullGuardResult.Passed;
			}

			if(!Find.typesToComponentTypes.TryGetValue(typeof(TValue), out Type[] componentTypes))
			{
				return NullGuardResult.TypeNotSupported;
			}

			if(componentTypes.Length == 1)
			{
				return NullGuardResult.Passed;
			}

			Type typeToAdd = null;
			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				var componentType = componentTypes[i];
				if(componentType.IsAbstract)
				{
					continue;
				}

				if(typeToAdd is null)
				{
					typeToAdd = componentType;
					continue;
				}

				const int RIGHT = 1;
				const int NEITHER = 0;
				int closestMatch = NamespaceMatchComparer(client.GetType().Namespace, typeToAdd.Namespace, componentType.Namespace);
				if(closestMatch == NEITHER)
				{
					return NullGuardResult.TypeNotSupported;
				}

				if(closestMatch == RIGHT)
				{
					typeToAdd = componentType;
				}
			}

			return typeToAdd is not null ? NullGuardResult.Passed : NullGuardResult.TypeNotSupported;
		}
		#endif
	}
}