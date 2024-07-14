using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the two arguments used to initialize an object of type <typeparamref name="TClient"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The <typeparamref name="TClient">client</typeparamref> does not need to implement the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface.
	/// The initialization arguments can instead be injected, for example, directly into properties with public setters.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to inject to the client. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to inject to the client. </typeparam>
	public abstract class CustomInitializer<TClient, TFirstArgument, TSecondArgument>
		: CustomInitializerBase<TClient, TFirstArgument, TSecondArgument>
			where TClient : Component
	{
		[SerializeField] private Any<TFirstArgument> firstArgument = default;
		[SerializeField] private Any<TSecondArgument> secondArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[SerializeField, HideInInspector] private Arguments asyncValueProviderArguments = Arguments.None;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncValueProviderArguments != Arguments.None;

		private protected sealed override async ValueTask<TClient> InitTargetAsync(TClient target)
		{
			var firstArgument = await this.firstArgument.GetValueAsync(this, Context.MainThread);
			var secondArgument = await this.secondArgument.GetValueAsync(this, Context.MainThread);

			#if DEBUG
			if(disposeArgumentsOnDestroy == Arguments.First)
			{
				OptimizeValueProviderNameForDebugging(this, this.firstArgument);
				OptimizeValueProviderNameForDebugging(this, this.secondArgument);
			}
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument);
			#endif

			#if UNITY_EDITOR
			if(target == null)
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

			InitTarget(target, firstArgument, secondArgument);
			return target;
		}

		private protected void OnDestroy()
		{
			if(disposeArgumentsOnDestroy == Arguments.None)
			{
				return;
			}

			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.First, ref firstArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Second, ref secondArgument);
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
															.Join(secondArgument.EvaluateNullGuard(this));

		private protected override void OnValidate() => Validate(this, gameObject, firstArgument, secondArgument);
		#endif
	}
}