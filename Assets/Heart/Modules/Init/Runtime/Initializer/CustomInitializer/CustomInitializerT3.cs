using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the three arguments used to initialize an object of type <typeparamref name="TClient"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The <typeparamref name="TClient">client</typeparamref> does not need to implement the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface.
	/// The initialization arguments can instead be injected, for example, directly into properties with public setters.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to inject to the client. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to inject to the client. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to inject to the client. </typeparam>
	public abstract class CustomInitializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
		: CustomInitializerBase<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			where TClient : Component
	{
		[SerializeField] private protected Any<TFirstArgument> firstArgument = default;
		[SerializeField] private protected Any<TSecondArgument> secondArgument = default;
		[SerializeField] private protected Any<TThirdArgument> thirdArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[SerializeField, HideInInspector] private Arguments asyncValueProviderArguments = Arguments.None;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }
		/// <inheritdoc/>
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValue(this, Context.MainThread); set => thirdArgument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncValueProviderArguments != Arguments.None;

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

			#if UNITY_2022_3_OR_NEWER && (UNITY_EDITOR || INIT_ARGS_SAFE_MODE)
			if(destroyCancellationToken.IsCancellationRequested) return default;
			#endif

			#if DEBUG
			if(disposeArgumentsOnDestroy == Arguments.First)
			{
				OptimizeValueProviderNameForDebugging(this, this.firstArgument);
				OptimizeValueProviderNameForDebugging(this, this.secondArgument);
				OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument);
			#endif

			TClient result;
			#if UNITY_EDITOR
			if(target == null)
			#else
			if(!target)
			#endif
			{
				result = gameObject.AddComponent<TClient>();
			}
			else if(target.gameObject != gameObject)
			{
				result = Instantiate(target);
			}
			else
			{
				result = target;
			}

			InitTarget(result, firstArgument, secondArgument, thirdArgument);
			return result;
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
			var setValue = asyncValueProviderArguments.WithFlag(argument, isAsyncValueProvider);
			if(asyncValueProviderArguments != setValue)
			{
				asyncValueProviderArguments = setValue;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
		
		private protected override NullGuardResult EvaluateNullGuard() => firstArgument.EvaluateNullGuard(this)
															.Join(secondArgument.EvaluateNullGuard(this))
															.Join(thirdArgument.EvaluateNullGuard(this));

		private protected override void OnValidate() => Validate(this, gameObject, firstArgument, secondArgument, thirdArgument);
		#endif
	}
}