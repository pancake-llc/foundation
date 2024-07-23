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
	/// A base class for an animator component that can be used to specify the argument used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument is injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the Awake event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="StateMachineBehaviourInitializerBase{,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="StateMachineBehaviourInitializer{,}"/> instead,
	/// then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized state machine behaviour client. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client's Init function. </typeparam>
	public abstract class StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TArgument> : StateMachineBehaviourInitializerBaseInternal<TStateMachineBehaviour>, IInitializer<TStateMachineBehaviour, TArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TStateMachineBehaviour, TArgument>
		#endif
		where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TArgument>
	{
		/// <summary>
		/// The argument passed to the <typeparamref name="TStateMachineBehaviour">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		/// <inheritdoc/>
		[return: NotNull]
		private protected override TStateMachineBehaviour InitTarget([DisallowNull] Animator target)
		{
			var argument = Argument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
			#endif

			var behaviours = target.GetBehaviours<TStateMachineBehaviour>();
			int count = behaviours.Length;
			
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(count == 0) throw new MissingComponentException($"No {typeof(TStateMachineBehaviour).Name} was found in the Animator on '{name}'.", null);
			#endif

			for(int i = count - 1; i >= 0; i--)
			{
				behaviours[i].Init(argument);
			}

			return behaviours[0];
		}

		bool IInitializable.HasInitializer => false;
		
		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				AutoInitInEditMode<StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TArgument>, TStateMachineBehaviour, TArgument>(this);
			}
			#endif

			InitTarget();
			return true;
		}

		/// <summary>
		/// Resets the Init argument to their default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the initializer to an Animator the first time.
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
		TArgument IInitializerEditorOnly<TStateMachineBehaviour, TArgument>.Argument { get => Argument; set => Argument = value; }
		void IInitializerEditorOnly<TStateMachineBehaviour, TArgument>.OnReset(ref TArgument argument) => OnReset(ref argument);
		private protected sealed override void Reset() => Reset<StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TArgument>, TStateMachineBehaviour, TArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, Argument);
		#endif
	}
}