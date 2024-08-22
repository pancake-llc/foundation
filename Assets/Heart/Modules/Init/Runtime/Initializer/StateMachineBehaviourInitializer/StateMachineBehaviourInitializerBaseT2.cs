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
	/// A base class for an animator component that can be used to specify the two arguments used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>.
	/// <para>
	/// The arguments are injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the Awake event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="StateMachineBehaviourInitializerBase{,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="StateMachineBehaviourInitializer{,,}"/> instead,
	/// then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized state machine behaviour client. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client's Init function. </typeparam>
	public abstract class StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument> : StateMachineBehaviourInitializerBaseInternal<TStateMachineBehaviour>, IInitializer<TStateMachineBehaviour, TFirstArgument, TSecondArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TStateMachineBehaviour, TFirstArgument, TSecondArgument>
		#endif
		where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TFirstArgument, TSecondArgument>
	{
		/// <summary>
		/// The first argument passed to the <typeparamref name="TStateMachineBehaviour">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TFirstArgument FirstArgument { get; set; }

		/// <summary>
		/// The second argument passed to the <typeparamref name="TStateMachineBehaviour">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TSecondArgument SecondArgument { get; set; }

		/// <inheritdoc/>
		[return: NotNull]
		private protected override TStateMachineBehaviour InitTarget([DisallowNull] Animator target)
		{
			var firstArgument = FirstArgument;
			var secondArgument = SecondArgument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument);
			#endif

			var behaviours = target.GetBehaviours<TStateMachineBehaviour>();
			int count = behaviours.Length;
			
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(count == 0) throw new MissingComponentException($"No {typeof(TStateMachineBehaviour).Name} was found in the Animator on '{name}'.", null);
			#endif

			for(int i = count - 1; i >= 0; i--)
			{
				behaviours[i].Init(firstArgument, secondArgument);
			}

			return behaviours[0];
		}

		bool IInitializable.HasInitializer => false;
		
		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				AutoInitInEditMode<StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument>, TStateMachineBehaviour, TFirstArgument, TSecondArgument>(this);
			}
			#endif

			InitTarget();
			return true;
		}

		/// <summary>
		/// Resets the Init arguments to their default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the initializer to an Animator the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </summary>
		/// <param name="firstArgument"> The first argument to reset. </param>
		/// <param name="secondArgument"> The second argument to reset. </param>
		protected virtual void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument) { }

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentsAtRuntime(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			ThrowIfMissing(firstArgument); ThrowIfMissing(secondArgument);
		}
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() => IsNull(FirstArgument) || IsNull(SecondArgument) ? NullGuardResult.ValueMissing : NullGuardResult.Passed;
		TFirstArgument IInitializerEditorOnly<TStateMachineBehaviour, TFirstArgument, TSecondArgument>.FirstArgument { get => FirstArgument; set => FirstArgument = value; }
		TSecondArgument IInitializerEditorOnly<TStateMachineBehaviour, TFirstArgument, TSecondArgument>.SecondArgument { get => SecondArgument; set => SecondArgument = value; }
		void IInitializerEditorOnly<TStateMachineBehaviour, TFirstArgument, TSecondArgument>.OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument) => OnReset(ref firstArgument, ref secondArgument);
		private protected sealed override void Reset() => Reset<StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument>, TStateMachineBehaviour, TFirstArgument, TSecondArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, FirstArgument, SecondArgument);
		#endif
	}
}