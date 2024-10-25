#pragma warning disable CS0414

using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the argument used to initialize an object of type <typeparamref name="TClient"/>.
	/// <para>
	/// The argument can be assigned using the inspector and is serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The <typeparamref name="TClient">client</typeparamref> does not need to implement the
	/// <see cref="IInitializable{TArgument}"/> interface.
	/// The initialization argument can instead be injected, for example, directly into a property with a public setter.
	/// </para>
	/// <para>
	/// After the argument has been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your initializer class from <see cref="CustomInitializerBase{,}"/>
	/// you are responsible for implementing the argument property and serializing its values.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="CustomInitializer{,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to inject to the client. </typeparam>
	public abstract class CustomInitializerBase<TClient, TArgument>
		: InitializerBaseInternal<TClient>, IInitializer<TClient, TArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient, TArgument>
		#endif
		where TClient : Component
	{
		/// <summary>
		/// The first argument injected to the <typeparamref name="TClient">client</typeparamref>.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		[return: NotNull]
		private protected sealed override TClient InitTarget([AllowNull] TClient target)
		{
			var argument = Argument;

			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				InitTarget(target, argument);
				return target;
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
			#endif

			#if UNITY_EDITOR
			if(!target)
			#else
			if(target is null)
			#endif
			{
				target = gameObject.AddComponent<TClient>();
			}
			else if(target.gameObject != gameObject)
			{
				target = Instantiate(target);
			}

			InitTarget(target, argument);
			return target;
		}

		/// <summary>
		/// Initializes the <paramref name="target"/> of type <see cref="TClient"/> using the provided argument.
		/// </summary>
		/// <param name="target"> The target to initialize. </param>
		/// <param name="argument"> The argument used to initialize the target. </param>
		protected abstract void InitTarget(TClient target, TArgument argument);

		bool IInitializable.HasInitializer => false;

		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				AutoInitInEditMode<CustomInitializerBase<TClient, TArgument>, TClient, TArgument>(this);
			}
			#endif

			InitTarget();
			return true;
		}

		/// <summary>
		/// Resets the initialization argument to its default value.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </summary>
		/// <param name="argument"> The argument to reset. </param>
		protected virtual void OnReset(ref TArgument argument) { }

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentAtRuntime(TArgument argument) => ThrowIfMissing(argument);
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() => IsNull(Argument) ? NullGuardResult.ValueMissing : NullGuardResult.Passed;
		TArgument IInitializerEditorOnly<TClient, TArgument>.Argument { get => Argument; set => Argument = value; }
		void IInitializerEditorOnly<TClient, TArgument>.OnReset(ref TArgument argument) => OnReset(ref argument);
		private protected sealed override void Reset() => Reset<CustomInitializerBase<TClient, TArgument>, TClient, TArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, Argument);
		#endif
	}
}