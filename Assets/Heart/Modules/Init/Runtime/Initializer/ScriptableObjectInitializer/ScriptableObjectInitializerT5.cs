using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that can be used to specify the five arguments used to
	/// initialize a scriptable object that implements
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and serialized as part of the
	/// client asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the client's <see cref="Awake"/> event, or when services become ready (whichever occurs later).
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see>
	/// method where it can assign them to member fields and properties.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized scriptable object. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument to pass to the client's Init function. </typeparam>
	public abstract class ScriptableObjectInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		: ScriptableObjectInitializerBase<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			where TClient : ScriptableObject, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		[SerializeField] private Any<TFirstArgument> firstArgument = default;
		[SerializeField] private Any<TSecondArgument> secondArgument = default;
		[SerializeField] private Any<TThirdArgument> thirdArgument = default;
		[SerializeField] private Any<TFourthArgument> fourthArgument = default;
		[SerializeField] private Any<TFifthArgument> fifthArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValue(this, Context.MainThread); set => thirdArgument = value; }
		protected override TFourthArgument FourthArgument { get => fourthArgument.GetValue(this, Context.MainThread); set => fourthArgument = value; }
		protected override TFifthArgument FifthArgument { get => fifthArgument.GetValue(this, Context.MainThread); set => fifthArgument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncArguments != Arguments.None;

		private protected sealed override async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif	
		InitTargetAsync(TClient target)
		{
			var firstArgument = await this.firstArgument.GetValueAsync();
			var secondArgument = await this.secondArgument.GetValueAsync();
			var thirdArgument = await this.thirdArgument.GetValueAsync();
			var fourthArgument = await this.fourthArgument.GetValueAsync();
			var fifthArgument = await this.fifthArgument.GetValueAsync();

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(disposeArgumentsOnDestroy != Arguments.None)
			{
				OptimizeValueProviderNameForDebugging(this, this.firstArgument);
				OptimizeValueProviderNameForDebugging(this, this.secondArgument);
				OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
				OptimizeValueProviderNameForDebugging(this, this.fourthArgument);
				OptimizeValueProviderNameForDebugging(this, this.fifthArgument);
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			#endif

			if(target == null)
			{
				Create.Instance(out target, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return target;
			}
			
			if(target is ScriptableObject<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> scriptableObjectT)
			{
				scriptableObjectT.InitInternal(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return target;
			}

			target.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			return target;
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

		private protected override NullGuardResult EvaluateNullGuard() => firstArgument.EvaluateNullGuard()
																	.Join(secondArgument.EvaluateNullGuard())
																	.Join(thirdArgument.EvaluateNullGuard())
																	.Join(fourthArgument.EvaluateNullGuard())
																	.Join(fifthArgument.EvaluateNullGuard());

		private protected override void OnValidate() => Validate(this, null, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
		#endif
	}
}