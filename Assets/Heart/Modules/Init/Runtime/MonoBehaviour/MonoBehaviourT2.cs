﻿using System;
using System.Diagnostics;
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
	/// <see cref="InstantiateExtensions.Instantiate{TComponent, TFirstArgument, TSecondArgument}">instantiated</see>
	/// or <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}">added</see>
	/// to a <see cref="GameObject"/> with two arguments passed to the <see cref="Init"/> function of the created instance.
	/// <para>
	/// If the object depends exclusively on classes that have the <see cref="ServiceAttribute"/> then
	/// it will receive them in its <see cref="Init"/> function automatically during initialization.
	/// </para>
	/// <para>
	/// If the component is part of a scene or a prefab, add depends on any classes that don't have the <see cref="ServiceAttribute"/>,
	/// then an <see cref="Initializer{TComponent, TFirstArgument, TSecondArgument}"/>
	/// can be used to specify its initialization arguments.
	/// </para>
	/// <para>
	/// Instances of classes inheriting from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> receive the arguments
	/// via the <see cref="Init"/> method where they can be assigned to member fields or properties.
	/// </para>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> function. </typeparam>
	public abstract class MonoBehaviour<TFirstArgument, TSecondArgument> : InitializableBaseInternal, IInitializable<TFirstArgument, TSecondArgument>
	{
		/// <summary>
		/// Provides the <see cref="Component"/> with the objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the component.
		/// </para>
		/// <para>
		/// <see cref="Init"/> get called when the script is being loaded, before the <see cref="OnAwake"/>, OnEnable and Start events when
		/// the component is created using <see cref="InstantiateExtensions.Instantiate{TFirstArgument, TSecondArgument}"/> or
		/// <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}"/>.
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
		/// <param name="firstArgument"> The first object that this component depends on. </param>
		/// <param name="secondArgument"> The second object that this component depends on. </param>
		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument);

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
		protected object this[[DisallowNull] string memberName]
		{
			set
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(initState is InitState.Initialized) throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
				#endif

				Inject<MonoBehaviour<TFirstArgument, TSecondArgument>, TFirstArgument, TSecondArgument>(this, memberName, value);
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
			Init(Context.Reset);
			OnInitializableReset(this);
			OnReset();
		}
		#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected override bool Init(Context context)
		{
			if(initState != InitState.Uninitialized)
			{
				return true;
			}

			if(!InitArgs.TryGet(context, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument))
			{
				return false;
			}

			initState = InitState.Initializing;

			ValidateArgumentsIfPlayMode(firstArgument, secondArgument, context);

			Init(firstArgument, secondArgument);

			initState = InitState.Initialized;

			return true;
		}

		/// <inheritdoc/>
		void IInitializable<TFirstArgument, TSecondArgument>.Init(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			initState = InitState.Initializing;
			ValidateArgumentsIfPlayMode(firstArgument, secondArgument, Context.MainThread);

			Init(firstArgument, secondArgument);

			initState = InitState.Initialized;
		}

		internal void InitInternal(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			initState = InitState.Initializing;
			ValidateArgumentsIfPlayMode(firstArgument, secondArgument, Context.MainThread);

			Init(firstArgument, secondArgument);

			initState = InitState.Initialized;
		}

		/// <summary>
		/// Method that can be overridden and used to validate the initialization arguments that were received by this object.
		/// <para>
		/// You can use the <see cref="ThrowIfNull"/> method to throw an <see cref="ArgumentNullException"/>
		/// if an argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArguments(IInputManager inputManager, Camera camera)
		/// {
		///		ThrowIfNull(inputManager);
		///		ThrowIfNull(camera);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// You can use the <see cref="AssertNotNull"/> method to log an assertion to the Console
		/// if an argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArguments(IInputManager inputManager, Camera camera)
		/// {
		///		AssertNotNull(inputManager);
		///		AssertNotNull(camera);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// Calls to this method are ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> The first received argument to validate. </param>
		/// <param name="secondArgument"> The second received argument to validate. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ValidateArguments(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			AssertNotNull(firstArgument);
			AssertNotNull(secondArgument);
			#endif
		}

		[Conditional("UNITY_EDITOR"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		#if UNITY_EDITOR
		async
		#endif
		private void ValidateArgumentsIfPlayMode(TFirstArgument firstArgument, TSecondArgument secondArgument, Context context)
		{
			#if UNITY_EDITOR
			if(context.TryDetermineIsEditMode(out bool editMode))
			{
				if(editMode)
				{
					return;
				}

				if(!context.IsUnitySafeContext())
				{
					await Until.UnitySafeContext();
				}
			}
			else
			{
				await Until.UnitySafeContext();

				if(!Application.isPlaying)
				{
					return;
				}
			}

			if(ShouldSelfGuardAgainstNull(this))
			{
				ValidateArguments(firstArgument, secondArgument);
			}
			#endif
		}
	}
}