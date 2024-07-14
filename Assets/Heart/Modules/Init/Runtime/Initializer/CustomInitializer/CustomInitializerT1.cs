using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the argument used to initialize an object of type <typeparamref name="TClient"/>.
	/// <para>
	/// The argument can be assigned using the inspector and is serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The <typeparamref name="TClient">client</typeparamref> does not need to implement the <see cref="IInitializable{TArgument}"/> interface.
	/// The initialization argument can instead be injected, for example, directly into a property with a public setter.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to inject to the client. </typeparam>
	public abstract class CustomInitializer<TClient, TArgument> : CustomInitializerBase<TClient, TArgument> where TClient : Component
	{
		[SerializeField] private protected Any<TArgument> argument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[SerializeField, HideInInspector] private Arguments asyncValueProviderArguments = Arguments.None;

		/// <inheritdoc/>
		protected override TArgument Argument { get => argument.GetValue(this, Context.MainThread); set => argument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncValueProviderArguments != Arguments.None;

		private protected sealed override async ValueTask<TClient> InitTargetAsync(TClient target)
		{
			var argument = await this.argument.GetValueAsync(this, Context.MainThread);

			#if DEBUG
			if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
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

			InitTarget(target, argument);
			return target;
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
			var setValue = asyncValueProviderArguments.WithFlag(argument, isAsyncValueProvider);
			if(asyncValueProviderArguments != setValue)
			{
				asyncValueProviderArguments = setValue;
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		private protected override NullGuardResult EvaluateNullGuard() => argument.EvaluateNullGuard(this);
		private protected override void OnValidate() => Validate(this, gameObject, argument);
		#endif
	}
}