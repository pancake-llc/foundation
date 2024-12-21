using System.Threading.Tasks;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that is responsible for asynchronously initializing a service
	/// of type <typeparamref name="TService"/>, which itself depends on one other service.
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
	/// class itself makes it possible to decouple the service class from the <see cref="ServiceAttribute"/>.
	/// If you want to keep your service classes as decoupled from Init(args) as possible,
	/// this is one tool at your disposable that can help with that.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <seealso cref="ServiceInitializer{TService}"/>
	public abstract class ServiceInitializerAsync<TService> : IServiceInitializerAsync<TService> where TService : class
	{
		/// <inheritdoc/>
		async Task<object> IServiceInitializerAsync.InitTargetAsync(params object[] arguments)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(arguments is null) Debug.LogWarning($"{GetType().Name}.{nameof(InitTargetAsync)} was given no services, when none were expected.");
			else if(arguments.Length != 0) Debug.LogWarning($"{GetType().Name}.{nameof(InitTargetAsync)} was given {arguments.Length} services, when none were expected.");
			#endif

			return await InitTargetAsync();
		}

		/// <summary>
		/// Returns an <see cref="Awaitable{TService}"/> that can be awaited to get a new instance
		/// of the <see cref="TService"/> class, or <see langword="null"/>.
		/// <para>
		/// If the task has a result of <see langword="null"/>, it tells the framework that it should
		/// handle creating the instance internally.
		/// </para>
		/// <para>
		/// If more control is needed, this method can be implemented to take over control of the
		/// creation of the service object from the framework.
		/// </para>
		/// </summary>
		/// <returns>
		/// A task containing an instance of the <see cref="TService"/> class, or <see langword="null"/>
		/// if the framework should create the instance automatically.
		/// </returns>
		public abstract Task<TService> InitTargetAsync();
	}
}