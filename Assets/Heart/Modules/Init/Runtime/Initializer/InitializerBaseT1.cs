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
	/// A base class for a component that can can specify the argument used to initialize an object that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can be assigned to a member field or a property.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="InitializerBase{,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="Initializer{,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client component's Init function. </typeparam>
	public abstract class InitializerBase<TClient, TArgument> : InitializerBaseInternal<TClient>, IInitializer<TClient, TArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient, TArgument>
		#endif
		where TClient : MonoBehaviour, IInitializable<TArgument>
	{
		/// <summary>
		/// The argument passed to the <typeparamref name="TClient">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		/// <inheritdoc/>
		[return: NotNull]
		private protected override TClient InitTarget([AllowNull] TClient target)
		{
			var argument = Argument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
			#endif

			#if UNITY_EDITOR
			if(!target)
			#else
			if(target is null)
			#endif
			{
				gameObject.AddComponent(out TClient result, argument);
				return result;
			}

			if(target.gameObject != gameObject)
			{
				return target.Instantiate(argument);
			}

			if(target is MonoBehaviour<TArgument> monoBehaviourT)
			{
				monoBehaviourT.InitInternal(argument);
			}
			else
			{
				target.Init(argument);
			}

			return target;
		}

		bool IInitializable.HasInitializer => false;

		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				AutoInitInEditMode<InitializerBase<TClient, TArgument>, TClient, TArgument>(this);
			}
			#endif

			_ = InitTarget();
			return initState is InitState.Initialized or InitState.Initializing;
		}

		/// <summary>
		/// Resets the Init argument to its default value.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// </para>
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to reset. </param>
		protected virtual void OnReset(ref TArgument argument) { }

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentAtRuntime(TArgument argument) => ThrowIfMissing(argument);
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() =>
			initState == InitState.Failed
				? NullGuardResult.ValueProviderException
				: IsNull(Argument)
					? NullGuardResult.ValueMissing
					: NullGuardResult.Passed;

		TArgument IInitializerEditorOnly<TClient, TArgument>.Argument { get => Argument; set => Argument = value; }
		void IInitializerEditorOnly<TClient, TArgument>.OnReset(ref TArgument argument) => OnReset(ref argument);
		private protected sealed override void Reset() => Reset<InitializerBase<TClient, TArgument>, TClient, TArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, Argument);
		#endif
	}
}