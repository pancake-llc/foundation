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
	/// A base class for a component that can specify the five constructor arguments used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,,,,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,,,,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : WrapperInitializerBaseInternal<TWrapper, TWrapped>
		, IInitializer<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, IInitializable
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		#endif
		where TWrapper : Wrapper<TWrapped>
	{
		/// <summary>
		/// The first argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TFirstArgument FirstArgument { get; set; }

		/// <summary>
		/// The second argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TSecondArgument SecondArgument { get; set; }

		/// <summary>
		/// The third argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TThirdArgument ThirdArgument { get; set; }

		/// <summary>
		/// The fourth argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TFourthArgument FourthArgument { get; set; }

		/// <summary>
		/// The fifth argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TFifthArgument FifthArgument { get; set; }

		/// <inheritdoc/>
		private protected override TWrapper InitTarget([AllowNull] TWrapper wrapper)
		{
			// Handle instance first creation method, which supports cyclical dependencies (A requires B, and B requires A).
			if(wrapper is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable
				&& GetOrCreateUninitializedWrappedObject() is var wrappedObject)
			{
				wrapper = InitWrapper(wrappedObject);

				var firstArgument = FirstArgument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref firstArgument);
				var secondArgument = SecondArgument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref secondArgument);
				var thirdArgument = ThirdArgument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref thirdArgument);
				var fourthArgument = FourthArgument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref fourthArgument);
				var fifthArgument = FifthArgument;
				OnAfterUninitializedWrappedObjectArgumentRetrieved(this, ref fifthArgument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				#endif

				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			}
			// Handle arguments first creation method, which supports constructor injection.
			else
			{
				var firstArgument = FirstArgument;
				var secondArgument = SecondArgument;
				var thirdArgument = ThirdArgument;
				var fourthArgument = FourthArgument;
				var fifthArgument = FifthArgument;

				#if DEBUG || INIT_ARGS_SAFE_MODE
				ValidateArgumentsAtRuntime(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				#endif

				wrappedObject = CreateWrappedObject(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
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
				AutoInitInEditMode<WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this);
			}
			#endif

			InitTarget();
			return true;
		}

		protected virtual void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument) { }

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the provided arguments and returns it.
		/// <para>
		/// Note: If you need support circular dependencies between your objects then you need to also override
		/// <see cref="GetOrCreateUninitializedWrappedObject()"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> The first argument used to initialize the wrapped object. </param>
		/// <param name="secondArgument"> The second argument used to initialize the wrapped object. </param>
		/// <param name="thirdArgument"> The third argument used to initialize the wrapped object. </param>
		/// <param name="fourthArgument"> The fourth argument used to initialize the wrapped object. </param>
		/// <param name="fifthArgument"> The fifth argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[return: NotNull]
		protected abstract TWrapped CreateWrappedObject(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);

		private protected sealed override TWrapped CreateWrappedObject() => CreateWrappedObject(FirstArgument, SecondArgument, ThirdArgument, FourthArgument, FifthArgument);

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentsAtRuntime(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			ThrowIfMissing(firstArgument); ThrowIfMissing(secondArgument); ThrowIfMissing(thirdArgument); ThrowIfMissing(fourthArgument); ThrowIfMissing(fifthArgument);
		}
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() => IsNull(FirstArgument) || IsNull(SecondArgument) || IsNull(ThirdArgument) || IsNull(FourthArgument) || IsNull(FifthArgument) ? NullGuardResult.ValueMissing : NullGuardResult.Passed;
		TFirstArgument IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.FirstArgument { get => FirstArgument; set => FirstArgument = value; }
		TSecondArgument IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.SecondArgument { get => SecondArgument; set => SecondArgument = value; }
		TThirdArgument IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.ThirdArgument { get => ThirdArgument; set => ThirdArgument = value; }
		TFourthArgument IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.FourthArgument { get => FourthArgument; set => FourthArgument = value; }
		TFifthArgument IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.FifthArgument { get => FifthArgument; set => FifthArgument = value; }
		void IInitializerEditorOnly<TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument) => OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument);
		private protected sealed override void Reset() => Reset<WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, gameObject);
		private protected override void OnValidate() => Validate(this, gameObject, FirstArgument, SecondArgument, ThirdArgument, FourthArgument, FifthArgument);
		#endif
	}
}