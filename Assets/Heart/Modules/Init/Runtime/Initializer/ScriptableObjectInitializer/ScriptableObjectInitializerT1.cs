using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for an initializer that can be used to specify the argument used to
	/// initialize a scriptable object that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument can be assigned using the inspector and is serialized as a sub-asset of the client scriptable object.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the client's <see cref="Awake"/> event, or when services become ready (whichever occurs later).
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign it to a member field or property.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized scriptable object. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client's Init function. </typeparam>
	public abstract class ScriptableObjectInitializer<TClient, TArgument> : ScriptableObjectInitializerBase<TClient, TArgument> where TClient : ScriptableObject, IInitializable<TArgument>
	{
		[SerializeField] private Any<TArgument> argument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

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
			var argument = await this.argument.GetValueAsync();

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
			#endif

			if(target == null)
			{
				Create.Instance(out target, argument);
				return target;
			}
			
			if(target is ScriptableObject<TArgument> scriptableObjectT)
			{
				scriptableObjectT.InitInternal(argument);
				return target;
			}

			target.Init(argument);
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

		private protected override NullGuardResult EvaluateNullGuard() => argument.EvaluateNullGuard();
		private protected override void OnValidate() => Validate(this, null, argument);
		#endif
	}
}