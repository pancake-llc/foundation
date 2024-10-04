using UnityEngine;
using static Sisus.Init.Internal.TypeUtility;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Returns all objects of the requested type attached to the client <see cref="GameObject"/>.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAll = Is.Collection | Is.SceneObject, Order = 1.1f, Tooltip = "Collection will be created at runtime from the components attached to this game object.")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1006)]
	#endif
	internal sealed class GetComponents : ScriptableObject, IValueByTypeProvider
	#if UNITY_EDITOR
	, INullGuardByType
	#endif
	{
		private const string MENU_NAME = "Hierarchy/Get Components";

		/// <summary>
		/// Gets all objects attached to the <paramref name="client"/> that match the element type of the <typeparamref name="TValue"/> array.
		/// </summary>
		/// <typeparam name="TValue"> Type of result array. </typeparam>
		/// <param name="client"> The <see cref="GameObject"/> to search. </param>
		/// <param name="value">
		/// When this method returns, contains an array of type <typeparamref name="TValue"/> if one or more results were found; otherwise, the <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if at least one matching object is found on the <see cref="GameObject"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>(Component client, out TValue value)
		{
			if(client == null)
			{
				value = default;
				return false;
			}

			object[] found = Find.AllIn(client.gameObject, GetCollectionElementType(typeof(TValue)));
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

			if(!Find.typesToComponentTypes.ContainsKey(typeof(TValue)))
			{
				return NullGuardResult.TypeNotSupported;
			}

			return NullGuardResult.Passed;
		}
		#endif
	}
}