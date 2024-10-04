using System;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.ValueProviders.AddComponent;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Returns an object of the given type attached to the client <see cref="GameObject"/>,
	/// or if one is not found, then attaches a new instance of the type to the client and returns that.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAny = Is.Component | Is.WrappedObject, WhereAll = Is.Concrete, Order = 1.6f, Tooltip = "Value will be located at runtime from this game object.\n\nIf no result is found, a new instance will be attached to the game object automatically.")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1009)]
	#endif
	internal sealed class GetOrAddComponent : ScriptableObject, IValueByTypeProvider
	#if UNITY_EDITOR
	, INullGuardByType
	#endif
	{
		private const string MENU_NAME = "Hierarchy/Get Or Add Component";

		/// <summary>
		/// Gets an existing object of type <typeparamref name="TValue"/> attached to the <paramref name="client"/>,
		/// or if one is not found, then attaches a new instance of <typeparamref name="TValue"/> to the
		/// <paramref name="client"/> and returns that.
		/// </summary>
		/// <typeparam name="TValue"> Type of object to find. </typeparam>
		/// <param name="client"> The <see cref="GameObject"/> to search. </param>
		/// <param name="value">
		/// When this method returns, contains an object <typeparamref name="TValue"/> if one was found; otherwise, the <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object of type <typeparamref name="TValue"/> was found on the <paramref name="client"/>
		/// or if a new instance of it was successfully attached to the <paramref name="client"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>(Component client, out TValue value) => Find.In(client, out value) || AddComponent.TryAdd(client, out value);

		public bool CanProvideValue<TValue>(Component client) => AddComponent.CanProvideValue<TValue>(client);

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

			if(Find.In<TValue>(client, out _))
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