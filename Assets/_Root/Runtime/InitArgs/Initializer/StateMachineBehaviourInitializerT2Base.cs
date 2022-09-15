#pragma warning disable CS0414

using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
using static Pancake.Init.Internal.InitializerUtility;
using static Pancake.NullExtensions;

#if UNITY_EDITOR
using Pancake.Editor.Init;
#endif


namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the three arguments used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>.
	/// <para>
	/// The arguments are injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="Initializer{,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
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
	public abstract class StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument> : MonoBehaviour, 
		IInitializer<TStateMachineBehaviour, TFirstArgument, TSecondArgument>, IValueProvider<TStateMachineBehaviour>
		#if UNITY_EDITOR
		, IInitializerEditorOnly
		#endif
		where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TFirstArgument, TSecondArgument>
	{
		
		[SerializeField, HideInInspector, Tooltip("Target Animator that contains the StateMachineBehaviour to initialize.")]
		protected Animator target = null;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private NullArgumentGuard nullArgumentGuard = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException;

		/// <inheritdoc/>
		TStateMachineBehaviour IValueProvider<TStateMachineBehaviour>.Value => target.GetBehaviour<TStateMachineBehaviour>();

		/// <inheritdoc/>
		object IValueProvider.Value => target;

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = (Animator)value; }

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(Animator));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <summary>
		/// The first argument passed to the <typeparamref name="TStateMachineBehaviour">state machine behaviour</typeparamref>'s Init function.
		/// </summary>
		protected abstract TFirstArgument FirstArgument { get; set; }

		/// <summary>
		/// The second argument passed to the <typeparamref name="TStateMachineBehaviour">state machine behaviour</typeparamref>'s Init function.
		/// </summary>
		protected abstract TSecondArgument SecondArgument { get; set; }

		#if UNITY_EDITOR
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(FirstArgument) || IsNull(SecondArgument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => true;
		#endif
		
		/// <inheritdoc/>
		public TStateMachineBehaviour InitTarget()
		{
			if(this == null)
			{
				return target == null ? null : target.GetBehaviour<TStateMachineBehaviour>();
			}

			var firstArgument = FirstArgument;
			var secondArgument = SecondArgument;

#if DEBUG || INIT_ARGS_SAFE_MODE
			if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
			{
				if(firstArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TStateMachineBehaviour), typeof(TFirstArgument));
				if(secondArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TStateMachineBehaviour), typeof(TSecondArgument));
			}
#endif

			Updater.InvokeAtEndOfFrame(DestroySelf);
			return InitTarget(firstArgument, secondArgument);
		}


		protected virtual void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument) { }

		/// <summary>
		/// Initializes the <see cref="TStateMachineBehaviour"/> behaviour inside the <see cref="target"/> <see cref="Animator"/> using the provided argument.
		/// </summary>
		/// <param name="argument"> The argument to pass to the behaviour's Init function. </param>
		/// <returns>
		/// The existing <see cref="TStateMachineBehaviour"/> that was initialized.
		/// </returns>
		[NotNull]
		protected virtual TStateMachineBehaviour InitTarget(TFirstArgument firstArgument, TSecondArgument secondArgument)
        {
			Animator animator = target;
			if(target.gameObject != gameObject)
			{
				InitArgs.Set<TStateMachineBehaviour, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);
				animator = Instantiate(target);
				if(!InitArgs.Clear<TStateMachineBehaviour, TFirstArgument, TSecondArgument>())
				{
					return animator.GetBehaviours<TStateMachineBehaviour>()[0];
				}
			}

			var behaviours = animator.GetBehaviours<TStateMachineBehaviour>();
			int count = behaviours.Length;
			
			#if DEBUG
			if(count == 0) throw new MissingComponentException($"No {typeof(TStateMachineBehaviour).Name} was found in the Animator '{animator.name}'.", null);
			#endif

			for(int i = count - 1; i >= 0; i--)
			{
				behaviours[i].Init(firstArgument, secondArgument);
			}

			return behaviours[0];
        }

		#if UNITY_EDITOR
        private void Reset()
		{
			var set = HandleReset(this, ref target, FirstArgument, SecondArgument, OnReset);
			if(!AreEqual(FirstArgument, set.firstArgument)) FirstArgument = set.firstArgument;
			if(!AreEqual(SecondArgument, set.secondArgument)) SecondArgument = set.secondArgument;
		}

		private void OnValidate() => OnMainThread(Validate);
		#endif

		protected virtual void Validate()
		{
			#if UNITY_EDITOR
			ValidateOnMainThread(this);
			ValidateTargetOnMainThread<StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument>, TStateMachineBehaviour>(this);
			#endif
		}

		private void Awake() => InitTarget();

		private void DestroySelf()
		{
			if(this != null)
			{
				Destroy(this);
			}
		}
	}
}