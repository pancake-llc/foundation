#pragma warning disable CS0414

using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that is responsible for specifying how a service of type
	/// <typeparamref name="TService"/> should be initialized and registered with the framework.
	/// <para>
	/// Optionally, the <see cref="InitTarget"/> function can be overriden to take
	/// direct control of the initialization of the service.
	/// </para>
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
	/// <typeparam name="TService"> The concrete type of the service object class. </typeparam>
	/// <seealso cref="ServiceInitializer{TService, TArgument}"/>
	public abstract class ServiceInitializer<TService> : IServiceInitializer<TService> where TService : class
	{
		/// <inheritdoc/>
		object IServiceInitializer.InitTarget(params object[] services)
		{
			Task<TService> task = InitTargetAsync();
			return task.IsCompleted ? task.Result : task;
		}

		/// <summary>
		/// Returns a new instance of the <see cref="TService"/> class, or <see langword="null"/>.
		/// <para>
		/// By default this method always returns <see langword="null"/>.
		/// This tells the framework that it should handle creating the instance internally.
		/// </para>
		/// <para>
		/// If more control is needed, this method can be override to take over control of the
		/// creation of the service object from the framework.
		/// </para>
		/// </summary>
		/// <returns>
		/// A new instance of the <see cref="TService"/> class, or <see langword="null"/>
		/// if the framework should create the instance automatically.
		/// </returns>
		[return: MaybeNull]
		public virtual TService InitTarget() => null;

		/// <summary>
		/// Returns a task that can be awaited to get a new instance
		/// of the <see cref="TService"/> class, or <see langword="null"/>.
		/// <para>
		/// By default this method returns the result of <see cref="InitTarget"/> synchronously.
		/// </para>
		/// <para>
		/// This method can be overridden to implement logic for loading the service asynchronously.
		/// </para>
		/// </summary>
		/// <returns>
		/// A task containing an instance of the <see cref="TService"/> class, or <see langword="null"/>
		/// if the framework should create the instance automatically.
		/// </returns>
		[return: MaybeNull]
		public virtual Task<TService> InitTargetAsync() => Task.FromResult(InitTarget());
	}
}