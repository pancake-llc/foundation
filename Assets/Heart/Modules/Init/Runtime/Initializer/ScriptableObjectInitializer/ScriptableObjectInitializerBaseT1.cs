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
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign them to member fields and properties.
	/// </para>
	/// <para>
	/// When you derive your initializer class from <see cref="ScriptableObjectInitializerBase{,}"/>
	/// you are responsible for implementing the argument properties and serializing their values.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="ScriptableObjectInitializer{,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized scriptable object. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client's Init function. </typeparam>
	public abstract class ScriptableObjectInitializerBase<TClient, TArgument> : ScriptableObjectInitializerBaseInternal<TClient>, IInitializer<TClient, TArgument>
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient, TArgument>
		#endif
		where TClient : ScriptableObject, IInitializable<TArgument>
	{
		/// <summary>
		/// The argument passed to the <typeparamref name="TClient">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		/// <inheritdoc/>
		[return: NotNull]
		private protected override TClient InitTarget([AllowNull] TClient target)
		{
			var argument = Argument;

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

		/// <summary>
		/// Resets the Init arguments to their default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the initializer to a scriptable object the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </summary>
		/// <param name="argument"> The argument to reset. </param>
		protected virtual void OnReset(ref TArgument argument) { }

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ValidateArgumentAtRuntime(TArgument argument) => ThrowIfMissing(argument);
		#endif

		#if UNITY_EDITOR
		private protected override NullGuardResult EvaluateNullGuard() => IsNull(Argument) ? NullGuardResult.ValueMissing : NullGuardResult.Passed;
		TArgument IInitializerEditorOnly<TClient, TArgument>.Argument { get => Argument; set => Argument = value; }
		void IInitializerEditorOnly<TClient, TArgument>.OnReset(ref TArgument argument) => OnReset(ref argument);
		private protected sealed override void Reset() => Reset<ScriptableObjectInitializerBase<TClient, TArgument>, TClient, TArgument>(this, null);
		private protected override void OnValidate() => Validate(this, null, Argument);
		#endif
	}
}