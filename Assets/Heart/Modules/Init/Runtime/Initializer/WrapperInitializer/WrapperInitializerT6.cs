﻿using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the six constructor arguments used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializer<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		: WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField] private Any<TFirstArgument> firstArgument = default;
		[SerializeField] private Any<TSecondArgument> secondArgument = default;
		[SerializeField] private Any<TThirdArgument> thirdArgument = default;
		[SerializeField] private Any<TFourthArgument> fourthArgument = default;
		[SerializeField] private Any<TFifthArgument> fifthArgument = default;
		[SerializeField] private Any<TSixthArgument> sixthArgument = default;

		[SerializeField, HideInInspector] private Arguments disposeArgumentsOnDestroy = Arguments.None;
		[FormerlySerializedAs("asyncValueProviderArguments"),SerializeField, HideInInspector] private Arguments asyncArguments = Arguments.None;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValue(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValue(this, Context.MainThread); set => secondArgument = value; }
		/// <inheritdoc/>
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValue(this, Context.MainThread); set => thirdArgument = value; }
		/// <inheritdoc/>
		protected override TFourthArgument FourthArgument { get => fourthArgument.GetValue(this, Context.MainThread); set => fourthArgument = value; }
		/// <inheritdoc/>
		protected override TFifthArgument FifthArgument { get => fifthArgument.GetValue(this, Context.MainThread); set => fifthArgument = value; }
		/// <inheritdoc/>
		protected override TSixthArgument SixthArgument { get => sixthArgument.GetValue(this, Context.MainThread); set => sixthArgument = value; }

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
			if(wrapper is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable
				&& GetOrCreateUninitializedWrappedObject() is var wrappedObject)
			{
				wrapper = InitWrapper(wrappedObject);

				var firstArgument = await this.firstArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref firstArgument);
				var secondArgument = await this.secondArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref secondArgument);
				var thirdArgument = await this.thirdArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref thirdArgument);
				var fourthArgument = await this.fourthArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref fourthArgument);
				var fifthArgument = await this.fifthArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref fifthArgument);
				var sixthArgument = await this.sixthArgument.GetValueAsync(this, Context.MainThread);
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref sixthArgument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(disposeArgumentsOnDestroy != Arguments.None)
				{
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.First)) OptimizeValueProviderNameForDebugging(this, this.firstArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Second)) OptimizeValueProviderNameForDebugging(this, this.secondArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Third)) OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fourth)) OptimizeValueProviderNameForDebugging(this, this.fourthArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fifth)) OptimizeValueProviderNameForDebugging(this, this.fifthArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Sixth)) OptimizeValueProviderNameForDebugging(this, this.sixthArgument);
				}
				#endif

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				#endif

				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			}
			// Handle arguments first creation method, which supports constructor injection.
			else
			{
				var firstArgument = await this.firstArgument.GetValueAsync(this, Context.MainThread);
				var secondArgument = await this.secondArgument.GetValueAsync(this, Context.MainThread);
				var thirdArgument = await this.thirdArgument.GetValueAsync(this, Context.MainThread);
				var fourthArgument = await this.fourthArgument.GetValueAsync(this, Context.MainThread);
				var fifthArgument = await this.fifthArgument.GetValueAsync(this, Context.MainThread);
				var sixthArgument = await this.sixthArgument.GetValueAsync(this, Context.MainThread);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(disposeArgumentsOnDestroy != Arguments.None)
				{
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.First)) OptimizeValueProviderNameForDebugging(this, this.firstArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Second)) OptimizeValueProviderNameForDebugging(this, this.secondArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Third)) OptimizeValueProviderNameForDebugging(this, this.thirdArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fourth)) OptimizeValueProviderNameForDebugging(this, this.fourthArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Fifth)) OptimizeValueProviderNameForDebugging(this, this.fifthArgument);
					if(disposeArgumentsOnDestroy.HasFlag(Arguments.Sixth)) OptimizeValueProviderNameForDebugging(this, this.sixthArgument);
				}
				#endif

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				#endif

				wrappedObject = CreateWrappedObject(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				wrapper = InitWrapper(wrappedObject);
			}
			
			return wrapper;
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
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Fifth, ref fifthArgument);
			HandleDisposeValue(this, disposeArgumentsOnDestroy, Arguments.Sixth, ref sixthArgument);
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

		private protected override NullGuardResult EvaluateNullGuard() => firstArgument.EvaluateNullGuard(this)
																 .Join(secondArgument.EvaluateNullGuard(this))
																 .Join(thirdArgument.EvaluateNullGuard(this))
																 .Join(fourthArgument.EvaluateNullGuard(this))
																 .Join(fifthArgument.EvaluateNullGuard(this))
																 .Join(sixthArgument.EvaluateNullGuard(this));

		private protected override void OnValidate() => Validate(this, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
		#endif
	}
}