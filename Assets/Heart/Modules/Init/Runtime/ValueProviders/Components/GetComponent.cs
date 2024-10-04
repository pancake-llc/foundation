using UnityEngine;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Returns an object of the requested type attached to the client <see cref="GameObject"/>.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, Is.SceneObject, Order = 1.1f, Tooltip = "Value will be located at runtime from this game object.")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1003)]
	#endif
	internal sealed class GetComponent : ScriptableObject, IValueByTypeProvider
	#if UNITY_EDITOR
	, INullGuardByType
	#endif
	{
		private const string MENU_NAME = "Hierarchy/Get Component";

		/// <summary>
		/// Gets an object of type <typeparamref name="TValue"/> attached to the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TValue"> Type of object to find. </typeparam>
		/// <param name="client"> The <see cref="GameObject"/> to search. </param>
		/// <param name="value">
		/// When this method returns, contains an object <typeparamref name="TValue"/> if one was found; otherwise, the <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object was found; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>(Component client, out TValue value)
		{
			if(client == null)
			{
				value = default;
				return false;
			}
			
			return Find.In(client.gameObject, out value);
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => client != null && Find.typesToComponentTypes.ContainsKey(typeof(TValue));

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

			return Find.In<TValue>(client.gameObject, out _) ? NullGuardResult.Passed : NullGuardResult.ValueProviderValueNullInEditMode;
		}
		#endif
	}
}