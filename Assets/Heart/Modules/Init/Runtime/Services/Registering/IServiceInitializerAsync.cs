using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an initializer that specifies how a service object should be initialized asynchronously.
	/// <para>
	/// Base interface for all generic <see cref="IServiceInitializerAsync{}"/> interfaces,
	/// which should be implemented by all asynchron ous service initializer classes.
	/// </para>
	/// </summary>
	[RequireImplementors]
	public interface IServiceInitializerAsync
	{
		[return: NotNull] public Task InitTargetAsync(params object[] arguments);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized asynchronously.
	/// <para>
	/// Implemented by initializers of services that are initialized automatically by the framework
	/// or services that don't depend on any other services and are initialized manually via the
	/// <see cref="InitTargetAsync"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService> : IServiceInitializerAsync
	{
		/// <summary>
		/// Returns an <see cref="Awaitable{TService}"/> that can be awaited to get a new instance of the <see cref="TService"/> class asynchronously.
		/// </summary>
		[return: NotNull] Task<TService> InitTargetAsync();
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized asynchronously.
	/// <para>
	/// Implemented by initializers of services that depend on one other service and
	/// are initialized manually via the <see cref="InitTargetAsync"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TArgument"> Type of another service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with another service that it depends on.
		/// </summary>
		/// <param name="argument"> Service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TArgument argument);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on two other services and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with two other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on three other services and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with three other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on four other services and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with four other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on five other services and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with five other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on six other services and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth service which the service of type <typeparamref name="TService"/> depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously with six other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously using seven other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously using eight other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously using nine other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		/// <param name="ninthArgument"> Ninth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously using ten other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		/// <param name="ninthArgument"> Ninth service used during initialization of the target service. </param>
		/// <param name="tenthArgument"> Tenth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously asynchronously using eleven other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		/// <param name="ninthArgument"> Ninth service used during initialization of the target service. </param>
		/// <param name="tenthArgument"> Tenth service used during initialization of the target service. </param>
		/// <param name="eleventhArgument"> Eleventh service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializerAsync<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IServiceInitializerAsync
	{
		/// <summary>
		/// Initializes the service asynchronously using twelve other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		/// <param name="ninthArgument"> Ninth service used during initialization of the target service. </param>
		/// <param name="tenthArgument"> Tenth service used during initialization of the target service. </param>
		/// <param name="eleventhArgument"> Eleventh service used during initialization of the target service. </param>
		/// <param name="twelfthArgument"> Twelfth service used during initialization of the target service. </param>
		[return: NotNull] Task<TService> InitTargetAsync(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument);
	}
}