using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using static Pancake.Init.NullExtensions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.Linq;
using Pancake.Init.EditorOnly;
using UnityEditor;
using UnityEditorInternal;
using static Pancake.Init.ServiceUtility;
using static Pancake.Init.EditorOnly.AutoInitUtility;

#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

#endif

namespace Pancake.Init.Internal
{
	internal static class InitializerUtility
	{
		internal const string InitArgumentMetadataClassName = "Init";

		internal const string TargetTooltip = "Existing target instance to initialize.\n\n" +
		"If value is null then the argument is passed to a new instance that is attached to the " +
		"same GameObject that this initializer is attached to.\n\n" +
		"If value is a prefab then the argument is injected to a new instance of that prefab.";

		internal const string NullArgumentGuardTooltip =
			"Specifies how this object should guard against the argument being null.\n\n" +
				"None:\nAllow the argument to be null in edit mode and to be passed as null at runtime.\n\n" +
				"Edit Mode Warning:\nWarn about the arguments being null in edit mode.\n" +
					"Note that validation in edit mode might not give accurate results if the argument is a service that only becomes available at runtime.\n\n" +
				"Runtime Exception:\nThrow an exception if the argument is null at runtime when it should be injected to the client.\n\n" +
				"Enabled For Prefabs:\nWarn about null arguments detected on prefab assets.\n" +
					"Note that this option only affects prefab assets; prefab instances in scenes will still give warnings about null arguments in Edit Mode if the 'Edit Mode Warning' flag is enabled.";

		internal const string NullGuardFailedDefaultMessage = "A missing argument was detected.\n\nIf the argument is allowed to be null set the Null Argument Guard to None.\n\nIf the argument is a service that only becomes available at runtime configure the Null Argument Guard to Runtime Exception only.";

		private static readonly List<IInitializer> reusableInitializerList = new List<IInitializer>();

		internal static MissingInitArgumentsException GetMissingInitArgumentsException(Type initializerType, Type clientType, Type argumentType)
		{
			foreach(var serviceAttribute in argumentType.GetCustomAttributes<ServiceAttribute>())
			{
				var definingType = serviceAttribute.definingType;
				if(definingType != null)
				{
					if(!definingType.IsAssignableFrom(argumentType))
					{
						if(typeof(IValueProvider<>).MakeGenericType(definingType).IsAssignableFrom(argumentType))
						{
							return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute and implements IValueProvider<{definingType.Name}> but the instance was still null.");
						}

						if(definingType.IsInterface)
						{
							return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not implement {definingType.Name}.");
						}

						return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not derive from {definingType.Name}.");
					}

					return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but its instance was still null.");
				}

				return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service] attribute but its instance was still null.");
			}

			string classThatDerivesFromImplementsOrIs = !argumentType.IsAbstract && !TypeUtility.IsBaseType(argumentType) ? "the class" : argumentType.IsInterface ? "a class that implements" : "a class that derives from";

			if(!typeof(Component).IsAssignableFrom(argumentType))
			{
				return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a value using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {classThatDerivesFromImplementsOrIs} {argumentType.Name}.\nIf you have already done one of these things, initialization could be failing due to circular dependencies ({clientType.Name} initialization depends on {argumentType.Name} existing and {argumentType.Name} initialization depends on {clientType.Name} existing.");
			}

			return new MissingInitArgumentsException($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a reference using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {classThatDerivesFromImplementsOrIs} {argumentType.Name}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAfterUnitializedWrappedObjectArgumentRetrieved<TArgument>(Component wrapperInitializer, ref TArgument argument)
		{
			if(argument == Null && wrapperInitializer.TryGetComponent(out IInitializer<TArgument> argumentInitializer) && argumentInitializer as Component != wrapperInitializer)
			{
				argument = argumentInitializer.InitTarget();
			}
		}

		#if UNITY_EDITOR
		internal static void ResetArgument<TArgument, TInitializer>(ref TArgument argument, int argumentIndex, bool canAutoInitArguments, [NotNull] TInitializer initializer) where TInitializer : MonoBehaviour
		{
			if(IsServiceDefiningType<TArgument>())
			{
				argument = default;
				return;
			}

			if(canAutoInitArguments)
            {
				argument = GetAutoInitArgument<TInitializer, TArgument>(initializer, argumentIndex);

				if(Service.ForEquals(initializer.gameObject, argument))
				{
					argument = default;
					return;
				}

				return;
			}

			if(typeof(TArgument).IsValueType)
            {
				argument = default;
				return;
            }

			if(typeof(Object).IsAssignableFrom(typeof(TArgument)) || typeof(Type).IsAssignableFrom(typeof(TArgument)))
			{
				return;
			}

			if(!typeof(TArgument).IsAbstract)
			{
				argument = CreateInstance<TArgument>();
				return;
			}

			Type onlyConcreteType = null;
			foreach(Type derivedType in TypeUtility.GetDerivedTypes<TArgument>())
			{
				if(derivedType.IsAbstract)
                {
					continue;
                }

				if(onlyConcreteType != null)
                {
					return;
                }

				onlyConcreteType = derivedType;
			}

			if(onlyConcreteType is null || typeof(Object).IsAssignableFrom(onlyConcreteType) || typeof(Type).IsAssignableFrom(onlyConcreteType))
            {
				return;
            }

			argument = CreateInstance<TArgument>(onlyConcreteType);
		}

		internal static bool AreEqual<TArgument>(TArgument x, TArgument y) => x == Null ? y == Null : y != Null && x.Equals(y);

		internal static TArgument CreateInstance<TArgument>() => CreateInstance<TArgument>(typeof(TArgument));

		internal static TArgument CreateInstance<TArgument>(Type instanceType)
        {
			if(typeof(Type).IsAssignableFrom(instanceType))
            {
				#if DEV_MODE
				Debug.LogWarning($"Can not create instance of a {instanceType.FullName} using FormatterServices.GetUninitializedObject.");
				#endif
				return default;
            }

			try
			{
				return (TArgument)Activator.CreateInstance(instanceType);
			}
			catch(Exception)
			{
				return (TArgument)FormatterServices.GetUninitializedObject(instanceType);
			}
        }

		internal static void ResetInitializer<TClient>([NotNull] Component initializer, ref TClient client) where TClient : Component
        {
            client = FindComponentWithoutInitializer<TClient>(initializer.gameObject);

            if(client == null || client.gameObject != initializer.gameObject)
            {
                return;
            }

			// Components on prefab instances can't be reordered
			if(!PrefabUtility.IsPartOfPrefabInstance(initializer))
			{
				MoveInitializerAboveItsClient(initializer, client);
			}
        }

		private static TComponent FindComponentWithoutInitializer<TComponent>(GameObject gameObject) where TComponent : Component
		{
			var options = gameObject.GetComponents<TComponent>();
			int count = options.Length;			
			if(count > 0)
			{
				var initializers = gameObject.GetComponents<IInitializer>();
				foreach(var option in options)
				{
					if(!initializers.Any(i => i.Target == option))
					{
						return option;
					}
				}

				return null;
			}

			return Find.In<TComponent>(gameObject, Including.Children | Including.Parents | Including.Scene | Including.Inactive);
		}

        private static void MoveInitializerAboveItsClient([NotNull] Component initializer, [NotNull] Component target)
        {
			var moveComponentRelativeToComponent = typeof(ComponentUtility).GetMethod("MoveComponentRelativeToComponent", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Component), typeof(Component), typeof(bool) }, null);
			if(moveComponentRelativeToComponent != null)
			{
				moveComponentRelativeToComponent.Invoke(null, new object[] { initializer, target, true });
			}
			else
			{
				while(ComponentUtility.MoveComponentUp(initializer));
			}
		}

		internal static bool Validate<TArgument, TInitializer>(TInitializer initializer, Any<TArgument> argument) where TInitializer : Component, IInitializerEditorOnly
		{
			if(IsNull(initializer, argument) || IsServiceDefiningType<TArgument>() || Services.TryGetFor<TArgument>(initializer.gameObject, out _))
			{
				return true;
			}

			if(Array.IndexOf(Selection.gameObjects, initializer.gameObject) == -1 && initializer.gameObject.activeInHierarchy)
			{
				string message = $"{initializer.GetType().Name} on GameObject \"{initializer.name}\" is missing argument of type {typeof(TArgument).Name}.\nIf the argument is allowed to be null set the 'Null Argument Guard' option to 'None'.\nIf the argument is a service that only becomes available at runtime set the option to 'Runtime Exception'.";
				Debug.LogWarning(message, initializer);
				initializer.NullGuardFailedMessage = message;
			}
			else if(string.IsNullOrEmpty(initializer.NullGuardFailedMessage))
			{
				initializer.NullGuardFailedMessage = NullGuardFailedDefaultMessage;
			}

			return false;
		}

		internal static void Validate(GameObject gameObject, IInitializerEditorOnly initializer)
		{
			if(initializer.MultipleInitializersPerTargetAllowed)
			{
				return;
			}

			if(gameObject == null)
			{
				return;
			}

			if(initializer is Object obj && obj == null)
			{
				return;
			}

			var client = initializer.Target;
			if(client == null)
			{
				return;
			}

			var initializers = reusableInitializerList;
			gameObject.GetComponents(initializers);
			int count = initializers.Count;
			if(initializers.Count <= 1)
			{
				initializers.Clear();
				return;
			}

			for(int i = 0; i < count; i++)
			{
				var someInitializer = initializers[i];
				if(someInitializer == initializer)
				{
					continue;
				}

				var someInitializerClient = someInitializer.Target;
				if(someInitializerClient == client)
				{
					initializers.Clear();
					Debug.LogWarning($"Only one Initializer per object is supported but {client.GetType().Name} on '{client.name}' has multiple Initializers targeting it. Clearing Target of surplus Initializer.", initializer as Object);
					initializer.Target = null;
					EditorUtility.SetDirty(client);
					return;
				}
			}

			initializers.Clear();
		}

		/// <summary>
		/// Gets a value indicating whether or not the Init argument of type <typeparamref name="TArgument"/>
		/// will have a value in play mode.
		/// <para>
		/// This can only be called from the main thread.
		/// </para>
		/// </summary>
		/// <typeparam name="TArgument"> Type of the argument. </typeparam>
		/// <param name="argument"> Current value of the argument. </param>
		/// <returns> <see langword="true"/> if argument is missing; otherwise, <see langword="false"/>. </returns>
		internal static bool IsNull<TArgument>(TArgument argument) => argument == Null && (Application.isPlaying || !IsServiceDefiningType<TArgument>());

		internal static bool IsNull<TArgument>(Component initializer, Any<TArgument> argument) => !argument.GetHasValue(initializer, Context.MainThread);

		internal static void UpdateHideFlags(Component initializer, Component client)
		{
			var setHideFlags = client == null || client.gameObject != initializer.gameObject ? HideFlags.None : HideFlags.HideInInspector;
			if(setHideFlags != initializer.hideFlags)
			{
				Undo.RecordObject(initializer, "Update HideFlags");
				initializer.hideFlags = setHideFlags;
			}
		}

		/// <summary>
		/// Intializes the <paramref name="target"/> <typeparamref name="TWrapper"/> with the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> that contains the initializer component. </param>
		/// <param name="target">
		/// The existing <typeparamref name="TWrapper"/> scene instance to initialize,
		/// The prefab to clone to create the <typeparamref name="TWrapper"/> instance,
		/// or <see langword="null"/> if a new instance should be created from scratch..
		/// </param>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <see cref="target"/> <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TWrapper"/>. </returns>
		[NotNull, MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static TWrapper InitWrapper<TWrapper, TWrapped>(GameObject gameObject, TWrapper target, TWrapped wrappedObject)
			where TWrapper : MonoBehaviour, IWrapper<TWrapped>
        {
            if(target == null)
            {
				return gameObject.AddComponent<TWrapper, TWrapped>(wrappedObject);
            }

            if(!target.gameObject.scene.IsValid())
            {
				return target.Instantiate(wrappedObject);
            }

            target.Init(wrappedObject);
			return target;
		}

		internal static bool CanArgumentsBeNullInEditMode<TInitializer>(TInitializer initializer) where TInitializer : MonoBehaviour, IInitializerEditorOnly
			=> CanArgumentsBeNullInEditMode(initializer.gameObject, initializer.NullArgumentGuard);

		internal static bool CanArgumentsBeNullInEditMode(GameObject gameObject, NullArgumentGuard nullArgumentGuard)
				=> nullArgumentGuard.IsDisabled(NullArgumentGuard.EditModeWarning)
				|| (nullArgumentGuard.IsDisabled(NullArgumentGuard.EnabledForPrefabs)
				&& (PrefabUtility.IsPartOfPrefabAsset(gameObject) || PrefabStageUtility.GetPrefabStage(gameObject) != null));

		internal static void ValidateTargetOnMainThread<TStateMachineBehaviourInitializer, TStateMachineBehaviour>(TStateMachineBehaviourInitializer initializer)
				where TStateMachineBehaviourInitializer : MonoBehaviour, IInitializer, IValueProvider<TStateMachineBehaviour>, IInitializerEditorOnly
				where TStateMachineBehaviour : StateMachineBehaviour
		{
			var target = initializer.Target as Animator;
			if(target != null && target.GetBehaviour<TStateMachineBehaviour>() == null)
			{
				Debug.LogWarning($"AnimatorController of {target.GetType().Name} on '{target.name}' no longer contains a StateMachineBehaviour of type {typeof(TStateMachineBehaviour).Name}. Clearing Target of Initializer.", target);
				initializer.Target = null;
				EditorUtility.SetDirty(initializer);
			}
		}

		internal static void HandleValidate<TInitializer>(TInitializer initializer) where TInitializer : MonoBehaviour, IInitializerEditorOnly
			=> OnMainThread(() => ValidateOnMainThread(initializer));

		internal static void OnMainThread(EditorApplication.CallbackFunction action) => EditorApplication.delayCall += action;

		internal static void ValidateOnMainThread<TInitializer>(TInitializer initializer) where TInitializer : MonoBehaviour, IInitializerEditorOnly
		{
			if(initializer == null)
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate(initializer.gameObject, initializer);
			#endif

			if(initializer.Target is Component targetComponent)
			{
				UpdateHideFlags(initializer, targetComponent);
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			if(CanArgumentsBeNullInEditMode(initializer) || !initializer.HasNullArguments)
			{
				initializer.NullGuardFailedMessage = "";
				return;
			}

			if(Array.IndexOf(Selection.gameObjects, initializer.gameObject) == -1 && initializer.gameObject.activeInHierarchy)
			{
				string message = $"{GetClientComponentTypeName(initializer)} component on GameObject \"{initializer.name}\" contains missing Init arguments.\nIf missing arguments should be allowed set the 'Null Argument Guard' option to 'None'.\nIf the argument is a service that only becomes available at runtime set the option to 'Runtime Exception'.";
				Debug.LogWarning(message, initializer);
				initializer.NullGuardFailedMessage = message;
				return;
			}
			
			if(string.IsNullOrEmpty(initializer.NullGuardFailedMessage))
			{
				initializer.NullGuardFailedMessage = NullGuardFailedDefaultMessage;
			}

			static string GetClientComponentTypeName(Component initializer)
			{
				Type initializerType = initializer.GetType();
				Type baseType = initializerType.BaseType;
				if(baseType.IsGenericType)
				{
					Type clientType = baseType.GetGenericArguments()[0];
					if(typeof(MonoBehaviour).IsAssignableFrom(clientType))
					{
						return clientType.Name;
					}
				}

				string initializerTypeName = initializerType.Name;
				if(!initializerTypeName.EndsWith("Initializer"))
				{
					return initializerTypeName;
				}

				return initializerTypeName.Substring(0, initializerTypeName.Length - "Initializer".Length);
			}
			#endif
		}

		public delegate void OnResetHandler<TArgument>(ref TArgument argument);
		public delegate void OnResetHandler<TFirstArgument, TSecondArgument>(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument);
		public delegate void OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument>(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument);
		public delegate void OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument);
		public delegate void OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument);
		public delegate void OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument, ref TFifthArgument fifthArgument, ref TSixthArgument sixthArgument);

		internal static TArgument HandleReset<TInitializer, TClient, TArgument>(TInitializer initializer, ref TClient target, TArgument argument,
			OnResetHandler<TArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 1);

			ResetArgument(ref argument, 0, true, initializer);

			overridableOnReset(ref argument);

			return argument;
		}

		internal static (TFirstArgument firstArgument, TSecondArgument secondArgument) HandleReset<TInitializer, TClient, TFirstArgument, TSecondArgument>
			(TInitializer initializer, ref TClient target, TFirstArgument firstArgument, TSecondArgument secondArgument,
			OnResetHandler<TFirstArgument, TSecondArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 2);

			ResetArgument(ref firstArgument, 0, true, initializer);
			ResetArgument(ref secondArgument, 1, true, initializer);

			overridableOnReset(ref firstArgument, ref secondArgument);

			return (firstArgument, secondArgument);
		}

		internal static (TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
			HandleReset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(TInitializer initializer, ref TClient target, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 3);

			ResetArgument(ref firstArgument, 0, true, initializer);
			ResetArgument(ref secondArgument, 1, true, initializer);
			ResetArgument(ref thirdArgument, 2, true, initializer);

			overridableOnReset(ref firstArgument, ref secondArgument, ref thirdArgument);

			return (firstArgument, secondArgument, thirdArgument);
		}

		internal static (TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
			HandleReset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(TInitializer initializer, ref TClient target, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
			OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 4);

			ResetArgument(ref firstArgument, 0, true, initializer);
			ResetArgument(ref secondArgument, 1, true, initializer);
			ResetArgument(ref thirdArgument, 2, true, initializer);
			ResetArgument(ref fourthArgument, 3, true, initializer);

			overridableOnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument);

			return (firstArgument, secondArgument, thirdArgument, fourthArgument);
		}

		internal static (TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
			HandleReset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(TInitializer initializer, ref TClient target, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument,
			OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 5);

			ResetArgument(ref firstArgument, 0, true, initializer);
			ResetArgument(ref secondArgument, 1, true, initializer);
			ResetArgument(ref thirdArgument, 2, true, initializer);
			ResetArgument(ref fourthArgument, 3, true, initializer);
			ResetArgument(ref fifthArgument, 4, true, initializer);

			overridableOnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument);

			return (firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
		}

		internal static (TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
			HandleReset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(TInitializer initializer, ref TClient target, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			OnResetHandler<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> overridableOnReset) where TInitializer : MonoBehaviour, IInitializerEditorOnly where TClient : Component
		{
			ResetInitializer(initializer, ref target);
			PrepareArgumentsForAutoInit<TClient>(initializer, 6);

			ResetArgument(ref firstArgument, 0, true, initializer);
			ResetArgument(ref secondArgument, 1, true, initializer);
			ResetArgument(ref thirdArgument, 2, true, initializer);
			ResetArgument(ref fourthArgument, 3, true, initializer);
			ResetArgument(ref fifthArgument, 4, true, initializer);
			ResetArgument(ref sixthArgument, 5, true, initializer);

			overridableOnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument);

			return (firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
		}
		#endif
	}
}