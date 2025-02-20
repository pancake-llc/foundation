using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the argument used to
	/// initialize a component that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument can be assigned using the inspector and is serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the Awake event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can be assigned to a member field or property.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TArgument> : InitializerBase<TClient, TArgument> where TClient : MonoBehaviour, IInitializable<TArgument>
	{
		[SerializeField] private Any<TArgument> argument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"), SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		protected override TArgument Argument { get => argument.GetValue(this, Context.MainThread); set => argument = value; }

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
			var argument = await this.argument.GetValueAsync(this, Context.MainThread);

			#if DEBUG
			if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
			#endif

			#if UNITY_EDITOR
			if(!target)
			#else
			if(target is null)
			#endif
			{
				gameObject.AddComponent(out TClient result, argument);
				return result;
			}

			if(target.gameObject != gameObject)
			{
				#if UNITY_6000_0_OR_NEWER
				TClient[] results = await target.InstantiateAsync(argument);
				return results[0];
				#else
				return target.Instantiate(argument);
				#endif
			}

			if(target is MonoBehaviour<TArgument> monoBehaviourT)
			{
				monoBehaviourT.InitInternal(argument);
			}
			else
			{
				target.Init(argument);
			}

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
				: argument.EvaluateNullGuard(this);

		private protected override void OnValidate() => Validate(this, gameObject, argument);
		#endif
	}
}