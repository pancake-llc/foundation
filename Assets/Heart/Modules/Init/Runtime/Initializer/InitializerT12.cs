using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the twelve arguments used to initialize a component that implements
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref> during the Awake event.
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">Init</see>
	/// method where they can be assigned to member fields or properties.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TEleventhArgument"> Type of the eleventh argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		: InitializerBase<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			where TClient : MonoBehaviour, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
	{
		[SerializeField] private Any<TFirstArgument> firstArgument = default;
		[SerializeField] private Any<TSecondArgument> secondArgument = default;
		[SerializeField] private Any<TThirdArgument> thirdArgument = default;
		[SerializeField] private Any<TFourthArgument> fourthArgument = default;
		[SerializeField] private Any<TFifthArgument> fifthArgument = default;
		[SerializeField] private Any<TSixthArgument> sixthArgument = default;
		[SerializeField] private Any<TSeventhArgument> seventhArgument = default;
		[SerializeField] private Any<TEighthArgument> eighthArgument = default;
		[SerializeField] private Any<TNinthArgument> ninthArgument = default;
		[SerializeField] private Any<TTenthArgument> tenthArgument = default;
		[SerializeField] private Any<TEleventhArgument> eleventhArgument = default;
		[SerializeField] private Any<TTwelfthArgument> twelfthArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValue(this, Context.MainThread); set => thirdArgument = value; }
		protected override TFourthArgument FourthArgument { get => fourthArgument.GetValue(this, Context.MainThread); set => fourthArgument = value; }
		protected override TFifthArgument FifthArgument { get => fifthArgument.GetValue(this, Context.MainThread); set => fifthArgument = value; }
		protected override TSixthArgument SixthArgument { get => sixthArgument.GetValue(this, Context.MainThread); set => sixthArgument = value; }
		protected override TSeventhArgument SeventhArgument { get => seventhArgument.GetValue(this, Context.MainThread); set => seventhArgument = value; }
		protected override TEighthArgument EighthArgument { get => eighthArgument.GetValue(this, Context.MainThread); set => eighthArgument = value; }
		protected override TNinthArgument NinthArgument { get => ninthArgument.GetValue(this, Context.MainThread); set => ninthArgument = value; }
		protected override TTenthArgument TenthArgument { get => tenthArgument.GetValue(this, Context.MainThread); set => tenthArgument = value; }
		protected override TEleventhArgument EleventhArgument { get => eleventhArgument.GetValue(this, Context.MainThread); set => eleventhArgument = value; }
		protected override TTwelfthArgument TwelfthArgument { get => twelfthArgument.GetValue(this, Context.MainThread); set => twelfthArgument = value; }

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
			var firstArgument = await this.firstArgument.GetValueAsync(this, Context.MainThread);
			var secondArgument = await this.secondArgument.GetValueAsync(this, Context.MainThread);
			var thirdArgument = await this.thirdArgument.GetValueAsync(this, Context.MainThread);
			var fourthArgument = await this.fourthArgument.GetValueAsync(this, Context.MainThread);
			var fifthArgument = await this.fifthArgument.GetValueAsync(this, Context.MainThread);
			var sixthArgument = await this.sixthArgument.GetValueAsync(this, Context.MainThread);
			var seventhArgument = await this.seventhArgument.GetValueAsync(this, Context.MainThread);
			var eighthArgument = await this.eighthArgument.GetValueAsync(this, Context.MainThread);
			var ninthArgument = await this.ninthArgument.GetValueAsync(this, Context.MainThread);
			var tenthArgument = await this.tenthArgument.GetValueAsync(this, Context.MainThread);
			var eleventhArgument = await this.eleventhArgument.GetValueAsync(this, Context.MainThread);
			var twelfthArgument = await this.twelfthArgument.GetValueAsync(this, Context.MainThread);

			#if DEBUG
			if(disposeArgumentsOnDestroy != Arguments.None)
			{
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.First)) OptimizeValueProviderNameForDebugging(this, this.firstArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Second)) OptimizeValueProviderNameForDebugging(this, this.secondArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Third)) OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fourth)) OptimizeValueProviderNameForDebugging(this, this.fourthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fifth)) OptimizeValueProviderNameForDebugging(this, this.fifthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Sixth)) OptimizeValueProviderNameForDebugging(this, this.sixthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Seventh)) OptimizeValueProviderNameForDebugging(this, this.seventhArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Eighth)) OptimizeValueProviderNameForDebugging(this, this.eighthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Ninth)) OptimizeValueProviderNameForDebugging(this, this.ninthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Tenth)) OptimizeValueProviderNameForDebugging(this, this.tenthArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Eleventh)) OptimizeValueProviderNameForDebugging(this, this.eleventhArgument);
				if(disposeArgumentsOnDestroy.HasFlag(Arguments.Twelfth)) OptimizeValueProviderNameForDebugging(this, this.twelfthArgument);
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			#endif

			#if UNITY_EDITOR
			if(!target)
			#else
			if(target is null)
			#endif
			{
				gameObject.AddComponent(out TClient result, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return result;
			}

			if(target.gameObject != gameObject)
			{
				#if UNITY_6000_0_OR_NEWER
				var results = await target.InstantiateAsync(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return results[0];
				#else
				return target.Instantiate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				#endif
			}

			if(target is MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> monoBehaviourT)
			{
				monoBehaviourT.InitInternal(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			}
			else
			{
				target.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			}

			return target;
		}

		private protected void OnDestroy()
		{
			#if DEV_MODE
			Debug.Log($"{GetType().Name}.OnDestroy() with disposeArgumentsOnDestroy:{disposeArgumentsOnDestroy}");
			#endif

			if(disposeArgumentsOnDestroy == Arguments.None)
			{
				return;
			}

			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.First, ref firstArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Second, ref secondArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Third, ref thirdArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Fourth, ref fourthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Fifth, ref fifthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Sixth, ref sixthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Seventh, ref seventhArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Eighth, ref eighthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Ninth, ref ninthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Tenth, ref tenthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Eleventh, ref eleventhArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Twelfth, ref twelfthArgument);
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

		private protected sealed override void SetIsArgumentAsync(Arguments argument, bool isAsync)
		{
			var setValue = asyncArguments.WithFlag(argument, isAsync);
			if(asyncArguments != setValue)
			{
				asyncArguments = setValue;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		private protected override NullGuardResult EvaluateNullGuard() =>
			initState == InitState.Failed
				? NullGuardResult.ValueProviderException
				: firstArgument.EvaluateNullGuard(this)
				  .Join(secondArgument.EvaluateNullGuard(this))
				  .Join(thirdArgument.EvaluateNullGuard(this))
				  .Join(fourthArgument.EvaluateNullGuard(this))
				  .Join(fifthArgument.EvaluateNullGuard(this))
				  .Join(sixthArgument.EvaluateNullGuard(this))
				  .Join(seventhArgument.EvaluateNullGuard(this))
				  .Join(eighthArgument.EvaluateNullGuard(this))
				  .Join(ninthArgument.EvaluateNullGuard(this))
				  .Join(tenthArgument.EvaluateNullGuard(this))
				  .Join(eleventhArgument.EvaluateNullGuard(this))
				  .Join(twelfthArgument.EvaluateNullGuard(this));

		private protected override void OnValidate() => Validate(this, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
		#endif
	}
}