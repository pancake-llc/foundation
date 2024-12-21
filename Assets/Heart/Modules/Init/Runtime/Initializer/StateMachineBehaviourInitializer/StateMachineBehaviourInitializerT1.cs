using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the argument used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="StateMachineBehaviourInitializer{,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized state machine behaviour client. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client's Init function. </typeparam>
	public abstract class StateMachineBehaviourInitializer<TStateMachineBehaviour, TArgument>
		: StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TArgument>
			where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TArgument>
	{
		[SerializeField] private Any<TArgument> argument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		protected override TArgument Argument { get => argument.GetValue(this, Context.MainThread); set => argument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncArguments != Arguments.None;

		private protected sealed override async ValueTask<TStateMachineBehaviour> InitTargetAsync(Animator target)
		{
			var argument = await this.argument.GetValueAsync(this, Context.MainThread);

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
			#endif

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

		private protected void OnDestroy()
		{
			if(disposeArgumentsOnDestroy == Arguments.First)
			{
				HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.First, ref argument);
			}
		}

		#if UNITY_EDITOR
		private protected sealed override void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease)
		{
			var setValue = disposeArgumentsOnDestroy.WithFlag(argument, shouldRelease);
			if(disposeArgumentsOnDestroy != setValue)
			{
				disposeArgumentsOnDestroy = setValue;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		private protected sealed override void SetIsArgumentAsyncValueProvider(Arguments argument, bool isAsyncValueProvider)
		{
			var setValue = asyncArguments.WithFlag(argument, isAsyncValueProvider);
			if(asyncArguments != setValue)
			{
				asyncArguments = setValue;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		private protected override NullGuardResult EvaluateNullGuard() => argument.EvaluateNullGuard(this);
		private protected override void OnValidate() => Validate(this, gameObject, argument);
		#endif
	}
}