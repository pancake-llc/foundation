using System.Threading.Tasks;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that is responsible for asynchronously initializing a service of type
	/// <typeparamref name="TService"/>, which itself depends on three other services.
	/// <para>
	/// The <see cref="ServiceAttribute"/> must be added to all classes that derive from this
	/// base class; otherwise the framework will not discover the initializer and the
	/// service will not get registered.
	/// </para>
	/// <para>
	/// The <see cref="ServiceAttribute"/> can also be used to specify additional details
	/// about the service, such as its <see cref="ServiceAttribute.definingType">defining type</see>.
	/// </para>
	/// <para>
	/// Adding the <see cref="ServiceAttribute"/> to a service initializer instead of the service
	/// class itself makes it possible to decouple the service class from the ServiceAttribute.
	/// If you want to keep your service classes as decoupled from Init(args) as possible,
	/// this is one tool at your disposable that can help with that.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the initialized service depends on. </typeparam>
	public abstract class ServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument> : IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument> where TService : class
	{
		/// <inheritdoc/>
		async Task<object> IServiceInitializerAsync.InitTargetAsync(params object[] arguments)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(arguments is null) Debug.LogWarning($"{GetType().Name}.{nameof(InitTargetAsync)} was given no services, when three were expected.");
			else if(arguments.Length != 3) Debug.LogWarning($"{GetType().Name}.{nameof(InitTargetAsync)} was given {arguments.Length} services, when three were expected.");
			#endif

			return await InitTargetAsync((TFirstArgument)arguments[0], (TSecondArgument)arguments[1], (TThirdArgument)arguments[2]);
		}

		/// <inheritdoc/>
		public abstract Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);
	}
}