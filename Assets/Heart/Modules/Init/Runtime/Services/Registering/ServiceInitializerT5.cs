#pragma warning disable CS0414

using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that is responsible for initializing a service of type
	/// <typeparamref name="TService"/>, which itself depends on two other services.
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
	/// <typeparam name="TFirstArgument"> The defining type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> The defining type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> The defining type of the third service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> The defining type of the fourth service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFifthArgument"> The defining type of the fifth service which the initialized service depends on. </typeparam>
	public abstract class ServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> where TService : class
	{
		/// <inheritdoc/>
		object IServiceInitializer.InitTarget(params object[] services)
		{
			Task<TService> task = InitTargetAsync((TFirstArgument)services[0], (TSecondArgument)services[1], (TThirdArgument)services[2], (TFourthArgument)services[3], (TFifthArgument)services[4]);
			return task.IsCompleted ? task.Result : task;
		}

		/// <inheritdoc/>
		public abstract TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);
		
		/// <inheritdoc/>
		public Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) => Task.FromResult(InitTarget(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument));
	}
}