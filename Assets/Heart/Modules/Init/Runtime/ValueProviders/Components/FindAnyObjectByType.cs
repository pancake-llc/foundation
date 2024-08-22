using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.ValueProviders.ValueProviderUtility;

namespace Sisus.Init.ValueProviders
{
	/// <summary>
	/// Returns an object of the given type that is currently loaded.
	/// <para>
	/// Can be used to retrieve an Init argument at runtime.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, Is.SceneObject, Order = 2.1f, Tooltip = "Value will be located at runtime from any of the active scenes.")]
	#endif
	#if DEV_MODE
	[CreateAssetMenu(fileName = MENU_NAME, menuName = CREATE_ASSET_MENU_GROUP + MENU_NAME)]
	#endif
	internal sealed class FindAnyObjectByType : ScriptableObject, IValueByTypeProvider
	{
		private const string MENU_NAME = "Hierarchy/Find Any Object By Type";

		/// <summary>
		/// Gets an object of type <typeparamref name="TValue"/> that is currently loaded.
		/// </summary>
		/// <typeparam name="TValue"> Type of object to find. </typeparam>
		/// <param name="client"> This parameter is ignored. </param>
		/// <param name="value">
		/// When this method returns, contains an object <typeparamref name="TValue"/> if one was found; otherwise, the <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if an object was found; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TryGetFor<TValue>([AllowNull] Component client, out TValue value) => Find.Any(out value, false) || Find.Any(out value, true);

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => Find.typesToComponentTypes.ContainsKey(typeof(TValue));
	}
}