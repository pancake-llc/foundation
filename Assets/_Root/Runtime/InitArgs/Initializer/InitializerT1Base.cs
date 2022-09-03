#pragma warning disable CS0414

using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
using static Pancake.Init.Internal.InitializerUtility;
using static Pancake.NullExtensions;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the argument used to
	/// initialize an object that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument gets injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="Initializer{,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="InitializerBase{,}"/>
	/// you are responsible for implementing the Argument property and serializing its value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="Initializer{,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client component's Init function. </typeparam>
	public abstract class InitializerBase<TClient, TArgument> : MonoBehaviour, IInitializer, IValueProvider<TClient>
		#if UNITY_EDITOR
		, IInitializerEditorOnly
		#endif
		where TClient : MonoBehaviour, IInitializable<TArgument>
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TClient target = null;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private NullArgumentGuard nullArgumentGuard = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException;

		/// <inheritdoc/>
		TClient IValueProvider<TClient>.Value => target;

		/// <inheritdoc/>
		object IValueProvider.Value => target;

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = (TClient)value; }

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TClient));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <summary>
		/// The argument passed to the <typeparamref name="TClient">client</typeparamref>'s Init function.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		#if UNITY_EDITOR
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(Argument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		#endif

		/// <summary>
		/// Resets the Init <paramref name="argument"/> to its default value.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </summary>
		/// <param name="argument"> The argument to reset. </param>
		protected virtual void OnReset(ref TArgument argument) { }

		/// <summary>
		/// Initializes the existing <see cref="target"/> or new Instance of type <see cref="TClient"/> using the provided argument.
		/// </summary>
		/// <param name="argument"> The argument to pass to the target's Init function. </param>
		/// <returns> The existing <see cref="target"/> or new Instance of type <see cref="TClient"/>. </returns>
		[NotNull]
		protected virtual TClient InitTarget(TArgument argument)
        {
            if(target == null)
            {
                return target = gameObject.AddComponent<TClient, TArgument>(argument);
            }

			if(target.gameObject != gameObject)
			{
                return target.Instantiate(argument);
            }
			
			target.Init(argument);
			return target;
        }

		#if UNITY_EDITOR
        private void Reset()
		{
			var set = HandleReset(this, ref target, Argument, OnReset);
			if(!AreEqual(Argument, set)) Argument = set;
		}

		private void OnValidate() => OnMainThread(Validate);
		#endif

		protected virtual void Validate()
		{
			#if UNITY_EDITOR
			ValidateOnMainThread(this);
			#endif
		}

		private void Awake() => InitTarget();

		private TClient InitTarget()
		{
			if(this == null)
			{
				return target;
			}

			var argument = Argument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
			{
				if(argument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TClient), typeof(TArgument));
			}
			#endif

			target = InitTarget(argument);
			Updater.InvokeAtEndOfFrame(DestroySelf);
			return target;
		}

		private void DestroySelf()
		{
			if(this != null)
			{
				Destroy(this);
			}
		}
	}
}