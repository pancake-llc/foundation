#pragma warning disable CS0414

using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif

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
	/// <para>
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TArgument> : WrapperInitializerBaseInternal<TWrapper, TWrapped>
		, IInitializer<TWrapped, TArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TWrapped, TArgument>
		#endif
		where TWrapper : Wrapper<TWrapped>
	{
		/// <summary>
		/// The argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		/// <inheritdoc/>
		private protected override TWrapper InitTarget([AllowNull] TWrapper wrapper)
		{
			// Handle instance first creation method, which supports cyclical dependencies (A requires B, and B requires A).
			if(wrapper is IInitializable<TArgument> initializable
				&& GetOrCreateUninitializedWrappedObject() is var wrappedObject)
			{
				wrapper = InitWrapper(wrappedObject);

				var argument = Argument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref argument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
				#endif

				initializable.Init(argument);
			}
			// Handle argument first creation method, which supports constructor injection.
			else
			{
				var argument = Argument;

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(IsRuntimeNullGuardActive) ValidateArgumentAtRuntime(argument);
				#endif

				wrappedObject = CreateWrappedObject(argument);
				wrapper = InitWrapper(wrappedObject);
			}
			
			return wrapper;
		}

		bool IInitializable.HasInitializer => false;
		
		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				AutoInitInEditMode<WrapperInitializerBase<TWrapper, TWrapped, TArgument>, TWrapped, TArgument>(this);
			}
			#endif

			InitTarget();
			return true;
		}

		protected virtual void OnReset(ref TArgument argument) { }

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the provided argument and returns it.
		/// <para>
		/// Note: If you need support circular dependencies between your objects then you need to also override
		/// <see cref="GetOrCreateUninitializedWrappedObject()"/>.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[return: NotNull]
		protected abstract TWrapped CreateWrappedObject(TArgument argument);

		private protected sealed override TWrapped CreateWrappedObject() => CreateWrappedObject(Argument);

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentAtRuntime(TArgument argument) => ThrowIfMissing(argument);
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() => IsNull(Argument) ? NullGuardResult.ValueMissing : NullGuardResult.Passed;
		TArgument IInitializerEditorOnly<TWrapped, TArgument>.Argument { get => Argument; set => Argument = value; }
		void IInitializerEditorOnly<TWrapped, TArgument>.OnReset(ref TArgument argument) => OnReset(ref argument);
		private protected sealed override void Reset() => Reset<WrapperInitializerBase<TWrapper, TWrapped, TArgument>, TWrapped, TArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, Argument);
		#endif
	}
}