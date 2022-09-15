using System;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Pancake
{
	/// <summary>
	/// Represents an object that can can specify the arguments used to
	/// <see cref="IInitializable{}.Init">initialize</see> an object
	/// that implements one of the <see cref="IInitializable{}"/> interfaces.
	/// <para>
	/// Base interface for all generic <see cref="IInitializer{,}"/> interfaces,
	/// which should be implemented by all Initializer classes.
	/// </para>
	/// </summary>
	public interface IInitializer
	{
		/// <summary>
		/// Existing target instance to initialize, if any.
		/// <para>
		/// If value is <see langword="null"/> then the argument is injected to a new instance.
		/// </para>
		/// <para>
		/// If value is a component on another GameObject then the argument is injected to a new instance
		/// created by cloning the GameObject with the component.
		/// </para>
		/// </summary>
		[CanBeNull]
		Object Target { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not <see cref="Target"/> can be assigned to a field of type <paramref name="type"/>
		/// or can be converted to it via the <see cref="IValueProvider{T}"/> interface.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool TargetIsAssignableOrConvertibleToType(Type type);

		/// <summary>
		/// Initializes the target with the init arguments specified on this initializer.
		/// </summary>
		/// <returns> The existing <see cref="Target"/> or a new instance of type <see cref="TargetType"/>. </returns>
		[NotNull]
		object InitTarget();
	}
	
	/// <summary>
	/// Represents an Initializer that can can specify the arguments used to initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	public interface IInitializer<TClient> : IInitializer
	{
		/// <summary>
		/// Initializes the target with the init arguments specified on this initializer.
		/// </summary>
		/// <returns> The existing <see cref="IInitializer.Target"/> or new instance of type <see cref="TClient"/>. </returns>
		[NotNull]
		new TClient InitTarget();
	}

	/// <summary>
	/// Represents an Initializer that can can specify a single argument that is used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TArgument"> Type of the Init argument. </typeparam>
	public interface IInitializer<TClient, TArgument> : IInitializer<TClient> { }

	/// <summary>
	/// Represents an Initializer that can can specify two arguments that are used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first Init argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second Init argument. </typeparam>
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument> : IInitializer<TClient> { }

	/// <summary>
	/// Represents an Initializer that can can specify three arguments that are used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first Init argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second Init argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third Init argument. </typeparam>
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument> : IInitializer<TClient> { }
	
	/// <summary>
	/// Represents an Initializer that can can specify four arguments that are used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first Init argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second Init argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third Init argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth Init argument. </typeparam>
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IInitializer<TClient> { }

	/// <summary>
	/// Represents an Initializer that can can specify five arguments that are used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first Init argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second Init argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third Init argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth Init argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth Init argument. </typeparam>
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IInitializer<TClient> { }

	/// <summary>
	/// Represents an Initializer that can can specify six arguments that are used to
	/// initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first Init argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second Init argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third Init argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth Init argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth Init argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth Init argument. </typeparam>
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IInitializer<TClient> { }
}