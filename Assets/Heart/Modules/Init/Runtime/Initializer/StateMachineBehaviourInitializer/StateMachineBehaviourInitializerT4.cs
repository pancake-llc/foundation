using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the four arguments used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="StateMachineBehaviourInitializer{,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized state machine behaviour client. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument to pass to the client's Init function. </typeparam>
	public abstract class StateMachineBehaviourInitializer<TStateMachineBehaviour, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		: StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		[SerializeField] private Any<TFirstArgument> firstArgument = default;
		[SerializeField] private Any<TSecondArgument> secondArgument = default;
		[SerializeField] private Any<TThirdArgument> thirdArgument = default;
		[SerializeField] private Any<TFourthArgument> fourthArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValue(this, Context.MainThread); set => thirdArgument = value; }
		protected override TFourthArgument FourthArgument { get => fourthArgument.GetValue(this, Context.MainThread); set => fourthArgument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncArguments != Arguments.None;

		private protected sealed override async ValueTask<TStateMachineBehaviour> InitTargetAsync(Animator target)
		{
			var firstArgument = await this.firstArgument.GetValueAsync(this, Context.MainThread);
			var secondArgument = await this.secondArgument.GetValueAsync(this, Context.MainThread);
			var thirdArgument = await this.thirdArgument.GetValueAsync(this, Context.MainThread);
			var fourthArgument = await this.fourthArgument.GetValueAsync(this, Context.MainThread);

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(disposeArgumentsOnDestroy != Arguments.None)
			{
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.First)) OptimizeValueProviderNameForDebugging(this, this.firstArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Second)) OptimizeValueProviderNameForDebugging(this, this.secondArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Third)) OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fourth)) OptimizeValueProviderNameForDebugging(this, this.fourthArgument);
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument);
			#endif

			var behaviours = target.GetBehaviours<TStateMachineBehaviour>();
			int count = behaviours.Length;
			
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(count == 0) throw new MissingComponentException($"No {typeof(TStateMachineBehaviour).Name} was found in the Animator on '{name}'.", null);
			#endif

			for(int i = count - 1; i >= 0; i--)
			{
				behaviours[i].Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
			}

			return behaviours[0];
		}

		private protected void OnDestroy()
		{
			if(disposeArgumentsOnDestroy == Arguments.None)
			{
				return;
			}

			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.First, ref firstArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Second, ref secondArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Third, ref thirdArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Fourth, ref fourthArgument);
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

		private protected override NullGuardResult EvaluateNullGuard() => firstArgument.EvaluateNullGuard(this)
																 .Join(secondArgument.EvaluateNullGuard(this))
																 .Join(thirdArgument.EvaluateNullGuard(this))
																 .Join(fourthArgument.EvaluateNullGuard(this));

		private protected override void OnValidate() => Validate(this, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument);
		#endif
	}
}