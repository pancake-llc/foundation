#pragma warning disable CS0414

using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that is responsible for initializing a service of type
	/// <typeparamref name="TService"/>, which itself depends on another service of type
	/// <typeparamref name="TArgument"/>.
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
	/// <typeparam name="TService"> The concrete type of the service object class. </typeparam>
	/// <typeparam name="TArgument"> The defining type of the other service which the initialized service depends on. </typeparam>
	/// <seealso cref="ServiceInitializer{TService}"/>
	public abstract class ServiceInitializer<TService, TArgument> : IServiceInitializer<TService, TArgument> where TService : class
	{
		/// <inheritdoc/>
		object IServiceInitializer.InitTarget(params object[] services)
		{
			Task<TService> task = InitTargetAsync((TArgument)services[0]);
			return task.IsCompleted ? task.Result : task;
		}

		/// <inheritdoc/>
		public abstract TService InitTarget(TArgument argument);

		/// <inheritdoc/>
		public virtual Task<TService> InitTargetAsync(TArgument argument) => Task.FromResult(InitTarget(argument));
	}
}