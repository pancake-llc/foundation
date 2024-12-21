using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init
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
		/// If value is <see langword="null"/> then the arguments are injected to a new instance.
		/// </para>
		/// <para>
		/// If value is a component on another GameObject then arguments are injected to a new instance
		/// created by cloning the GameObject with the component.
		/// </para>
		/// </summary>
		[MaybeNull]
		Object Target { get; set; }

		/// <summary>
		/// Gets a value indicating whether this initializer is able to
		/// provide custom per-instance Init arguments for its client.
		/// <para>
		/// Initializers like <see cref="Internal.InactiveInitializer"/> don't do that,
		/// but will just call <see cref="IInitializable.Init"/> instead.
		/// </para>
		/// </summary>
		bool ProvidesCustomInitArguments => true;

		/// <summary>
		/// Gets a value indicating whether an object of the given <paramref name="type"/>
		/// can be assigned to the <see cref="Target"/> property directly,
		/// or if <paramref name="type"/> implements <see cref="IValueProvider{T}"/>,
		/// <see cref="IValueByTypeProvider"/> or <see cref="IValueByTypeProviderAsync"/>.
		/// </summary>
		/// <param name="type"> The type to check. </param>
		/// <returns>
		/// <see langword="true"/> if an object of the given <paramref name="type"/>
		/// can be assigned to the property; otherwise, <see langword="false"/>.
		/// </returns>
		public bool TargetIsAssignableOrConvertibleToType(Type type);

		/// <summary>
		/// Initializes the client object with the arguments specified by this initializer.
		/// <para>
		/// If <see cref="Target"/> is attached to the same <see cref="GameObject"/> as the initializer,
		/// then this method initializes the <see cref="IInitializer.Target"/> and returns it.
		/// </para>
		/// <para>
		/// If <see cref="Target"/> is attached to a different <see cref="GameObject"/> than the initializer, like for example a different prefab,
		/// then this method clones the <see cref="IInitializer.Target"/>, initializes the clone, and returns it. 
		/// </para>
		/// <para>
		/// If <see cref="Target"/> is <see langword="null"/>, the initializer is a component, and the client is a component type,
		/// then a new component is attached to the <see cref="GameObject"/> containing the initializer, initialized, and returned.
		/// </para>
		/// <para>
		/// This method should be idempotent, meaning that if it is called multiple times, it should always return the
		/// same object, and that object should only get initialized once.
		/// </para>
		/// </summary>
		/// <returns> The initialized object. </returns>
		[return: NotNull]
		object InitTarget();

		/// <summary>
		/// Initializes the client object asynchronously with the arguments specified by this initializer.
		/// <para>
		/// The method will wait until all dependencies of the client are ready, and only then initialize the target.
		/// For example, if one of the dependencies is an addressable asset, the method can wait until the asset
		/// has finished loading, and then pass it to the client's Init method.
		/// </para>
		/// <para>
		/// Note that if any dependency requires asynchronous loading, and the client is a
		/// component attached to an active scene object, then initialization event functions like Awake, OnEnable
		/// and Start can get called for the target before initialization has finished.
		/// </para>
		/// <para>
		/// If <see cref="Target"/> is attached to the same <see cref="GameObject"/> as the initializer,
		/// then this method initializes the <paramref name="target"/> and returns it.
		/// </para>
		/// <para>
		/// If <see cref="Target"/> is attached to a different <see cref="GameObject"/> than the initializer, like for example a different prefab,
		/// then this method clones the <paramref name="target"/>, initializes the clone, and returns it. 
		/// </para>
		/// <para>
		/// If <see cref="Target"/> is <see langword="null"/>, the initializer is a component, and the client is a component type,
		/// then a new component is attached to the <see cref="GameObject"/> containing the initializer, initialized, and returned.
		/// </para>
		/// <para>
		/// This method should be idempotent, meaning that if it is called multiple times, it should always return an
		/// awaitable with the same result, the result object should only get initialized once.
		/// </para>
		/// </summary>
		/// <returns> The initialized object. </returns>
		[return: NotNull]
		#if UNITY_2023_1_OR_NEWER
		Awaitable<object> InitTargetAsync() => Internal.AwaitableUtility.FromResult(InitTarget());
		#else
		System.Threading.Tasks.Task<object> InitTargetAsync() => System.Threading.Tasks.Task.FromResult(InitTarget());
		#endif
		
	}

	/// <summary>
	/// Represents an Initializer that can can specify the arguments used to initialize an object of type <typeparamref name="TClient"/>.
	/// </summary>
	/// <typeparam name="TClient"> Type of the client whose Init arguments this Initializer specifies. </typeparam>
	public interface IInitializer<TClient> : IInitializer
	{
		/// <summary>
		/// Initializes the client object of type <see cref="TClient"/> with the arguments specified by this initializer.
		/// <para>
		/// If <see cref="IInitializer.Target"/> is attached to the same <see cref="GameObject"/> as the initializer,
		/// then this method initializes the <see cref="IInitializer.Target"/> and returns it.
		/// </para>
		/// <para>
		/// If <see cref="IInitializer.Target"/> is attached to a different <see cref="GameObject"/> than the initializer, like for example a different prefab,
		/// then this method clones the <see cref="IInitializer.Target"/>, initializes the clone, and returns it. 
		/// </para>
		/// <para>
		/// If <see cref="IInitializer.Target"/> is <see langword="null"/>, and the initializer is a component,
		/// then a new component of type <see cref="TClient"/> is attached to the <see cref="GameObject"/>
		/// containing the initializer, initialized, and returned.
		/// </para>
		/// <para>
		/// This method should be idempotent, meaning that if it is called multiple times, it should always return the
		/// same object, and that object should only get initialized once.
		/// </para>
		/// </summary>
		/// <returns> The initialized object of type <see cref="TClient"/>. </returns>
		[return: NotNull]
		new TClient InitTarget();

		/// <summary>
		/// Initializes the client object of type <see cref="TClient"/> asynchronously with the arguments specified by this initializer.
		/// <para>
		/// The method will wait until all dependencies of the client are ready, and only then initialize the target.
		/// For example, if one of the dependencies is an addressable asset, the method can wait until the asset
		/// has finished loading, and then pass it to the client's Init method.
		/// </para>
		/// <para>
		/// Note that if any dependency requires asynchronous loading, and the <paramref name="client"/> is a
		/// component attached to an active scene object, then initialization event functions like Awake, OnEnable
		/// and Start can get called for the target before initialization has finished.
		/// </para>
		/// <para>
		/// If <see cref="IInitializer.Target"/> is attached to the same <see cref="GameObject"/> as the initializer,
		/// then this method initializes the <see cref="IInitializer.Target"/> and returns it.
		/// </para>
		/// <para>
		/// If <see cref="IInitializer.Target"/> is attached to a different <see cref="GameObject"/> than the initializer, like for example a different prefab,
		/// then this method clones the <see cref="IInitializer.Target"/>, initializes the clone, and returns it. 
		/// </para>
		/// <para>
		/// If <see cref="IInitializer.Target"/> is <see langword="null"/>, and the initializer is a component,
		/// then a new component of type <see cref="TClient"/> is attached to the <see cref="GameObject"/>
		/// containing the initializer, initialized, and returned.
		/// </para>
		/// <para>
		/// This method should be idempotent, meaning that if it is called multiple times, it should always return an
		/// awaitable with the same result, the result object should only get initialized once.
		/// </para>
		/// </summary>
		/// <returns> The initialized object. </returns>
		[return: NotNull]
		#if UNITY_2023_1_OR_NEWER
		new Awaitable<TClient> InitTargetAsync() => Internal.AwaitableUtility.FromResult(InitTarget());
		#else
		new System.Threading.Tasks.Task<TClient> InitTargetAsync() => System.Threading.Tasks.Task.FromResult(InitTarget());
		#endif
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
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument> : IInitializer<TClient>  { }

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

	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IInitializer<TClient> { }
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IInitializer<TClient> { }
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IInitializer<TClient> { }
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IInitializer<TClient> { }
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IInitializer<TClient> { }
	public interface IInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IInitializer<TClient> { }
}