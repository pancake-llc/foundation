using System.Diagnostics.CodeAnalysis;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an initializer that specifies how a service object should be initialized.
	/// <para>
	/// Base interface for all generic <see cref="IServiceInitializer{}"/> interfaces,
	/// which should be implemented by all service initializer classes.
	/// </para>
	/// </summary>
	[RequireImplementors]
	public interface IServiceInitializer
	{
		/// <summary>
		/// Returns a new instance of the service class,
		/// a <see cref="UnityEngine.Awaitable{}"/> that returns a new instance of the service class asynchronously,
		/// or <see langword="null"/>.
		/// <para>
		/// If this method returns <see langword="null"/>, the framework will handle creating the instance internally.
		/// </para>
		/// <para>
		/// If this method returns an <see cref="UnityEngine.Awaitable{}"/>, the framework will await for the result
		/// and register it as a service once it's ready.
		/// </para>
		/// <para>
		/// If this method returns an object of another type, that will be registered as a service immediately.
		/// </para>
		/// </summary>
		/// <param name="arguments"> Zero or more other services used during initialization of the target service. </param>
		/// <returns> An instance of the service class, an <see cref="UnityEngine.Awaitable{}"/>, or <see langword="null"/>. </returns>
		[return: MaybeNull]
		object InitTarget(params object[] arguments);
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that are initialized automatically by the framework
	/// or services that don't depend on any other services and are initialized manually via the
	/// <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService> : IServiceInitializer
	{
		/// <summary>
		/// Returns a new instance of the <see cref="TService"/> class, or <see langword="null"/>.
		/// <para>
		/// If this method returns <see langword="null"/>, it tells the framework that it should
		/// handle creating the instance internally.
		/// </para>
		/// <para>
		/// If more control is needed, this method can be implemented to  take over control of the
		/// creation of the service object from the framework.
		/// </para>
		/// </summary>
		/// <returns>
		/// A new instance of the <see cref="TService"/> class, or <see langword="null"/>
		/// if the framework should create the instance automatically.
		/// </returns>
		[return: MaybeNull]
		TService InitTarget();
	}

	/// <summary>
	/// Represents an initializer that specifies how a service of type
	/// <typeparamref name="TService"/> should be initialized.
	/// <para>
	/// Implemented by initializers of services that depend on one other service and
	/// are initialized manually via the <see cref="InitTarget"/> method.
	/// </para>
	/// </summary>
	/// <typeparam name="TService"> The concrete type of the initialized service. </typeparam>
	/// <typeparam name="TArgument"> Type of another service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with another service that it depends on.
		/// </summary>
		/// <param name="argument"> Service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TArgument argument);
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
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with two other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument);
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
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with three other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);
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
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with four other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument);
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
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with five other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);
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
	/// <typeparam name="TFirstArgument"> Type of the first service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second service which the initialized service depends on. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth service which the initialized service depends on. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth service which the initialized service depends on. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth service which the initialized service depends on. </typeparam>
	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service with six other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using seven other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using eight other services that it depends on.
		/// </summary>
		/// <param name="firstArgument"> First service used during initialization of the target service. </param>
		/// <param name="secondArgument"> Second service used during initialization of the target service. </param>
		/// <param name="thirdArgument"> Third service used during initialization of the target service. </param>
		/// <param name="fourthArgument"> Fourth service used during initialization of the target service. </param>
		/// <param name="fifthArgument"> Fifth service used during initialization of the target service. </param>
		/// <param name="sixthArgument"> Sixth service used during initialization of the target service. </param>
		/// <param name="seventhArgument"> Seventh service used during initialization of the target service. </param>
		/// <param name="eighthArgument"> Eighth service used during initialization of the target service. </param>
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using nine other services that it depends on.
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
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using ten other services that it depends on.
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
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument);
	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using eleven other services that it depends on.
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
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument);	}

	[RequireImplementors]
	public interface IServiceInitializer<TService, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IServiceInitializer
	{
		/// <summary>
		/// Initializes the service using twelve other services that it depends on.
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
		[return: NotNull]
		TService InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument);
	}
}