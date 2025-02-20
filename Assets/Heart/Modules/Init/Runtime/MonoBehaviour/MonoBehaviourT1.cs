using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.Init.Reflection.InjectionUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for <see cref="MonoBehaviour">MonoBehaviours</see> that can be
	/// <see cref="InstantiateExtensions.Instantiate{TComponent, TArgument}">instantiated</see>
	/// or <see cref="AddComponentExtensions.AddComponent{TComponent, TArgument}">added</see>
	/// to a <see cref="GameObject"/> with an argument passed to the <see cref="Init"/> method of the created instance.
	/// <para>
	/// If the object depends on an object that has been registered as service using the <see cref="ServiceAttribute"/>, then
	/// it will be able to receive the service in its <see cref="Init"/> method automatically during its initialization.
	/// </para>
	/// <para>
	/// If the component is part of a scene or a prefab, and depends on a class that doesn't have the <see cref="ServiceAttribute"/>,
	/// then an <see cref="Initializer{TComponent, TArgument}"/> can be used to specify its initialization argument.
	/// </para>
	/// <para>
	/// Instances of classes inheriting from <see cref="MonoBehaviour{TArgument}"/> receive the argument
	/// via the <see cref="Init"/> method where it can be assigned to a member field or property.
	/// </para>
	/// </summary>
	/// <typeparam name="TArgument"> Type of the argument received in the <see cref="Init"/> method. </typeparam>
	public abstract class MonoBehaviour<TArgument> : MonoBehaviourBase, IInitializable<TArgument>
	{
		/// <summary>
		/// Provides the <see cref="Component"/> with the <paramref name="argument">object</paramref> that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for the component.
		/// </para>
		/// <para>
		/// <see cref="Init"/> get called when the script is being loaded, before the <see cref="OnAwake"/>, OnEnable and Start events when
		/// the component is created using <see cref="InstantiateExtensions.Instantiate{TArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// In edit mode <see cref="Init"/> can also get called during the <see cref="Reset"/> event when the script is added to a <see cref="GameObject"/>,
		/// if the component has the <see cref="InitOnResetAttribute"/> or <see cref="RequireComponent"/> attribute for each argument.
		/// </para>
		/// <para>
		/// Note that <see cref="Init"/> gets called immediately following object creation even if the <see cref="GameObject"/> to which the component
		/// was added is <see cref="GameObject.activeInHierarchy">inactive</see> unlike some other initialization event functions such as Awake and OnEnable.
		/// </para>
		/// </summary>
		/// <param name="argument"> Object that this component depends on. </param>
		protected abstract void Init(TArgument argument);

		/// <summary>
		/// Assigns an argument received during initialization to a field or property by the <paramref name="memberName">given name</paramref>.
		/// <para>
		/// Because reflection is used to set the value it is possible to use this to assign to init only fields and properties.
		/// Properties that do not have a set accessor and are not auto-implemented are not supported however.
		/// </para>
		/// </summary>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <exception cref="InvalidOperationException">
		/// Thrown if this method is called outside of the context of the client object being <see cref="Init">initialized</see>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the provided <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found or if property by given name is not auto-implemented
		/// and does not have a set accessor.
		/// </exception>
		protected TArgument this[[DisallowNull] string memberName]
		{
			set
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(initState is InitState.Initialized) throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
				#endif

				Inject(this, memberName, value);
			}
		}

		/// <summary>
		/// Reset state to default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user selects Reset in the Inspector context menu or when adding the component the first time.
		/// </para>
		/// <para>
		/// This function is only called in edit mode.
		/// </para>
		/// </summary>
		protected virtual void OnReset() { }

		#if UNITY_EDITOR
		private protected void Reset()
		{
			InitInternal(Context.Reset);
			OnInitializableReset(this);
			OnReset();
		}
		#endif

		/// <summary>
		/// Requests the object to try and acquire the object that it depends on and initialize itself.
		/// </summary>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate the dependency and initialize itself, or has already
		/// successfully initialized itself previously; otherwise, <see langword="false"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool Init(Context context)
		{
			if(!InitArgs.TryGet(context, this, out TArgument argument))
			{
				return false;
			}
			
			HandleValidate(context, argument);
			Init(argument);
			return true;
		}

		/// <inheritdoc/>
		void IInitializable<TArgument>.Init(TArgument argument)
		{
			initState = InitState.Initializing;
			HandleValidate(Context.MainThread, argument);

			Init(argument);

			initState = InitState.Initialized;
		}

		/// <summary>
		/// Identical to <see cref="Init(TArgument)"/>, but non-virtual, so slightly faster.
		/// </summary>
		internal void InitInternal(TArgument argument)
		{
			initState = InitState.Initializing;
			HandleValidate(Context.MainThread, argument);

			Init(argument);

			initState = InitState.Initialized;
		}

		void IArgs<TArgument>.Validate(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			ValidateArgument(argument);
			#endif
		}
	}
}