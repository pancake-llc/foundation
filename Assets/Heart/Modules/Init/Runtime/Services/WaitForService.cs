using System.Diagnostics.CodeAnalysis;
using Sisus.Init.ValueProviders;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Initializer argument menu item that can be used to defer initialization of its client
	/// until a service that it requires becomes available.
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAny = Is.Class | Is.Interface, WhereNone = Is.BuiltIn, Order = 100f, Tooltip = "This service is expected to become available for the client at runtime.\n\nService can be a component that has the Service Tag, or an Object registered as a service in a Services component, that is located in another scene or prefab. The service can also be manually registered at runtime using " + nameof(Service) + "." + nameof(Service.Set) + ".\n\nInitialization will be delayed until the service has become available.")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = ValueProviderUtility.CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1010)]
	#endif
	public sealed class WaitForService : ScriptableObject, IValueByTypeProviderAsync, INullGuardByType
	{
		private const string MENU_NAME = "Wait For Service";

		/// <summary>
		/// Defers execution of the calling method until the service of type <typeparamref name="TValue"/> is available, and then returns it.
		/// </summary>
		/// <typeparam name="TValue"> Type of the service to wait for. </typeparam>
		/// <param name="client">
		/// The component requesting the service, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see cref="Awaitable{TValue}"/> that can be <see langword="await">awaited</see> to defer execution until the service is available.
		/// </returns>
		public
		#if UNITY_2023_1_OR_NEWER
		Awaitable
		#else
		System.Threading.Tasks.Task
		#endif
		<TValue> GetForAsync<TValue>(Component client) => client ? Service.GetForAsync<TValue>(client) : Service.GetAsync<TValue>();

		/// <inheritdoc/>
		public bool CanProvideValue<TService>([AllowNull] Component client) => !typeof(TService).IsValueType && typeof(TService) != typeof(string) && typeof(TService) != typeof(object);

		/// <returns>
		/// Returns always <see langword="true"/> as long as <typeparamref name="TService"/> is not a value type.
		/// <para>
		/// We will always assume that the service will be available at runtime to avoid warnings being shown
		/// to the user about missing arguments.
		/// </para>
		/// </returns>
		NullGuardResult INullGuardByType.EvaluateNullGuard<TService>(Component client) => NullGuardResult.Passed;
	}
}