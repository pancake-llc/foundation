using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
using Sisus.Init.ValueProviders;
using UnityEditor;
using static Sisus.Init.EditorOnly.AutoInitUtility;
using static Sisus.Init.ServiceUtility;
using Debug = UnityEngine.Debug;

[assembly:InternalsVisibleTo("InitArgs.Odin")]
#endif

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Utility methods related to initializers.
	/// </summary>
	public static class InitializerUtility
	{
		internal const string InitArgumentMetadataClassName = "Init";
		internal const NullArgumentGuard DefaultNullArgumentGuardFlags = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException;

		internal const string TargetTooltip =
			"Existing target instance to initialize.\n\n" +

			"If value is null then the argument is passed to a new instance that is attached to the " +
			"same GameObject that this initializer is attached to.\n\n" +

			"If value is a prefab then the argument is injected to a new instance of that prefab.";

		internal const string NullArgumentGuardTooltip =
			"Specifies how this object should guard against the argument being null.\n\n" +
			
			"Edit Mode Warning:\nWarn about the arguments being null in edit mode.\n" +
			"Note that validation in edit mode might not give accurate results if the argument is a service that only becomes available at runtime.\n\n" +
			
			"Runtime Exception:\nThrow an exception if the argument is null at runtime when it should be injected to the client.";

		internal const string NullGuardFailedDefaultMessage = "A missing argument was detected.\n\nIf the argument is allowed to be null set the Null Argument Guard to None.\n\nIf the argument is a service that only becomes available at runtime configure the Null Argument Guard to Runtime Exception only.";

		internal static readonly Dictionary<Type, int> argumentCountsByIInitializerTypeDefinition = new(12)
		{
			{ typeof(IInitializer<,>), 1 },
			{ typeof(IInitializer<,,>), 2 },
			{ typeof(IInitializer<,,,>), 3 },
			{ typeof(IInitializer<,,,,>), 4 },
			{ typeof(IInitializer<,,,,,>), 5 },
			{ typeof(IInitializer<,,,,,,>), 6 },
			{ typeof(IInitializer<,,,,,,,>), 7 },
			{ typeof(IInitializer<,,,,,,,,>), 8 },
			{ typeof(IInitializer<,,,,,,,,,>), 9 },
			{ typeof(IInitializer<,,,,,,,,,,>), 10 },
			{ typeof(IInitializer<,,,,,,,,,,,>), 11 },
			{ typeof(IInitializer<,,,,,,,,,,,,>), 12 }
		};

		internal static readonly Dictionary<Type, int> argumentCountsByIServiceInitializerTypeDefinition = new(12)
		{
			{ typeof(IServiceInitializer<>), 0 },
			{ typeof(IServiceInitializer<,>), 1 },
			{ typeof(IServiceInitializer<,,>), 2 },
			{ typeof(IServiceInitializer<,,,>), 3 },
			{ typeof(IServiceInitializer<,,,,>), 4 },
			{ typeof(IServiceInitializer<,,,,,>), 5 },
			{ typeof(IServiceInitializer<,,,,,,>), 6 },
			{ typeof(IServiceInitializer<,,,,,,,>), 7 },
			{ typeof(IServiceInitializer<,,,,,,,,>), 8 },
			{ typeof(IServiceInitializer<,,,,,,,,,>), 9 },
			{ typeof(IServiceInitializer<,,,,,,,,,,>), 10 },
			{ typeof(IServiceInitializer<,,,,,,,,,,,>), 11 },
			{ typeof(IServiceInitializer<,,,,,,,,,,,,>), 12 },
			{ typeof(IServiceInitializerAsync<>), 0 },
			{ typeof(IServiceInitializerAsync<,>), 1 },
			{ typeof(IServiceInitializerAsync<,,>), 2 },
			{ typeof(IServiceInitializerAsync<,,,>), 3 },
			{ typeof(IServiceInitializerAsync<,,,,>), 4 },
			{ typeof(IServiceInitializerAsync<,,,,,>), 5 },
			{ typeof(IServiceInitializerAsync<,,,,,,>), 6 },
			{ typeof(IServiceInitializerAsync<,,,,,,,>), 7 },
			{ typeof(IServiceInitializerAsync<,,,,,,,,>), 8 },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,>), 9 },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,>), 10 },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,,>), 11 },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,,,>), 12 }
		};

		internal static readonly Dictionary<Type, int> argumentCountsByIXArgumentsTypeDefinition = new(12)
		{
			{ typeof(IOneArgument), 1 },
			{ typeof(ITwoArguments), 2 },
			{ typeof(IThreeArguments), 3 },
			{ typeof(IFourArguments), 4 },
			{ typeof(IFiveArguments), 5 },
			{ typeof(ISixArguments), 6 },
			{ typeof(ISevenArguments), 7 },
			{ typeof(IEightArguments), 8 },
			{ typeof(INineArguments), 9 },
			{ typeof(ITenArguments), 10 },
			{ typeof(IElevenArguments), 11 },
			{ typeof(ITwelveArguments), 12 }
		};

		private static Type[] typeArray = new Type[3];

		public static bool TryGetClientAndInitArgumentTypes(Type initializerType, out Type clientType, out Type[] initArgumentTypes)
		{
			foreach(var interfaceType in initializerType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					var genericArguments = interfaceType.GetGenericArguments();
					clientType = genericArguments[0];
					initArgumentTypes = genericArguments.Skip(1).ToArray();
					return true;
				}
			}

			clientType = null;
			initArgumentTypes = null;
			return false;
		}

		public static bool TryGetClientType(Type initializerType, out Type clientType)
		{
			foreach(var interfaceType in initializerType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					var genericArguments = interfaceType.GetGenericArguments();
					clientType = genericArguments[0];
					return true;
				}
			}

			clientType = null;
			return false;
		}

		public static bool TryGetInitArgumentTypes(Type initializerType, out Type[] initArgumentTypes)
		{
			foreach(var interfaceType in initializerType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					initArgumentTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
					return true;
				}
			}

			initArgumentTypes = null;
			return false;
		}

		/// <summary>
		/// Gets all concrete types that implement <see cref="IInitializer{TClient}"/> where TClient
		/// is the concrete type of the <see paramref="component"/>.
		/// <remarks>
		/// NOTE: Never cache or modify the list returned by this method!
		/// </remarks>
		/// </summary>
		public static Span<Type> GetInitializerTypes([DisallowNull] Component component)
		{
			var clientType = component.GetType();
			var requiredInterfaceType = typeof(IInitializer<>).MakeGenericType(clientType);
			int count = 0;

			#if UNITY_EDITOR
			foreach(var type in TypeCache.GetTypesDerivedFrom<IInitializer>())
			#else
			foreach(var type in TypeUtility.GetAllTypesThreadSafe(typeof(CustomInitializer<Component, object>).Assembly, false))
			#endif
			{
				if (!requiredInterfaceType.IsAssignableFrom(type) || type.IsAbstract)
				{
					continue;
				}

				if(typeArray.Length <= count)
				{
					Array.Resize(ref typeArray, count + count);
				}

				typeArray[count] = type;
				count++;
			}

			return typeArray.AsSpan(0, count);
		}

		public static bool HasInitializer([DisallowNull] object client)
		{
			if(client is Component component)
			{
				return HasInitializer(component);
			}

			if(client is IInitializable initializable)
			{
				return initializable.HasInitializer;
			}

			return false;
		}

		public static bool HasInitializer([DisallowNull] ScriptableObject client) => client is IInitializable initializable && initializable.HasInitializer;

		public static bool HasInitializer([DisallowNull] Component initializable)
		{
			if(!initializable)
			{
				return false;
			}

			foreach(var someInitializer in initializable.gameObject.GetComponentsNonAlloc<IInitializer>())
			{
				if(someInitializer.Target == initializable)
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasCustomInitArguments([DisallowNull] Component initializable)
		{
			if(!initializable)
			{
				return false;
			}

			foreach(var someInitializer in initializable.gameObject.GetComponentsNonAlloc<IInitializer>())
			{
				if(someInitializer.Target == initializable)
				{
					return someInitializer.ProvidesCustomInitArguments;
				}
			}

			return false;
		}

		public static bool TryGetInitializer([DisallowNull] Component initializable, out IInitializer initializer)
		{
			if(!initializable)
			{
				initializer = null;
				return false;
			}

			foreach(var someInitializer in initializable.gameObject.GetComponentsNonAlloc<IInitializer>())
			{
				if(someInitializer.Target == initializable)
				{
					initializer = someInitializer;
					return true;
				}
			}

			initializer = null;
			return false;
		}

		public static bool TryGetInitializer(Object initializable, out IInitializer initializer)
		{
			if(!initializable)
			{
				initializer = null;
				return false;
			}

			if(initializable is not Component component)
			{
				initializer = null;
				return false;
			}

			foreach(var someInitializer in component.gameObject.GetComponentsNonAlloc<IInitializer>())
			{
				if(someInitializer.Target == component)
				{
					initializer = someInitializer;
					return true;
				}
			}

			initializer = null;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void HandleDisposeValue<TArgument>([AllowNull] Component initializer, Arguments argumentsToDispose, Arguments flag, ref Any<TArgument> argument)
		{
			if(!argumentsToDispose.ContainsFlag(flag))
			{
				return;
			}

			if(argument.reference is IValueByTypeReleaser byTypeReleaser)
			{
				byTypeReleaser.Release(initializer, argument.value);
			}
			else if(argument.reference is IValueReleaser<TArgument> releaser)
			{
				releaser.Release(initializer, argument.value);
			}
			else if(argument.reference)
			{
				#if UNITY_EDITOR
				if(AssetDatabase.Contains(argument.reference))
				{
					#if DEV_MODE
					Debug.LogError("HandleDisposeValue target '{argument.reference}' was an asset ('{AssetDatabase.GetAssetPath(argument.reference)}').");
					#endif
					return;
				}
				#endif

				Object.Destroy(argument.reference);
			}
		}

		[Conditional("DEBUG")]
		public static void OptimizeValueProviderNameForDebugging<TInitializer, TArgument>([DisallowNull] TInitializer initializer, Any<TArgument> argument) where TInitializer : Object, IInitializer
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying || AssetDatabase.IsMainAsset(argument.reference) || AssetDatabase.IsSubAsset(argument.reference))
			{
				return;
			}
			#endif

			if(argument.reference && argument.reference.name.Length == 0)
			{
				argument.reference.name = typeof(TInitializer).Name + "." + typeof(TArgument).Name;
			}
		}

		public static bool ShouldSelfGuardAgainstNull([DisallowNull] object target)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(target is MonoBehaviour monoBehaviour)
			{
				return ShouldSelfGuardAgainstNull(monoBehaviour);
			}

			if(target is ScriptableObject scriptableObject)
			{
				return ShouldSelfGuardAgainstNull(scriptableObject);
			}

			return true;
			#else
			return false;
			#endif
		}
		
		/// <summary>
		/// Gets a value indicating whether the target itself should validate that initialization arguments it receives
		/// at runtime are not null?
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool ShouldSelfGuardAgainstNull([DisallowNull] MonoBehaviour target)
		{
			#if UNITY_EDITOR
			// If null argument guard has been disabled by the user, then target should not guard itself against null.
			if(!requestNullArgumentGuardFlags(target).IsEnabled(NullArgumentGuard.RuntimeException))
			{
				return false;
			}

			// If null argument guard has been enabled by the user, and target has no initializer attached,
			// then target should guard itself against null.
			if(!TryGetInitializer(target, out var initializer))
			{
				return true;
			}

			// If we can not determine whether initializer can guard against null, then we should play it safe
			// and target should guard itself against null.
			if(initializer is not IInitializerEditorOnly initializerEditorOnly)
			{
				return true;
			}

			// If initializer already guards the target against null, then there's no need for the target to do it.
			return !initializerEditorOnly.CanGuardAgainstNull;
			#else
			// In development builds we can not determine whether the user has disabled null argument guard for the
			// target, since that information is stored in the script's metadata, which is only accessible in the editor.
			// As such targets should never guard themselves against null at runtime. An initializer is needed for this.
			return false;
			#endif
		}

		public static bool ShouldSelfGuardAgainstNull([DisallowNull] ScriptableObject target)
		{
			#if UNITY_EDITOR
			// If null argument guard has been disabled by the user, then target should not guard itself against null.
			if(!requestNullArgumentGuardFlags(target).IsEnabled(NullArgumentGuard.RuntimeException))
			{
				return false;
			}

			// If null argument guard has been enabled by the user, and target has no initializer attached,
			// then target should guard itself against null.
			if(!TryGetInitializer(target, out var initializer))
			{
				return true;
			}

			// If we can not determine whether initializer can guard against null, then we should play it safe
			// and target should guard itself against null.
			if(initializer is not IInitializerEditorOnly initializerEditorOnly)
			{
				return true;
			}

			// If initializer already guards the target against null, then there's no need for the target to do it.
			return !initializerEditorOnly.CanGuardAgainstNull;
			#else
			// In development builds we can not determine whether the user has disabled null argument guard for the
			// target, since that information is stored in the script's metadata, which is only accessible in the editor.
			// As such targets should never guard themselves against null at runtime. An initializer is needed for this.
			return false;
			#endif
		}

		#if UNITY_EDITOR
		// Delegate is injected from an Editor-only assembly.
		internal static Func<Object, NullArgumentGuard> requestNullArgumentGuardFlags;
		private static readonly List<string> missingArgumentTypes = new(0);
		private static readonly List<Component> components = new();

		public static void Validate<TInitializer, TArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject, Any<TArgument> argument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, argument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument, Any<TFifthArgument> fifthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument, Any<TFifthArgument> fifthArgument, Any<TSixthArgument> sixthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument));

		public static void Validate<TInitializer, TArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject, TArgument argument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, argument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument));

		public static void Validate<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>([DisallowNull] TInitializer initializer, [AllowNull] GameObject gameObject,
			TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument) where TInitializer : Object, IInitializerEditorOnly
			=> OnMainThread(()=> Validate_NotThreadSafe(initializer, gameObject, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void UpdateReleaseArgumentOnDestroy<TInitializer, TArgument>(TInitializer initializer, Arguments flag, Any<TArgument> argument) where TInitializer : Object, IInitializerEditorOnly
			=> initializer.SetReleaseArgumentOnDestroy(flag, ShouldReleaseValueOnDestroy(argument));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void UpdateIsArgumentAsync<TInitializer, TArgument>(TInitializer initializer, Arguments flag, Any<TArgument> argument) where TInitializer : Object, IInitializerEditorOnly
			=> initializer.SetIsArgumentAsync(flag, IsAsyncValueProviderOrService(argument));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool ShouldReleaseValueOnDestroy<TArgument>(Any<TArgument> argument)
			=> argument.reference is ScriptableObject scriptableObject && scriptableObject
			&& (scriptableObject is IValueProvider or IValueByTypeProvider or IValueByTypeProviderAsync)
			&& (argument.reference is IValueByTypeReleaser or IValueReleaser<TArgument> ||
				(argument.reference is IValueProvider && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(scriptableObject))));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsAsyncValueProviderOrService<TArgument>(Any<TArgument> argument)
		{
			if(argument.reference)
			{
				return ValueProviderUtility.IsAsyncValueProvider(argument.reference);
			}
			
			if(ServiceAttributeUtility.definingTypes.TryGetValue(typeof(TArgument), out var serviceInfo) && serviceInfo.LoadAsync)
			{
				return true;
			}

			return false;
		}
		#endif

		internal static MissingInitArgumentsException GetMissingInitArgumentsException<TClient>([DisallowNull] IInitializer<TClient> initializer, Type argumentType = null)
		{
			var initializerType = initializer.GetType();
			var clientType = typeof(TClient);
			var context = initializer.Target ? initializer.Target : initializer as Object;

			if(argumentType is null)
			{
				return new($"{initializerType.Name} failed to initialize {clientType.Name}.", context);
			}

			var serviceAttributes = argumentType.GetCustomAttributes<ServiceAttribute>().ToArray();
			if(serviceAttributes.Length > 0)
			{
				var definingTypes = argumentType.GetCustomAttributes<ServiceAttribute>().SelectMany(att => att.definingTypes).ToArray();
				foreach(var definingType in definingTypes)
				{
					if(!definingType.IsAssignableFrom(argumentType))
					{
						if(typeof(IValueProvider<>).MakeGenericType(definingType).IsAssignableFrom(argumentType))
						{
							return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute and implements IValueProvider<{definingType.Name}> but the instance was still null.", context);
						}

						if(definingType.IsInterface)
						{
							return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not implement {definingType.Name}.", context);
						}

						return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not derive from {definingType.Name}.", context);
					}
				}

				if(definingTypes.Length > 0)
				{
					return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service({string.Join(", ", definingTypes.Select(t => "typeof(" + TypeUtility.ToString(t) + ")" ))})] attribute but its instance was still null.", context);
				}

				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service] attribute but its instance was still null.", context);
			}

			bool userCanModifyClass = clientType.Namespace is not string clientNamespace || !clientNamespace.StartsWith("Unity");
			string derivesFromImplementsOrIs = !userCanModifyClass
												? (!argumentType.IsAbstract && !TypeUtility.IsBaseType(argumentType) ? "a component of type" : argumentType.IsInterface ? "a component that implements" : "a component that derives from")
												: (!argumentType.IsAbstract && !TypeUtility.IsBaseType(argumentType) ? "the class" : argumentType.IsInterface ? "a class that implements" : "a class that derives from");

			if(!typeof(Component).IsAssignableFrom(argumentType))
			{
				if(!userCanModifyClass)
				{
					return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a value using the Inspector or select the 'Make Service Of Type...' item in the context menu of {derivesFromImplementsOrIs} {argumentType.Name}.", context);
				}

				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a value using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {derivesFromImplementsOrIs} {argumentType.Name}.\nIf you have already done one of these things, initialization could also be failing due to circular dependencies (e.g. ({clientType.Name} depends on {argumentType.Name}, and {argumentType.Name} depends on {clientType.Name}).", context);
			}

			if(!userCanModifyClass)
			{
				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a reference using the Inspector.", context);
			}

			return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a reference using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {derivesFromImplementsOrIs} {argumentType.Name}.", context);
		}

		internal static MissingInitArgumentsException GetMissingInitArgumentsException([DisallowNull] Type initializerType, [DisallowNull] Type clientType, Type argumentType = null)
		{
			if(argumentType is null)
			{
				return new($"{initializerType.Name} failed to initialize {clientType.Name}.");
			}

			var serviceAttributes = argumentType.GetCustomAttributes<ServiceAttribute>().ToArray();
			if(serviceAttributes.Length > 0)
			{
				var definingTypes = argumentType.GetCustomAttributes<ServiceAttribute>().SelectMany(att => att.definingTypes).ToArray();
				foreach(var definingType in definingTypes)
				{
					if(!definingType.IsAssignableFrom(argumentType))
					{
						if(typeof(IValueProvider<>).MakeGenericType(definingType).IsAssignableFrom(argumentType))
						{
							return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute and implements IValueProvider<{definingType.Name}> but the instance was still null.");
						}

						if(definingType.IsInterface)
						{
							return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not implement {definingType.Name}.");
						}

						return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service(typeof({definingType.Name}))] attribute but does not derive from {definingType.Name}.");
					}
				}

				if(definingTypes.Length > 0)
				{
					return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service({string.Join(", ", definingTypes.Select(t => "typeof(" + TypeUtility.ToString(t) + ")"))})] attribute but its instance was still null.");
				}

				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. The {argumentType.Name} class has the [Service] attribute but its instance was still null.");
			}

			bool userCanModifyClass = clientType.Namespace is not string clientNamespace || !clientNamespace.StartsWith("Unity");
			string derivesFromImplementsOrIs = !userCanModifyClass
												? (!argumentType.IsAbstract && !TypeUtility.IsBaseType(argumentType) ? "a component of type" : argumentType.IsInterface ? "a component that implements" : "a component that derives from")
												: (!argumentType.IsAbstract && !TypeUtility.IsBaseType(argumentType) ? "the class" : argumentType.IsInterface ? "a class that implements" : "a class that derives from");

			if(!typeof(Component).IsAssignableFrom(argumentType))
			{
				if(!userCanModifyClass)
				{
					return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a value using the Inspector or select the 'Make Service Of Type...' item in the context menu of {derivesFromImplementsOrIs} {argumentType.Name}.");
				}

				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a value using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {derivesFromImplementsOrIs} {argumentType.Name}.\nIf you have already done one of these things, initialization could also be failing due to circular dependencies (e.g. ({clientType.Name} depends on {argumentType.Name}, and {argumentType.Name} depends on {clientType.Name}).");
			}

			if(!userCanModifyClass)
			{
				return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a reference using the Inspector.");
			}

			return new($"{initializerType.Name} failed to initialize {clientType.Name} because missing argument of type {argumentType.Name}. Assign a reference using the Inspector or add the [Service(typeof({argumentType.Name}))] attribute to {derivesFromImplementsOrIs} {argumentType.Name}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAfterUninitializedWrappedObjectArgumentRetrieved<TArgument>(Component wrapperInitializer, ref TArgument argument)
		{
			if(argument == Null && wrapperInitializer.TryGetComponent(out IInitializer<TArgument> argumentInitializer) && argumentInitializer as Component != wrapperInitializer)
			{
				argument = argumentInitializer.InitTarget();
			}
		}

		#if UNITY_EDITOR
		internal static bool AreEqual<TArgument>(TArgument x, TArgument y) => x == Null ? y == Null : y != Null && x.Equals(y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Validate_Argument<TArgument>(Object initializer, [AllowNull] GameObject gameObject, Any<TArgument> argument, List<string> addArgumentTypeToListIfValidationFails)
		{
			if(!Validate_Argument(initializer as Component, gameObject, argument))
			{
				addArgumentTypeToListIfValidationFails.Add(TypeUtility.ToString(typeof(TArgument)));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Validate_Argument<TArgument>([AllowNull] GameObject gameObject, TArgument argument, List<string> addArgumentTypeToListIfValidationFails)
		{
			if(!Validate_Argument(gameObject, argument))
			{
				addArgumentTypeToListIfValidationFails.Add(TypeUtility.ToString(typeof(TArgument)));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Validate_Argument<TArgument>([AllowNull] Component initializer, [AllowNull] GameObject gameObject, Any<TArgument> argument)
			=> !IsNull(initializer, argument) || IsServiceDefiningType<TArgument>() || (gameObject is null ? Service.Exists<TArgument>() : Service.ExistsFor<TArgument>(gameObject));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Validate_Argument<TArgument>(GameObject gameObject, TArgument argument)
			=> argument != Null && !IsServiceDefiningType<TArgument>() && !(gameObject is null ? Service.Exists<TArgument>() : Service.ExistsFor<TArgument>(gameObject));

		internal static void Validate_NotThreadSafe<TInitializer, TArgument>(TInitializer initializer, [AllowNull] GameObject gameObject, Any<TArgument> argument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, argument);
			UpdateIsArgumentAsync(initializer, Arguments.First, argument);
			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, argument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument>(TInitializer initializer, GameObject gameObject, Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, firstArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Second, secondArgument);
			
			UpdateIsArgumentAsync(initializer, Arguments.First, firstArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Second, secondArgument);

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, secondArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument>(TInitializer initializer, GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, firstArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Second, secondArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Third, thirdArgument);
			
			UpdateIsArgumentAsync(initializer, Arguments.First, firstArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Second, secondArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Third, thirdArgument);

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, thirdArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(TInitializer initializer, GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, firstArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Second, secondArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Third, thirdArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Fourth, fourthArgument);
			
			UpdateIsArgumentAsync(initializer, Arguments.First, firstArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Second, secondArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Third, thirdArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Fourth, fourthArgument);

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, fourthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(TInitializer initializer, GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument, Any<TFifthArgument> fifthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, firstArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Second, secondArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Third, thirdArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Fourth, fourthArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Fifth, fifthArgument);
			
			UpdateIsArgumentAsync(initializer, Arguments.First, firstArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Second, secondArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Third, thirdArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Fourth, fourthArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Fifth, fifthArgument);

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, fifthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(TInitializer initializer, GameObject gameObject,
			Any<TFirstArgument> firstArgument, Any<TSecondArgument> secondArgument, Any<TThirdArgument> thirdArgument, Any<TFourthArgument> fourthArgument, Any<TFifthArgument> fifthArgument, Any<TSixthArgument> sixthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			UpdateReleaseArgumentOnDestroy(initializer, Arguments.First, firstArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Second, secondArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Third, thirdArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Fourth, fourthArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Fifth, fifthArgument);
			UpdateReleaseArgumentOnDestroy(initializer, Arguments.Sixth, sixthArgument);
			
			UpdateIsArgumentAsync(initializer, Arguments.First, firstArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Second, secondArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Third, thirdArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Fourth, fourthArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Fifth, fifthArgument);
			UpdateIsArgumentAsync(initializer, Arguments.Sixth, sixthArgument);

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(initializer, gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(initializer, gameObject, sixthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TArgument>(TInitializer initializer, GameObject gameObject, TArgument argument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, argument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eighthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eighthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, ninthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eighthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, ninthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, tenthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eighthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, ninthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, tenthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eleventhArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		internal static void Validate_NotThreadSafe<TInitializer, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(TInitializer initializer, GameObject gameObject,
		TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			if(!TryValidate_Initializer(initializer, gameObject))
			{
				return;
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Argument(gameObject, firstArgument, missingArgumentTypes);
			Validate_Argument(gameObject, secondArgument, missingArgumentTypes);
			Validate_Argument(gameObject, thirdArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fourthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, fifthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, sixthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, seventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eighthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, ninthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, tenthArgument, missingArgumentTypes);
			Validate_Argument(gameObject, eleventhArgument, missingArgumentTypes);
			Validate_Argument(gameObject, twelfthArgument, missingArgumentTypes);
			HandleNullGuardFailedMessage(initializer, gameObject, missingArgumentTypes);
			#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool TryValidate_Initializer<TInitializer>(TInitializer initializer, GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return false;
			}

			if(gameObject is not null)
			{
				UpdateHideFlags(initializer, gameObject);
			}

			#if !INIT_ARGS_DISABLE_EDITOR_VALIDATION
			Validate_Initializer(initializer, gameObject);

			if(initializer.NullArgumentGuard.IsDisabled(NullArgumentGuard.EditModeWarning))
			{
				initializer.NullGuardFailedMessage = "";
				return false;
			}

			return true;
			#else
			return false;
			#endif
		}

		#if UNITY_2022_2_OR_NEWER
		[HideInCallstack]
		#endif
		private static void HandleNullGuardFailedMessage<TInitializer>(TInitializer initializer, GameObject gameObject, List<string> missingArgumentTypes) where TInitializer : Object, IInitializerEditorOnly
		{
			if(missingArgumentTypes.Count == 0)
			{
				initializer.NullGuardFailedMessage = "";
				return;
			}

			string messageEnd = missingArgumentTypes.Count == 1
			? $"an argument: {missingArgumentTypes[0]}.\nIf the argument is optional set the 'Null Argument Guard' option to 'None'.\nIf the argument is a service that only becomes available at runtime set the option to 'Runtime Exception'."
			: $"{missingArgumentTypes.Count} arguments: {string.Join(", ", missingArgumentTypes)}.\nIf any of the arguments are optional set the 'Null Argument Guard' option to 'None'.\nIf the arguments are services that only becomes available at runtime set the option to 'Runtime Exception'.";

			missingArgumentTypes.Clear();

			initializer.NullGuardFailedMessage = "Missing " + messageEnd;

			if(ShouldValidateInitializer(initializer, gameObject))
			{
				Debug.LogWarning($"{initializer.GetType().Name} on GameObject \"{initializer.name}\" is missing " + messageEnd, initializer);
			}
		}

		private static bool ShouldValidateInitializer<TInitializer>(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly
			=> !initializer.WasJustReset && (gameObject is null
			? Array.IndexOf(Selection.objects, initializer) == -1
			: Array.IndexOf(Selection.gameObjects, gameObject) == -1 && gameObject.activeInHierarchy);

		private static bool IsBeingInspected<TInitializer>(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly => Array.IndexOf(Selection.objects, gameObject is null ? (Object)initializer : gameObject) >= 0;

		private static void Validate_Initializer<TInitializer>(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly
		{
			if(!initializer)
			{
				return;
			}

			// Avoid warning spam while Initializer is being actively modified in the Inspector
			if(IsBeingInspected(initializer, gameObject))
			{
				return;
			}

			var client = initializer.Target;
			if(!client)
			{
				if(initializer is ScriptableObject scriptableObject && AssetDatabase.IsSubAsset(scriptableObject))
				{
					AssetDatabase.RemoveObjectFromAsset(scriptableObject);
				}

				return;
			}

			if(!gameObject || initializer.WasJustReset || initializer.MultipleInitializersPerTargetAllowed || !gameObject.activeInHierarchy)
			{
				return;
			}

			using var initializers = gameObject.GetComponentsNonAlloc<IInitializer>();
			int count = initializers.Count;
			if(initializers.Count <= 1)
			{
				return;
			}

			for(int i = 0; i < count; i++)
			{
				var someInitializer = initializers[i];
				if(ReferenceEquals(someInitializer, initializer))
				{
					continue;
				}

				var someInitializerClient = someInitializer.Target;
				if(someInitializerClient == client)
				{
					Debug.LogWarning($"Only one Initializer per object is supported but {client.GetType().Name} on '{client.name}' has multiple Initializers targeting it. Clearing Target of surplus Initializer.", initializer as Object);
					initializer.Target = null;
					EditorUtility.SetDirty(client);
					return;
				}
			}
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
		internal static bool IsNull<TArgument>(TArgument argument) => argument == Null && (ThreadSafe.Application.IsPlaying || !IsServiceDefiningType<TArgument>());
		internal static bool IsNull<TArgument>(ScriptableObject initializer, Any<TArgument> argument) => !argument.GetHasValue(null, Context.MainThread);
		internal static bool IsNull<TArgument>(Component initializer, Any<TArgument> argument) => !argument.GetHasValue(initializer, Context.MainThread);

		/// <summary>
		/// Initializes the <paramref name="target"/> <typeparamref name="TWrapper"/> with the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> that contains the initializer component. </param>
		/// <param name="target">
		/// The existing <typeparamref name="TWrapper"/> scene instance to initialize,
		/// The prefab to clone to create the <typeparamref name="TWrapper"/> instance,
		/// or <see langword="null"/> if a new instance should be created from scratch..
		/// </param>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <see cref="target"/> <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TWrapper"/>. </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: NotNull]
		internal static TWrapper InitWrapper<TWrapper, TWrapped>([DisallowNull] GameObject gameObject, [AllowNull] TWrapper target, TWrapped wrappedObject)
			where TWrapper : MonoBehaviour, IWrapper<TWrapped>
		{
			if(!target)
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

		internal static void OnMainThread(EditorApplication.CallbackFunction action) => EditorApplication.delayCall += action;

		internal static void Reset<TInitializer, TClient, TArgument>(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 1);

			var argument =  initializer.Argument;

			Reset_Argument(ref argument, 0, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref argument);

			initializer.Argument = argument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 2);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 3);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 4);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 5);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 7);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 8);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;
			var eighthArgument =  initializer.EighthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eighthArgument, 7, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument, ref eighthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
			initializer.EighthArgument = eighthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 9);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;
			var eighthArgument =  initializer.EighthArgument;
			var ninthArgument =  initializer.NinthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eighthArgument, 7, shouldAutoInitArguments, initializer);
			Reset_Argument(ref ninthArgument, 8, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument, ref eighthArgument, ref ninthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
			initializer.EighthArgument = eighthArgument;
			initializer.NinthArgument = ninthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 10);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;
			var eighthArgument =  initializer.EighthArgument;
			var ninthArgument =  initializer.NinthArgument;
			var tenthArgument =  initializer.TenthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eighthArgument, 7, shouldAutoInitArguments, initializer);
			Reset_Argument(ref ninthArgument, 8, shouldAutoInitArguments, initializer);
			Reset_Argument(ref tenthArgument, 9, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument, ref eighthArgument, ref ninthArgument, ref tenthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
			initializer.EighthArgument = eighthArgument;
			initializer.NinthArgument = ninthArgument;
			initializer.TenthArgument = tenthArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 11);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;
			var eighthArgument =  initializer.EighthArgument;
			var ninthArgument =  initializer.NinthArgument;
			var tenthArgument =  initializer.TenthArgument;
			var eleventhArgument =  initializer.EleventhArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eighthArgument, 7, shouldAutoInitArguments, initializer);
			Reset_Argument(ref ninthArgument, 8, shouldAutoInitArguments, initializer);
			Reset_Argument(ref tenthArgument, 9, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eleventhArgument, 10, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument, ref eighthArgument, ref ninthArgument, ref tenthArgument, ref eleventhArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
			initializer.EighthArgument = eighthArgument;
			initializer.NinthArgument = ninthArgument;
			initializer.TenthArgument = tenthArgument;
			initializer.EleventhArgument = eleventhArgument;
		}

		internal static void Reset<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			(TInitializer initializer, [AllowNull] GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			Reset_Initializer<TInitializer, TClient>(initializer, gameObject);
			bool shouldAutoInitArguments = TryPrepareArgumentsForAutoInit<TClient>(initializer, 12);

			var firstArgument =  initializer.FirstArgument;
			var secondArgument =  initializer.SecondArgument;
			var thirdArgument =  initializer.ThirdArgument;
			var fourthArgument =  initializer.FourthArgument;
			var fifthArgument =  initializer.FifthArgument;
			var sixthArgument =  initializer.SixthArgument;
			var seventhArgument =  initializer.SeventhArgument;
			var eighthArgument =  initializer.EighthArgument;
			var ninthArgument =  initializer.NinthArgument;
			var tenthArgument =  initializer.TenthArgument;
			var eleventhArgument =  initializer.EleventhArgument;
			var twelfthArgument  =  initializer.TwelfthArgument;

			Reset_Argument(ref firstArgument, 0, shouldAutoInitArguments, initializer);
			Reset_Argument(ref secondArgument, 1, shouldAutoInitArguments, initializer);
			Reset_Argument(ref thirdArgument, 2, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fourthArgument, 3, shouldAutoInitArguments, initializer);
			Reset_Argument(ref fifthArgument, 4, shouldAutoInitArguments, initializer);
			Reset_Argument(ref sixthArgument, 5, shouldAutoInitArguments, initializer);
			Reset_Argument(ref seventhArgument, 6, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eighthArgument, 7, shouldAutoInitArguments, initializer);
			Reset_Argument(ref ninthArgument, 8, shouldAutoInitArguments, initializer);
			Reset_Argument(ref tenthArgument, 9, shouldAutoInitArguments, initializer);
			Reset_Argument(ref eleventhArgument, 10, shouldAutoInitArguments, initializer);
			Reset_Argument(ref twelfthArgument, 11, shouldAutoInitArguments, initializer);

			initializer.OnReset(ref firstArgument, ref secondArgument, ref thirdArgument, ref fourthArgument, ref fifthArgument, ref sixthArgument, ref seventhArgument, ref eighthArgument, ref ninthArgument, ref tenthArgument, ref eleventhArgument, ref twelfthArgument);

			initializer.FirstArgument = firstArgument;
			initializer.SecondArgument = secondArgument;
			initializer.ThirdArgument = thirdArgument;
			initializer.FourthArgument = fourthArgument;
			initializer.FifthArgument = fifthArgument;
			initializer.SixthArgument = sixthArgument;
			initializer.SeventhArgument = seventhArgument;
			initializer.EighthArgument = eighthArgument;
			initializer.NinthArgument = ninthArgument;
			initializer.TenthArgument = tenthArgument;
			initializer.EleventhArgument = eleventhArgument;
			initializer.TwelfthArgument = twelfthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TArgument>(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 1);
			if(AutoInit_Argument(out TArgument argument, 0, true, initializer)) initializer.Argument = argument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 2);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 3);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 4);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 5);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
			if(AutoInit_Argument(out TEighthArgument eighthArgument, 7, true, initializer)) initializer.EighthArgument = eighthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
			if(AutoInit_Argument(out TEighthArgument eighthArgument, 7, true, initializer)) initializer.EighthArgument = eighthArgument;
			if(AutoInit_Argument(out TNinthArgument ninthArgument, 8, true, initializer)) initializer.NinthArgument = ninthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
			if(AutoInit_Argument(out TEighthArgument eighthArgument, 7, true, initializer)) initializer.EighthArgument = eighthArgument;
			if(AutoInit_Argument(out TNinthArgument ninthArgument, 8, true, initializer)) initializer.NinthArgument = ninthArgument;
			if(AutoInit_Argument(out TTenthArgument tenthArgument, 9, true, initializer)) initializer.TenthArgument = tenthArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
			if(AutoInit_Argument(out TEighthArgument eighthArgument, 7, true, initializer)) initializer.EighthArgument = eighthArgument;
			if(AutoInit_Argument(out TNinthArgument ninthArgument, 8, true, initializer)) initializer.NinthArgument = ninthArgument;
			if(AutoInit_Argument(out TTenthArgument tenthArgument, 9, true, initializer)) initializer.TenthArgument = tenthArgument;
			if(AutoInit_Argument(out TEleventhArgument eleventhArgument, 10, true, initializer)) initializer.EleventhArgument = eleventhArgument;
		}

		internal static void AutoInitInEditMode<TInitializer, TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			(TInitializer initializer) where TInitializer : Object, IInitializerEditorOnly<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			TryPrepareArgumentsForAutoInit<TClient>(initializer, 6);
			if(AutoInit_Argument(out TFirstArgument firstArgument, 0, true, initializer)) initializer.FirstArgument = firstArgument;
			if(AutoInit_Argument(out TSecondArgument secondArgument, 1, true, initializer)) initializer.SecondArgument = secondArgument;
			if(AutoInit_Argument(out TThirdArgument thirdArgument, 2, true, initializer)) initializer.ThirdArgument = thirdArgument;
			if(AutoInit_Argument(out TFourthArgument fourthArgument, 3, true, initializer)) initializer.FourthArgument = fourthArgument;
			if(AutoInit_Argument(out TFifthArgument fifthArgument, 4, true, initializer)) initializer.FifthArgument = fifthArgument;
			if(AutoInit_Argument(out TSixthArgument sixthArgument, 5, true, initializer)) initializer.SixthArgument = sixthArgument;
			if(AutoInit_Argument(out TSeventhArgument seventhArgument, 6, true, initializer)) initializer.SeventhArgument = seventhArgument;
			if(AutoInit_Argument(out TEighthArgument eighthArgument, 7, true, initializer)) initializer.EighthArgument = eighthArgument;
			if(AutoInit_Argument(out TNinthArgument ninthArgument, 8, true, initializer)) initializer.NinthArgument = ninthArgument;
			if(AutoInit_Argument(out TTenthArgument tenthArgument, 9, true, initializer)) initializer.TenthArgument = tenthArgument;
			if(AutoInit_Argument(out TEleventhArgument eleventhArgument, 10, true, initializer)) initializer.EleventhArgument = eleventhArgument;
			if(AutoInit_Argument(out TTwelfthArgument twelfthArgument, 11, true, initializer)) initializer.TwelfthArgument = twelfthArgument;
		}

		internal static void Reset_Initializer<TInitializer, TClient>([DisallowNull] TInitializer initializer, GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly<TClient>
		{
			initializer.WasJustReset = true;

			if(gameObject is null)
			{
				return;
			}


			if(TryFindTargetForInitializer(gameObject, typeof(TClient), out Component setTarget, initializer.MultipleInitializersPerTargetAllowed))
			{
				initializer.Target = setTarget;
			}

			UpdateHideFlags(initializer, gameObject);
		}

		private static bool TryFindTargetForInitializer(GameObject gameObject, Type clientType, out Component result, bool multipleInitializersPerTargetAllowed)
		{
			if(!Find.typesToComponentTypes.TryGetValue(clientType, out var componentTypes))
			{
				if(typeof(StateMachineBehaviour).IsAssignableFrom(clientType))
				{
					componentTypes = new[] { typeof(Animator) };
				}
				#if UI_TOOLKIT
				else if(typeof(UnityEngine.UIElements.VisualElement).IsAssignableFrom(clientType))
				{
					componentTypes = new[] { UnityEngine.UIElements.UIDocument };
				}
				#endif
				else
				{
					result = null;
					return false;
				}
			}

			using var initializers = gameObject.GetComponentsNonAlloc<IInitializer>();

			foreach(Type componentType in componentTypes)
			{
				gameObject.GetComponents(componentType, components);

				foreach(var component in components)
				{
					if(!initializers.Any(initializer => initializer.Target == component
					&& (!multipleInitializersPerTargetAllowed || initializer is not IInitializerEditorOnly initializerEditorOnly || !initializerEditorOnly.MultipleInitializersPerTargetAllowed)))
					{
						result = component;
						return true;
					}
				}
			}

			result = null;
			return false;
		}

		private static void UpdateHideFlags<TInitializer>(TInitializer initializer, GameObject gameObject) where TInitializer : Object, IInitializerEditorOnly
		{
			var setHideFlags = initializer.Target is not Component clientComponent || !clientComponent || clientComponent.gameObject != gameObject ? HideFlags.None : HideFlags.HideInInspector;

			if(setHideFlags != initializer.hideFlags)
			{
				Undo.RecordObject(initializer, "Update HideFlags");
				initializer.hideFlags = setHideFlags;
			}
		}

		private static void Reset_Argument<TArgument, TInitializer>(ref TArgument argument, int argumentIndex, bool shouldAutoInitArguments, [DisallowNull] TInitializer initializer) where TInitializer : IInitializer
		{
			if(IsServiceDefiningType<TArgument>())
			{
				argument = default;
				return;
			}

			if(shouldAutoInitArguments)
			{
				argument = GetAutoInitArgument<TInitializer, TArgument>(initializer, argumentIndex);

				if(initializer is Component component)
				{
					if(Service.ForEquals(component.gameObject, argument))
					{
						argument = default;
						return;
					}
				}
				else if(Service.TryGet(out TArgument service) && ReferenceEquals(service, argument))
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
			foreach(Type derivedType in TypeCache.GetTypesDerivedFrom<TArgument>())
			{
				if(derivedType.IsAbstract)
				{
					continue;
				}

				if(onlyConcreteType is not null)
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

		private static bool AutoInit_Argument<TArgument, TInitializer>(out TArgument argument, int argumentIndex, bool canAutoInitArguments, [DisallowNull] TInitializer initializer) where TInitializer : IInitializer
		{
			if(!canAutoInitArguments || GetAutoInitArgument<TInitializer, TArgument>(initializer, argumentIndex) is not TArgument value)
			{
				argument = default;
				return false;
			}

			if(initializer is Component component)
			{
				if(Service.ForEquals(component.gameObject, value))
				{
					argument = default;
					return false;
				}
			}
			else if(Service.TryGet(out TArgument service) && ReferenceEquals(service, value))
			{
				argument = default;
				return false;
			}

			argument = value;
			return true;
		}

		private static TArgument CreateInstance<TArgument>() => CreateInstance<TArgument>(typeof(TArgument));

		private static TArgument CreateInstance<TArgument>(Type instanceType)
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

		public static void OnInitializableReset(MonoBehaviour initializable)
		{
			var type = initializable.GetType();
			var initializersOnGameObject = initializable.GetComponents<IInitializer>();
			
			foreach(var initializerOnGameObject in initializersOnGameObject)
			{
				if(ReferenceEquals(initializerOnGameObject.Target, initializable))
				{
					return;
				}
			}
			
			foreach(var initializerOnGameObject in initializersOnGameObject)
			{
				if(!initializerOnGameObject.Target && initializerOnGameObject.TargetIsAssignableOrConvertibleToType(type))
				{
					initializerOnGameObject.Target = initializable;
					return;
				}
			}

			var initializerTypes = GetInitializerTypes(initializable);

			// If Initializable has exactly one concrete Initializer class targeting it in particular,
			// then attach it to GameObjects alongside the Initializable by default...
			if(initializerTypes.Length == 1

			// ...however, only do this if the user is (likely) doing this via the Inspector.
			// If the component was added in code, we should skip doing this, because
			// this could be done as part of a unit test, and there were crashing issues
			// (in Unity 2021.3.4f1) when this was done.
			&& Array.IndexOf(Selection.gameObjects, initializable.gameObject) != -1)
			{
				var initializerType = initializerTypes[0];
				var newInitializer = initializable.gameObject.IsAsset(true)
					? initializable.gameObject.AddComponent(initializerType) as IInitializer
					: Undo.AddComponent(initializable.gameObject, initializerType) as IInitializer;

				newInitializer.Target = initializable;
			}
		}
		#endif
	}
}