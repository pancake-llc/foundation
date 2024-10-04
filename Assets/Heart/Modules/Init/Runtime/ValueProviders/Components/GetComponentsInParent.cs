using System;
using UnityEngine;
using static Sisus.Init.Internal.TypeUtility;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Returns all objects of the given type attached to the <see cref="GameObject"/> and all of its parents.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAll = Is.SceneObject | Is.Collection, Order = 10, Tooltip = "Collection will be created at runtime from the components attached to this game object and all its parents.")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1008)]
	#endif
	internal sealed class GetComponentsInParent : ScriptableObject, IValueByTypeProvider
	#if UNITY_EDITOR
	, INullGuardByType
	#endif
	{
		private const string MENU_NAME = "Hierarchy/Get Components In Parent";

		/// <summary>
		/// Gets all objects attached to the <paramref name="client"/> and all its parents that match the element type of the <typeparamref name="TValue"/> collection.
		/// </summary>
		/// <typeparam name="TValue"> Type of result array. </typeparam>
		/// <param name="client"> The <see cref="GameObject"/> to search. </param>
		/// <param name="value">
		/// When this method returns, contains an array of type <typeparamref name="TValue"/> if one or more results were found; otherwise, the <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if at least one matching object is found; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>(Component client, out TValue value)
		{
			if(client == null)
			{
				value = default;
				return false;
			}

			var found = Find.AllInParents(client.gameObject, GetCollectionElementType(typeof(TValue)), client.gameObject.activeInHierarchy);
			value = ConvertToCollection<TValue, object>(found);
			return true;
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => client != null && Find.typesToComponentTypes.ContainsKey(GetCollectionElementType(typeof(TValue)));

		#if UNITY_EDITOR
		NullGuardResult INullGuardByType.EvaluateNullGuard<TValue>(Component client)
		{
			if(client == null)
			{
				return NullGuardResult.ClientNotSupported;
			}

			if(GetCollectionElementType(typeof(TValue)) is not Type elementType)
			{
				return NullGuardResult.TypeNotSupported;
			}

			if(!Find.typesToComponentTypes.ContainsKey(elementType))
			{
				return NullGuardResult.TypeNotSupported;
			}

			return NullGuardResult.Passed;
		}
		#endif
	}
}