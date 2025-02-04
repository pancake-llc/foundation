using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the constructor argument used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializer<TWrapper, TWrapped, TArgument> : WrapperInitializerBase<TWrapper, TWrapped, TArgument> where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField] private Any<TArgument> argument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		/// <inheritdoc/>
		protected override TArgument Argument { get => argument.GetValue(this, Context.MainThread); set => argument = value; }

		protected override bool IsRemovedAfterTargetInitialized => disposeArgumentsOnDestroy == Arguments.None;
		private protected override bool IsAsync => asyncArguments != Arguments.None;

		private protected sealed override async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TWrapper>
		#else
		System.Threading.Tasks.Task<TWrapper>
		#endif
		InitTargetAsync(TWrapper wrapper)
		{
			// Handle instance first creation method, which supports cyclical dependencies (A requires B, and B requires A).
			if(wrapper is IInitializable<TArgument> initializable
				&& GetOrCreateUninitializedWrappedObject() is var wrappedObject)
			{
				wrapper = InitWrapper(wrappedObject);

				var argument = await this.argument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref argument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
				#endif

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
				#endif

				initializable.Init(argument);
			}
			// Handle arguments first creation method, which supports constructor injection.
			else
			{
				var argument = await this.argument.GetValueAsync(this, Context.MainThread);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(disposeArgumentsOnDestroy == Arguments.First) OptimizeValueProviderNameForDebugging(this, this.argument);
				#endif

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
				#endif

				wrappedObject = CreateWrappedObject(argument);
				wrapper = InitWrapper(wrappedObject);
			}
			
			return wrapper;
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

		private protected sealed override void SetIsArgumentAsync(Arguments argument, bool isAsyncValueProvider)
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