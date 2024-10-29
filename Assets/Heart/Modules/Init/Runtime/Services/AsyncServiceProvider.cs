using System.Diagnostics.CodeAnalysis;
using Sisus.Init.ValueProviders;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class that can provide an asynchronously initialized local or global service to clients when they become available.
	/// <para>
	/// This is a simple proxy for the static <see cref="Service{TDefiningClassOrInterface}"/> class;
	/// Calling the <see cref="GetForAsync"/> method on any instance of this class will return the shared
	/// service instance stored in <see cref="Service{TDefiningClassOrInterface}.Instance"/>.
	/// </para>
	/// <para>
	/// Additionally, it makes it easier to swap your service provider with another implementation at a later time.
	/// </para>
	/// <para>
	/// A third benefit is that it makes your code less coupled with other classes, making it much easier to
	/// port the code over to another project for example.
	/// </para>
	/// <para>
	/// The <see cref="AsyncServiceProvider"/> can be automatically received by classes that derive from
	/// <see cref="MonoBehaviour{IServiceProvider}"/> or <see cref="ScriptableObject{IServiceProvider}"/>.
	/// </para>
	/// </summary>
	#if !INIT_ARGS_DISABLE_VALUE_PROVIDER_MENU_ITEMS
	[ValueProviderMenu(MENU_NAME, WhereAny = Is.Class | Is.Interface, WhereNone = Is.BuiltIn, Order = 100f, Tooltip = "An instance of this service is expected to become available asynchronously for the client at runtime.\n\nService can be a component that has the Service Tag, or an Object registered as a service in a Services component, that is located in another scene or prefab.\n\nThe service could also be manually registered in code using " + nameof(Service) + "." + nameof(Service.Set) + ".")]
	#endif
	#if !INIT_ARGS_DISABLE_CREATE_ASSET_MENU_ITEMS
	[CreateAssetMenu(fileName = MENU_NAME, menuName = ValueProviderUtility.CREATE_ASSET_MENU_GROUP + MENU_NAME, order = 1010)]
	#endif
	public sealed class AsyncServiceProvider : ScriptableObject, IValueByTypeProviderAsync, INullGuardByType
	{
		private const string MENU_NAME = "Async Service";

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