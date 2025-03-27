//#define DEBUG_DISPOSE
#define DEBUG_REPAINT
//#define DEBUG_SET_UNFOLDED

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditorInternal;
using UnityEngine;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;
using static Sisus.Init.FlagsValues;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using UnityEngine.Profiling;
using Unity.Profiling;
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	public delegate void AfterHeaderGUIHandler(Rect remainingRect, Editor initializerEditor);

	[InitializeOnLoad]
	public sealed class InitializerGUI : IDisposable
	{
		const float IconWidth = 20f;

		public static bool ServicesShown
		{
			get => EditorPrefs.GetBool(ServiceVisibilityEditorPrefsKey, true);
			set => EditorPrefs.SetBool(ServiceVisibilityEditorPrefsKey, value);
		}

		public const string SetInitializerTargetOnScriptsReloadedEditorPrefsKey = "InitArgs.SetInitializerTarget";
		private const string ServiceVisibilityEditorPrefsKey = "InitArgs.InitializerServiceVisibility";
		private const string HideInitSectionUserDataKey = "hideInitSection";
		private const string NullArgumentGuardUserDataKey = "nullArgumentGuard";
		private const string DefaultHeaderText = "Init";
		private const string ClientInitializedDuringOnAfterDeserializeText = "Client will be initialized during deserialization.";
		private static readonly GUIContent ClientInitializedDuringOnAfterDeserializeLabel = new(ClientInitializedDuringOnAfterDeserializeText);
		private const string SpaceForButtons = "\n\n\n";
		private const string ClientInitializedDuringOnAfterDeserializeTextWithSpaceForButton = SpaceForButtons + ClientInitializedDuringOnAfterDeserializeText;
		private const string ClientInitializedWhenBecomesActiveText = "Client will be initialized when it becomes active.";
		private static readonly GUIContent ClientInitializedWhenBecomesActiveLabel = new(ClientInitializedWhenBecomesActiveText, "Client will only receive its Init arguments once the game object becomes active.");
		private const string SomeDependenciesMissingText = "Some services that this object depends on are missing.";
		private const string SomeDependenciesMissingTooltipNoInitializer = "Potential fixes:\n- Register Missing Services\n- Change Service Availability\n- Attach an Initializer";
		private const string SomeDependenciesMissingTooltipHasInitializer = "Potential fixes:\n- Register Missing Services\n- Change Service Availability\n- Assign Values Using Inspector\n- Select 'Wait For Service'";
		private static readonly GUIContent SomeDependenciesMissingLabel = new(SomeDependenciesMissingText);
		private const string HelpLinkText = "Help";
		private static readonly GUIContent HelpLinkLabel = new(HelpLinkText, "Open Documentation");
		private const string HelpLinkURL = "https://docs.sisus.co/init-args/problems-and-solutions/client-not-receiving-services/";
		
		private const string ClientInitializedWhenBecomesActiveTextWithSpaceForButton = SpaceForButtons + ClientInitializedWhenBecomesActiveText;
		private const string IsUnfoldedUserDataKey = "initArgsUnfolded";
		private const string AddInitializerTooltip = "Attach an Initializer.\n\n" +
		                                             "Initializers can be used to customize the Init arguments that this component receives using the Inspector.";
		private const string AddStateMachineInitializerTooltip = "Attach a State Machine Behaviour Initializer.\n\nThis can be used to customize the arguments received by the state machine behaviour during initialization.";
		private static readonly GUIContent useAwakeButtonLabel = new(" Use Awake", "Initialize target later during the Awake event when the game object becomes active?");
		private static readonly GUIContent useOnAfterDeserializeButtonLabel = new(" Use OnAfterDeserialize", "Initialize target earlier during the OnAfterDeserialize event before the game object becomes active?");
		private static readonly GUIContent notFoundLabel = new GUIContent("Not Found", "No global service of type Service not found.\n\nIf this is a scene based service that only becomes available at runtime, you can attach an Initializer to this component and then select 'Wait For Service' from the Init argument's dropdown menu.");
		private const string notFoundTooltip = "No service of type {0} was found.\n\nYou can use [Service(typeof({0}))] to define a global service.\n\nIf {0} only becomes available at runtime, you can attach an Initializer to this component and select 'Wait For Service' from the Init argument's dropdown menu.";
		private static readonly Vector2 IconSize = new(16f, 16f);
		
		public static InitializerGUI NowDrawing { get; private set; }

		public event Action<InitializerGUI> Changed;
		public event AfterHeaderGUIHandler AfterHeaderGUI;

		/// <summary>
		/// Scriptable object of the editor that owns this InitializerGUI (e.g. InitializerEditor, InitializableEditor, MultiInitializableEditor).
		/// </summary>
		private readonly SerializedObject ownerSerializedObject;
		private readonly Object[] targets; // E.g. Initializer, Animator
		private readonly object[] initializables; // E.g. MonoBehaviour<T...>, StateMachineBehaviour<T...>
		private bool isResponsibleForInitializerEditorLifetime;
		private readonly GameObject[] gameObjects;
		private readonly Object[] rootObjects; // e.g. GameObject[] or ScriptableObject[]
		private ServiceChangedListener[] serviceChangedListeners = Array.Empty<ServiceChangedListener>();

		private GUIStyle initArgsFoldoutBackgroundStyle;
		private GUIStyle initArgsFoldoutStyle;
		private GUIStyle noInitArgsLabelStyle;
		private readonly GUIContent addInitializerIcon = new();
		private readonly GUIContent addInitializerTooltipOnly = new();
		private readonly GUIContent contextMenuIcon = new();
		private readonly GUIContent nullGuardDisabledIcon = new();
		private readonly GUIContent nullGuardPassedWithValueProviderValueMissing = new();
		private readonly GUIContent nullGuardPassedIcon = new();
		private readonly GUIContent nullGuardFailedIcon = new();
		private readonly GUIContent initStateUninitializedIcon = new();
		private readonly GUIContent initStateInitializedIcon = new();
		private readonly GUIContent initStateFailedIcon = new();
		private readonly GUIContent initStateInitializingIcon = new();
		private readonly GUIContent servicesHiddenIcon = new();
		private readonly GUIContent servicesShownIcon = new();
		private readonly GUIContent headerLabel = new(DefaultHeaderText);
		private GUIStyle initializerBackgroundStyle;
		private GUIStyle noInitializerBackgroundStyle;
		private readonly Type[] initParameterTypes;
		private readonly bool[] initParametersAreServices;
		private bool hasServiceParameters;
		private bool allParametersAreServices;
		private bool anyParameterIsAsyncLoadedService;
		private readonly bool targetImplementsIArgs;
		private readonly bool targetCanSelfInitializeWithoutInitializer;
		private bool? hadInitializerLastFrame;
		private NullGuardResult? nullGuardResultLastFrame;
		private Object[] initializers = new Object[1];
		private Editor initializerEditor;
		private bool lockInitializers;
		private bool shouldUpdateInitArgumentDependentState;

		#if ODIN_INSPECTOR
		private PropertyTree odinPropertyTree;
		internal PropertyTree OdinPropertyTree => odinPropertyTree ??= PropertyTree.Create(ownerSerializedObject);
		#endif

		[MaybeNull]
		private Object Target => targets[0];

		[MaybeNull]
		private GameObject FirstGameObject => gameObjects.Length > 0 ? gameObjects[0] : null;

		public Action<Rect> OnAddInitializerButtonPressedOverride { get; set; }

		public Object[] Initializers
		{
			get => initializers;

			set
			{
				#if DEV_MODE
				Debug.Assert(value != null && value.GetType() == typeof(Object[]));
				#endif

				initializers = value;
				lockInitializers = true;
				headerLabel.text = initializers.Length == 0 || !initializers[0] ? DefaultHeaderText : "Init → " + ObjectNames.NicifyVariableName(initializables[0].GetType().Name);
				GUI.changed = true;
			}
		}

		static InitializerGUI() => InitializerUtility.requestNullArgumentGuardFlags = GetNullArgumentGuardFlags;

		/// <param name="ownerSerializedObject">
		/// Scriptable object of the editor that owns this InitializerGUI (e.g. InitializerEditor, InitializableEditor, MultiInitializableEditor).
		/// </param>
		/// <param name="ownerSerializedObject">
		/// Scriptable object of the InitializerEditor that owns this InitializerGUI.
		/// </param>
		/// <param name="targets">
		/// The components or scriptable objects that are the targets of the top-level Editor.
		/// <para>
		/// These can be for example Initializer components or Animator components.
		/// </para>
		/// </param>
		/// <param name="initializables">
		/// The client objects to which the Init arguments will be injected.
		/// <para>
		/// These can be for example <see cref="MonoBehaviour{T...}"/> or <see cref="StateMachineBehaviour{T...}"/> instances.
		/// </para>
		/// </param>
		/// <param name="initParameterTypes"> Types of all init parameter, if known; otherwise an empty array. </param>
		/// <param name="initializerEditor"> (Optional) Delegate for drawing the arguments inside the Init section. </param>
		/// <param name="gameObjects"> GamesObjects for all component type clients. If null, then the array  will be automatically generated. </param>
		public InitializerGUI(SerializedObject ownerSerializedObject, object[] initializables, Type[] initParameterTypes, InitializerEditor initializerEditor = null, GameObject[] gameObjects = null)
		{
			NowDrawing = this;

			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = constructorMarker.Auto();
			#endif

			this.ownerSerializedObject = ownerSerializedObject;
			targets = ownerSerializedObject.targetObjects;
			this.initializables = initializables;

			int count = targets.Length;
			if(count > 0)
			{
				var target = targets[0];
				if(gameObjects is null)
				{
					if(target is Component)
					{
						gameObjects = new GameObject[count];

						for(int i = 0; i < count; i++)
						{
							var component = targets[i] as Component;
							gameObjects[i] = component ? component.gameObject : null;
						}
					}
					else
					{
						gameObjects = Array.Empty<GameObject>();
					}
				}

				var targetType = target.GetType();
				targetCanSelfInitializeWithoutInitializer = InitializableUtility.CanSelfInitializeWithoutInitializer(target);
				targetImplementsIArgs = InitializableUtility.TryGetIArgsInterface(targetType, out _);
			}

			this.initParameterTypes = initParameterTypes;
			initParametersAreServices = new bool[initParameterTypes.Length];

			this.gameObjects = gameObjects;
			rootObjects = gameObjects.Length > 0 ? gameObjects : targets;

			isResponsibleForInitializerEditorLifetime = initializerEditor is null;
			this.initializerEditor = initializerEditor;

			GetInitializersOnTargets(out bool hasInitializers, out Object firstInitializer);

			if(TryGetCustomHeaderLabel(hasInitializers, firstInitializer, out string customHeaderText))
			{
				headerLabel.text = customHeaderText;
			}
			else
			{
				headerLabel.text = DefaultHeaderText;
			}

			// Make sure tooltips are updated when services change etc.
			ServiceChangedListener.UpdateAll(ref serviceChangedListeners, initParameterTypes, OnInitArgumentServiceChanged);

			Setup();
			UpdateInitArgumentDependentState(hasInitializers, firstInitializer);
			NowDrawing = null;
		}

		public bool IsValid()
		{
			// Check if any of the Editor targets have been destroyed
			foreach(var target in targets)
			{
				if(!target)
				{
					return false;
				}
			}

			foreach(var initializable in initializables)
			{
				// Check if any of the initializers have been destroyed
				if(initializable is Object unityObject && !unityObject)
				{
					return false;
				}
			}
			
			return true;
		}

		private void UpdateInitArgumentDependentState(bool hasInitializers, Object firstInitializer)
		{
			shouldUpdateInitArgumentDependentState = false;
			int count = initParameterTypes.Length;
			allParametersAreServices = true;
			anyParameterIsAsyncLoadedService = false;
			hasServiceParameters = false;
			for(int i = 0; i < count; i++)
			{
				Type parameterType = initParameterTypes[i];

				if(ServiceAttributeUtility.definingTypes.TryGetValue(parameterType, out var serviceInfo) && serviceInfo.LoadAsync)
				{
					anyParameterIsAsyncLoadedService = true;
					initParametersAreServices[i] = true;
				}
				else if(IsService(parameterType))
				{
					hasServiceParameters = true;
					initParametersAreServices[i] = true;
				}
				else
				{
					allParametersAreServices = false;
					initParametersAreServices[i] = false;
				}
			}

			UpdateTooltips(hasInitializers, firstInitializer);
		}

		private void OnInitArgumentServiceChanged() => shouldUpdateInitArgumentDependentState = true;

		/// <summary>
		/// Gets a value indicating whether or not the user has hidden the Init section for the target.
		/// </summary>
		/// <param name="target">
		/// Components or scriptable object that is the target of the top-level Editor.
		/// <para>
		/// This can be for example an Initializer or an Animator component.
		/// </para>
		/// </param>
		/// <returns> <see langword="true"/> if user has hidden the Init section for the target via the context menu; otherwise, <see langword="false"/>. </returns>
		public static bool IsInitSectionHidden([DisallowNull] Object target)
		{
			GetScriptAndTargetType(target, out MonoScript targetScript, out Type targetType);
			return IsInitSectionHidden(targetScript, targetType);
		}
		
		private static void GetScriptAndTargetType([DisallowNull] Object target, out MonoScript targetScript, out Type targetType)
		{
			if(target is MonoBehaviour monoBehaviour)
			{
				targetScript = MonoScript.FromMonoBehaviour(monoBehaviour);
				targetType = monoBehaviour.GetType();
			}
			else if(target is MonoScript script)
			{
				targetScript = script;
				targetType = targetScript.GetClass();
			}
			else
			{
				targetScript = null;
				targetType = target.GetType();
			}
		}

		public static bool IsInitSectionHidden([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType) => GetBoolUserData(initializableScript, initializableType, HideInitSectionUserDataKey);

		public static void ToggleHideInitSection([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType)
		{
			const string userDataKey = HideInitSectionUserDataKey;
			bool setValue = !GetBoolUserData(initializableScript, initializableType, userDataKey);
			Menu.SetChecked(ComponentMenuItems.ShowInitSection, setValue);
			SetUserData(initializableScript, initializableType, userDataKey, setValue);
			EditorDecoratorInjector.RemoveFrom(initializableType);
		}

		public static NullArgumentGuard GetNullArgumentGuardFlags([DisallowNull] Object target)
		{
			Type targetType;
			MonoScript initializableScript;
			if(target is MonoBehaviour monoBehaviour)
			{
				if(InitializerUtility.TryGetInitializer(monoBehaviour, out IInitializer initializer)
					&& initializer is IInitializerEditorOnly initializerEditorOnly)
				{
					return initializerEditorOnly.NullArgumentGuard;
				}

				initializableScript = MonoScript.FromMonoBehaviour(monoBehaviour);
				targetType = monoBehaviour.GetType();
			}
			else
			{
				initializableScript = target as MonoScript;
				targetType = initializableScript ? initializableScript.GetClass() : target.GetType();
			}

			return GetEnumUserData(initializableScript, targetType, NullArgumentGuardUserDataKey, InitializerUtility.DefaultNullArgumentGuardFlags);
		}

		public static void ToggleNullArgumentGuardFlag([DisallowNull] Object target, NullArgumentGuard flag)
		{
			Type initializableType;
			MonoScript initializableScript;
			if(target is MonoBehaviour monoBehaviour)
			{
				if(InitializerUtility.TryGetInitializer(monoBehaviour, out IInitializer initializer)
					&& initializer is IInitializerEditorOnly initializerEditorOnly)
				{
					Undo.RecordObject(target, "Set Null Argument Guard");
					initializerEditorOnly.NullArgumentGuard = WithFlagToggled(initializerEditorOnly.NullArgumentGuard, flag);
					return;
				}

				initializableScript = MonoScript.FromMonoBehaviour(monoBehaviour);
				initializableType = monoBehaviour.GetType();
			}
			else
			{
				initializableScript = target as MonoScript;
				initializableType = initializableScript ? initializableScript.GetClass() : target.GetType();
			}

			ToggleNullArgumentGuardFlag(initializableScript, initializableType, flag);
			
		}

		public static void ToggleNullArgumentGuardFlag([DisallowNull] Object[] targets, NullArgumentGuard flag)
		{
			bool alltargetsHandled = true;

			foreach(Object target in targets)
			{
				if(target is MonoBehaviour monoBehaviour
					&& InitializerUtility.TryGetInitializer(monoBehaviour, out IInitializer initializer)
					&& initializer is IInitializerEditorOnly initializerEditorOnly)
				{
					Undo.RecordObject(target, "Set Null Argument Guard");
					initializerEditorOnly.NullArgumentGuard = WithFlagToggled(initializerEditorOnly.NullArgumentGuard, flag);
					continue;
				}

				alltargetsHandled = false;
			}

			if(alltargetsHandled)
			{
				return;
			}

			// If any of the targets does not have an initializer attached, then record the flag in the script's metadata

			Type initializableType;
			MonoScript initializableScript;
			var firstTarget = targets[0];
			if(firstTarget is MonoBehaviour firstMonoBehaviour)
			{
				initializableScript = MonoScript.FromMonoBehaviour(firstMonoBehaviour);
				initializableType = firstMonoBehaviour.GetType();
			}
			else
			{
				initializableScript = firstTarget as MonoScript;
				initializableType = initializableScript ? initializableScript.GetClass() : firstTarget.GetType();
			}

			ToggleNullArgumentGuardFlag(initializableScript, initializableType, flag);
		}

		private void SetNullArgumentGuardFlags(NullArgumentGuard value)
		{
			bool allTargetsHandled = true;
			
			if(Target is IInitializer)
			{
				if(ownerSerializedObject.FindProperty("nullArgumentGuard") is { } nullArgumentGuardProperty)
				{
					nullArgumentGuardProperty.intValue = (int)value;
					ownerSerializedObject.ApplyModifiedProperties();
				}
				else
				{
					Undo.RecordObjects(targets, "Set Null Argument Guard");

					foreach(Object target in targets)
					{
						if (target is not IInitializerEditorOnly initializerEditorOnly)
						{
							allTargetsHandled = false;
							continue;
						}

						initializerEditorOnly.NullArgumentGuard = value;
					}

					ownerSerializedObject.Update();
					ownerSerializedObject.ApplyModifiedProperties();
				}
			}
			else
			{
				var initializers = targets.Select(t => t is MonoBehaviour monoBehaviour && InitializerUtility.TryGetInitializer(monoBehaviour, out IInitializer initializer) ? initializer as Object : null).Where(i => i).ToArray();

				if(initializers.Any())
				{
					using var tempSerializedObject = new SerializedObject(initializers);
					if(tempSerializedObject.FindProperty("nullArgumentGuard") is { } nullArgumentGuardProperty)
					{
						nullArgumentGuardProperty.intValue = (int)value;
						tempSerializedObject.ApplyModifiedProperties();
					}
					else
					{
						Undo.RecordObjects(initializers, "Set Null Argument Guard");

						foreach(Object initializer in initializers)
						{
							if(initializer is not IInitializerEditorOnly initializerEditorOnly)
							{
								allTargetsHandled = false;
								continue;
							}

							initializerEditorOnly.NullArgumentGuard = value;
						}

						tempSerializedObject.Update();
						tempSerializedObject.ApplyModifiedProperties();
					}
				}
				else
				{
					allTargetsHandled = false;
				}
			}

			if(allTargetsHandled)
			{
				return;
			}

			// If any of the targets does not have an initializer attached, then record the flag in the script's metadata

			Type initializableType;
			MonoScript initializableScript;
			var firstTarget = targets[0];
			if(firstTarget is MonoBehaviour firstMonoBehaviour)
			{
				initializableScript = MonoScript.FromMonoBehaviour(firstMonoBehaviour);
				initializableType = firstMonoBehaviour.GetType();
			}
			else
			{
				initializableScript = firstTarget as MonoScript;
				initializableType = initializableScript ? initializableScript.GetClass() : firstTarget.GetType();
			}

			SetNullArgumentGuardFlags(initializableScript, initializableType, value);
		}

		private static void SetNullArgumentGuardFlags([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType, NullArgumentGuard value)
			=> SetUserData(initializableScript, initializableType, NullArgumentGuardUserDataKey, value, InitializerUtility.DefaultNullArgumentGuardFlags);

		private static void ToggleNullArgumentGuardFlag([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType, NullArgumentGuard flag)
		{
			var nullArgumentGuardWas = GetEnumUserData(initializableScript, initializableType, NullArgumentGuardUserDataKey, InitializerUtility.DefaultNullArgumentGuardFlags);
			NullArgumentGuard setNullArgumentGuard = WithFlagToggled(nullArgumentGuardWas, flag);
			SetNullArgumentGuardFlags(initializableScript, initializableType, setNullArgumentGuard);
		}

		private static NullArgumentGuard WithFlagToggled(NullArgumentGuard nullArgumentGuard, NullArgumentGuard toggleFlag)
			=> toggleFlag == NullArgumentGuard.None ? NullArgumentGuard.None : nullArgumentGuard.WithFlagToggled(toggleFlag);

		private static bool GetBoolUserData([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType, [DisallowNull] string userDataKey, bool defaultValue = false)
		{
			if (initializableScript && initializableScript.TryGetUserData(userDataKey, out bool? valueFromMetaData))
			{
				return valueFromMetaData ?? defaultValue;
			}

			// Use EditorPrefs as fallback, in cases where type is inside a DLL etc.
			return EditorPrefsUtility.GetBoolUserData(initializableType, userDataKey, defaultValue);
		}

		private static TEnum GetEnumUserData<TEnum>([AllowNull] MonoScript targetScript, [DisallowNull] Type targetType, [DisallowNull] string userDataKey, TEnum defaultValue = default) where TEnum : struct, Enum
		{
			if(targetScript && targetScript.TryGetUserData(userDataKey, out TEnum? valueFromMetaData))
			{
				return valueFromMetaData ?? defaultValue;
			}

			// Use EditorPrefs as fallback, in cases where type is inside a DLL etc.
			return EditorPrefsUtility.GetEnumUserData(targetType, userDataKey, defaultValue);
		}

		private static void SetUserData([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType, [DisallowNull] string userDataKey, bool value, bool defaultValue = false)
		{
			if(!initializableScript || !initializableScript.TrySetUserData(userDataKey, value, defaultValue))
			{
				EditorPrefsUtility.SetUserData(initializableType, userDataKey, value, defaultValue);
			}
		}

		private static void SetUserData<TEnum>([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType, [DisallowNull] string userDataKey, TEnum value, TEnum defaultValue = default) where TEnum : Enum
		{
			if(!initializableScript || !initializableScript.TrySetUserData(userDataKey, value, defaultValue))
			{
				EditorPrefsUtility.SetUserData(initializableType, userDataKey, value, defaultValue);
			}
		}

		private bool IsService(Type parameterType) => ServiceUtility.IsServiceDefiningType(parameterType) || (Target && ServiceUtility.ExistsFor(Target, parameterType));

		private static bool TryGetCustomHeaderLabel(bool hasInitializers, Object firstInitializer, out string customHeaderText)
		{
			if(hasInitializers && firstInitializer.GetType().GetNestedType(InitializerEditor.InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic) is Type metadata && metadata.GetCustomAttributes<DisplayNameAttribute>().FirstOrDefault() is DisplayNameAttribute displayName)
			{
				customHeaderText = displayName.DisplayName;
				return true;
			}

			customHeaderText = null;
			return false;
		}

		private void Setup()
		{
			#if DEV_MODE
			using var x = setupMarker.Auto();
			#endif

			initializerBackgroundStyle = new GUIStyle(EditorStyles.helpBox);
			noInitializerBackgroundStyle = new GUIStyle(EditorStyles.label);

			var padding = initializerBackgroundStyle.padding;
			padding.left += 14;
			noInitializerBackgroundStyle.padding = padding;
			initializerBackgroundStyle.padding = padding;

			initArgsFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			initArgsFoldoutStyle.fontStyle = FontStyle.Bold;

			noInitArgsLabelStyle = new GUIStyle(EditorStyles.label);
			noInitArgsLabelStyle.fontStyle = FontStyle.Bold;

			initArgsFoldoutBackgroundStyle = "RL Header";
			initArgsFoldoutBackgroundStyle.fixedHeight = 24f;

			addInitializerIcon.image = EditorGUIUtility.TrIconContent("Toolbar Plus").image;
			contextMenuIcon.image = EditorGUIUtility.IconContent("_Menu").image;
			nullGuardDisabledIcon.image = EditorGUIUtility.IconContent("DebuggerDisabled").image;
			nullGuardPassedIcon.image = PancakeEditor.Common.EditorResources.IconNullGuardPassed(PancakeEditor.Common.Uniform.Theme);
			nullGuardPassedWithValueProviderValueMissing.image = EditorGUIUtility.IconContent("DebuggerAttached").image;
			nullGuardFailedIcon.image = EditorGUIUtility.IconContent("DebuggerEnabled").image;
			initStateUninitializedIcon.image = EditorGUIUtility.IconContent("TestIgnored").image;
			initStateInitializingIcon.image = EditorGUIUtility.IconContent("Loading").image;
			initStateInitializedIcon.image = PancakeEditor.Common.EditorResources.IconNullGuardPassed(PancakeEditor.Common.Uniform.Theme);
			initStateFailedIcon.image = EditorGUIUtility.IconContent("TestFailed").image;
			
			servicesHiddenIcon.image = EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image;
			servicesShownIcon.image = EditorGUIUtility.IconContent("animationvisibilitytoggleon").image;
			
			#if DEV_MODE
			Debug.Assert(addInitializerIcon.image);
			Debug.Assert(contextMenuIcon.image);
			Debug.Assert(nullGuardDisabledIcon.image);
			Debug.Assert(nullGuardPassedWithValueProviderValueMissing.image);
			Debug.Assert(nullGuardFailedIcon.image);
			Debug.Assert(initStateUninitializedIcon.image);
			Debug.Assert(initStateInitializingIcon.image);
			Debug.Assert(initStateInitializedIcon.image);
			Debug.Assert(initStateFailedIcon.image);
			Debug.Assert(servicesHiddenIcon.image);
			Debug.Assert(servicesShownIcon.image);
			#endif

			if(LayoutUtility.NowDrawing)
			{
				LayoutUtility.NowDrawing.Repaint();
			}
		}

		private void UpdateTooltips(bool hasInitializers, Object firstInitializer)
		{
			addInitializerIcon.tooltip = Target is Animator ? AddStateMachineInitializerTooltip : AddInitializerTooltip;
			contextMenuIcon.tooltip = firstInitializer ? TypeUtility.ToStringNicified(firstInitializer.GetType()) : ""; 
			
			if(hasServiceParameters)
			{
				servicesShownIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initParametersAreServices, allParametersAreServices, hasInitializers, servicesShown: true);
				servicesHiddenIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initParametersAreServices, allParametersAreServices, hasInitializers, servicesShown: false);
			}
			else
			{
				servicesShownIcon.tooltip = "";
				servicesHiddenIcon.tooltip = "";
			}

			headerLabel.tooltip = GetInitArgumentsTooltip(initParameterTypes, initParametersAreServices, hasInitializers);
		}

		public void OnInspectorGUI()
		{
			#if DEV_MODE
			using var x = onInspectorGUIMarker.Auto();
			#endif

			if(initializerBackgroundStyle is null)
			{
				Setup();
			}

			NowDrawing = this;
			bool hierarchyModeWas = EditorGUIUtility.hierarchyMode;

			ownerSerializedObject.Update();

			try
			{
				GetInitializersOnTargets(out bool hasInitializers, out Object firstInitializer);

				if(shouldUpdateInitArgumentDependentState)
				{
					UpdateInitArgumentDependentState(hasInitializers, firstInitializer);
				}

				bool mixedInitializers = false;
				if(hasInitializers)
				{
					for(int i = 0, initializerCount = initializers.Length; i < initializerCount; i++)
					{
						if(!initializers[i])
						{
							mixedInitializers = true;
							break;
						}
					}
				}
				// Don't draw Init GUI in play mode unless the target has an Initializer
				// (possible if the GameObject is inactive).
				else if(Application.isPlaying)
				{
					return;
				}

				if(!hadInitializerLastFrame.HasValue || hadInitializerLastFrame.Value != hasInitializers)
				{
					hadInitializerLastFrame = hasInitializers;
					UpdateInitArgumentDependentState(hasInitializers, firstInitializer);
				}

				EditorGUIUtility.hierarchyMode = true;
				EditorGUI.indentLevel = 0;

				var firstInitializerEditorOnly = firstInitializer as IInitializerEditorOnly;
				var hasInitializerThatProvidesCustomInitArguments = firstInitializerEditorOnly is { ProvidesCustomInitArguments: true };

				HelpBoxMessageType helpBoxMessage;
				if(mixedInitializers)
				{
					helpBoxMessage = HelpBoxMessageType.None;
				}
				else if(IsGameObjectInactive())
				{
					if(hasInitializers)
					{
						helpBoxMessage = CanInitializerInitInactiveTarget(firstInitializerEditorOnly)
									? HelpBoxMessageType.TargetInitializedWhenDeserialized
									: HelpBoxMessageType.TargetInitializedWhenBecomesActive;
					}
					else
					{
						helpBoxMessage = IsInitializableUnableToInitSelfWhenInactive()
									? HelpBoxMessageType.TargetInitializedWhenBecomesActive
									: HelpBoxMessageType.TargetInitializedWhenDeserialized;
					}
				}
				// Also show the help box if an InactiveInitializer is attached to a component on an active GameObject.
				else if(CanInitializerInitInactiveTarget(firstInitializerEditorOnly))
				{
					helpBoxMessage = HelpBoxMessageType.TargetInitializedWhenDeserialized;
				}
				else
				{
					helpBoxMessage = HelpBoxMessageType.None;
				}

				bool drawInitHeader = !string.IsNullOrEmpty(headerLabel.text);
				bool servicesShown = ServicesShown;
				bool isCollapsible = drawInitHeader && (helpBoxMessage != HelpBoxMessageType.None || (initParameterTypes.Length > 0 && (!allParametersAreServices || servicesShown)));

				if(drawInitHeader)
				{
					var backgroundStyle = isCollapsible ? initializerBackgroundStyle : noInitializerBackgroundStyle;
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(-14f);
					EditorGUILayout.BeginVertical(backgroundStyle);
				}

				var labelStyle = isCollapsible ? initArgsFoldoutStyle : noInitArgsLabelStyle;
				var headerRect = EditorGUILayout.GetControlRect(false, 20f, labelStyle);

				bool isUnfolded;
				// if there is no control for toggling collapsed state, then always draw contents
				if(!isCollapsible)
				{
					isUnfolded = true;
				}
				else if(CanUseInitializersToStoreUnfoldedState(hasInitializers))
				{
					isUnfolded = InternalEditorUtility.GetIsInspectorExpanded(firstInitializer);
				}
				else
				{
					isUnfolded = EditorPrefsUtility.GetBoolUserData(GetIsUnfoldedUserDataType(), IsUnfoldedUserDataKey);
				}

				if(isCollapsible)
				{
					headerRect.y -= 2f;
				}

				var foldoutRect = headerRect;
				bool drawAddInitializerButton = !hasInitializers;
				bool drawContextMenuButton = !drawAddInitializerButton && isResponsibleForInitializerEditorLifetime;
				var targetProperty = ownerSerializedObject?.FindProperty("target");

				// When initializer has no target, or target is on another GameObject, then draw target field on the Initializer.
				// Otherwise, Initializer will be drawn embedded inside the client's editor, and we don't want to draw the target field.
				bool drawTargetField = targetProperty != null && firstInitializer is Component initializerComponent and IInitializer init
					&& (!init.Target || (init.Target is Component clientComponent && clientComponent.gameObject != initializerComponent.gameObject));

				var drawNullGuard = drawAddInitializerButton ? targetImplementsIArgs : firstInitializerEditorOnly is { ShowNullArgumentGuard: true };
				
				Rect targetFieldRect = headerRect;

				if(drawTargetField)
				{
					var labelWidth = labelStyle.CalcSize(headerLabel).x;
					targetFieldRect.x += labelWidth;
					targetFieldRect.width -= labelWidth;
					targetFieldRect.height = EditorGUIUtility.singleLineHeight;
					
					if(isResponsibleForInitializerEditorLifetime)
					{
						foldoutRect.x -= 12f;
					}

					foldoutRect.xMax = targetFieldRect.x;
				}
				else if(!isResponsibleForInitializerEditorLifetime)
				{
					foldoutRect.x -= 12f;
					foldoutRect.width -= 38f + IconWidth;
				}
				else
				{
					foldoutRect.width -= 42f + IconWidth;
				}
				
				// Leave space to draw the Null Argument Guard icon
				if(drawNullGuard)
				{
					targetFieldRect.width -= IconWidth;
				}

				// Leave space to draw the [+] or the kebab menu icon
				if(drawAddInitializerButton || drawContextMenuButton)
				{
					targetFieldRect.width -= IconWidth;
				}

				// Leave space to draw the service visibility icon
				if(hasServiceParameters)
				{
					targetFieldRect.width -= IconWidth;
				}

				foldoutRect.y -= 1f;

				var addInitializerOrContextMenuRect = headerRect;
				addInitializerOrContextMenuRect.x += addInitializerOrContextMenuRect.width - IconWidth;
				addInitializerOrContextMenuRect.width = IconWidth;
				addInitializerOrContextMenuRect.height = IconWidth;
				if(hasInitializers)
				{
					addInitializerOrContextMenuRect.y -= 1f;
				}

				if(drawAddInitializerButton)
				{
					addInitializerTooltipOnly.tooltip = addInitializerIcon.tooltip;
					if(GUI.Button(addInitializerOrContextMenuRect, addInitializerTooltipOnly, EditorStyles.label))
					{
						if(OnAddInitializerButtonPressedOverride != null)
						{
							OnAddInitializerButtonPressedOverride.Invoke(addInitializerOrContextMenuRect);
							return;
						}

						AddInitializer(addInitializerOrContextMenuRect);
					}
				}
				else if(!isResponsibleForInitializerEditorLifetime)
				{
					addInitializerOrContextMenuRect.x += addInitializerOrContextMenuRect.width;
				}
				else if(GUI.Button(addInitializerOrContextMenuRect, GUIContent.none, EditorStyles.label))
				{
					OnInitializerContextMenuButtonPressed(firstInitializer, mixedInitializers, addInitializerOrContextMenuRect);
				}

				var nullGuardIconRect = addInitializerOrContextMenuRect;
				nullGuardIconRect.x -= addInitializerOrContextMenuRect.width;
				nullGuardIconRect.x += 4f;
				NullGuardResult nullGuardResult;
				if(!allParametersAreServices && hasInitializerThatProvidesCustomInitArguments)
				{
					nullGuardResult = NullGuardResult.Passed;
					try
					{
						for(int i = 0, initializerCount = initializers.Length; i < initializerCount; i++)
						{
							if(initializers[i] is IInitializerEditorOnly initializerEditorOnly)
							{
								nullGuardResult = initializerEditorOnly.EvaluateNullGuard();
								if(nullGuardResult != NullGuardResult.Passed)
								{
									break;
								}
							}
						}
					}
					catch
					{
						nullGuardResult = NullGuardResult.ValueProviderException;
					}
				}
				else
				{
					nullGuardResult = allParametersAreServices ? NullGuardResult.Passed : NullGuardResult.ValueMissing;
				}

				if(!nullGuardResultLastFrame.HasValue || nullGuardResultLastFrame.Value != nullGuardResult)
				{
					nullGuardResultLastFrame = nullGuardResult;
					UpdateInitArgumentDependentState(hasInitializers, firstInitializer);
				}

				var nullGuard = !drawNullGuard? NullArgumentGuard.None : firstInitializerEditorOnly?.NullArgumentGuard ?? GetNullArgumentGuardFlags(Target);
				bool isAsset = FirstGameObject?.IsAsset(true) ?? !Application.isPlaying;
				var nullGuardDisabled = !nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning);
				
				if(nullGuardResult is NullGuardResult.ValueMissing && !nullGuardDisabled)
				{
					helpBoxMessage |= HelpBoxMessageType.TargetHasMissingDependencies;
				}

				if(drawNullGuard && GUI.Button(nullGuardIconRect, GUIContent.none, EditorStyles.label))
				{
					OnInitializerNullGuardButtonPressed(nullGuard, nullGuardIconRect, CanThrowRuntimeExceptions(hasInitializers, targetCanSelfInitializeWithoutInitializer));
				}

				var serviceVisibilityIconRect = nullGuardIconRect;
				if(drawNullGuard)
				{
					serviceVisibilityIconRect.x -= nullGuardIconRect.width + 2f;
				}

				if(hasServiceParameters && GUI.Button(serviceVisibilityIconRect, GUIContent.none, EditorStyles.label))
				{
					servicesShown = !servicesShown;
					ServicesShown = servicesShown;
					EditorPrefs.SetBool(ServiceVisibilityEditorPrefsKey, servicesShown);
				}
				
				var iconSizeWas = EditorGUIUtility.GetIconSize();
				// Helps help box warning icons and toolbar icons to be sharp
				EditorGUIUtility.SetIconSize(IconSize);

				if(isUnfolded)
				{
					if(helpBoxMessage != HelpBoxMessageType.None)
					{
						if(drawAddInitializerButton && helpBoxMessage.HasFlag(HelpBoxMessageType.TargetInitializedWhenBecomesActive))
						{
							DrawHelpBoxes(helpBoxMessage & ~HelpBoxMessageType.TargetInitializedWhenBecomesActive, hasInitializerThatProvidesCustomInitArguments);
							DrawInactiveInitializerHelpBox(HelpBoxMessageType.TargetInitializedWhenBecomesActive);
						}
						else if(helpBoxMessage.HasFlag(HelpBoxMessageType.TargetInitializedWhenDeserialized))
						{
							DrawHelpBoxes(helpBoxMessage & ~HelpBoxMessageType.TargetInitializedWhenDeserialized, hasInitializerThatProvidesCustomInitArguments);
							DrawInactiveInitializerHelpBox(HelpBoxMessageType.TargetInitializedWhenDeserialized);
						}
						else
						{
							DrawHelpBoxes(helpBoxMessage, hasInitializerThatProvidesCustomInitArguments);
						}
					}

					GUILayout.Space(3f);

					if(hasInitializerThatProvidesCustomInitArguments)
					{
						if(!allParametersAreServices || servicesShown)
						{
							DrawInitializerArguments();
						}
					}
					else
					{
						for(int parameterIndex = 0; parameterIndex < initParameterTypes.Length; parameterIndex++)
						{
							Type parameterType = initParameterTypes[parameterIndex];
							var isService = initParametersAreServices[parameterIndex];
							if(!servicesShown && isService)
							{
								continue;
							}
							
							var rect = EditorGUILayout.GetControlRect();
							var prefixLabel = TypeUtility.ToStringNicified(parameterType);
							var controlRect = EditorGUI.PrefixLabel(rect, new(prefixLabel));

							if(isService)
							{
								bool clicked = EditorServiceTagUtility.Draw(controlRect);
								if(clicked)
								{
									if(Event.current.button == 1)
									{
										#if DEV_MODE
										Debug.Log($"OpenContextMenuForServiceOfClient({Target}, {prefixLabel})");
										#endif
										EditorServiceTagUtility.OpenContextMenuForServiceOfClient(Target, parameterType, rect);
									}
									else if(!EditorServiceTagUtility.PingServiceFor(Target, parameterType))
									{
										EditorApplication.ExecuteMenuItem("Window/General/Console");
										var type = ServiceAttributeUtility.definingTypes.TryGetValue(parameterType, out var serviceInfo) ? serviceInfo.serviceOrProviderType ?? parameterType : parameterType;
										if(parameterType == type)
										{
											Debug.Log($"{TypeUtility.ToString(parameterType)} is registered as a service using the [Service] attribute. Could not locate its script asset.\nThis can happen when the name of the script does not match the type name.");
										}
										else
										{
											Debug.Log($"{TypeUtility.ToString(parameterType)} is registered as a service using the [Service] attribute in the class {TypeUtility.ToString(type)}. Could not locate its script asset.\nThis can happen when the name of the script does not match the type name.");
										}
									}

									LayoutUtility.ExitGUI();
								}

								continue;
							}

							var guiColorWas = GUI.color;

							if(!nullGuardDisabled && InitializerEditorUtility.TryGetTintForNullGuardResult(NullGuardResult.ValueMissing, out Color valueMissingColor))
							{
								GUI.color = valueMissingColor;
							}

							if(rect.Contains(Event.current.mousePosition))
							{
								notFoundLabel.tooltip = string.Format(notFoundTooltip, TypeUtility.ToString(parameterType));
							}

							EditorGUI.LabelField(controlRect, notFoundLabel);
							GUI.color = guiColorWas;
						}
					}
				}

				if(drawInitHeader)
				{
					DrawInitHeader(headerRect, ref foldoutRect, labelStyle, isUnfolded, isCollapsible, hasInitializers, mixedInitializers, firstInitializer, nullGuardResult);
				}

				if(drawAddInitializerButton)
				{
					GUI.Label(addInitializerOrContextMenuRect, addInitializerIcon);
				}
				else if(drawContextMenuButton)
				{
					GUI.Label(addInitializerOrContextMenuRect, contextMenuIcon);
				}

				if(drawNullGuard)
				{
					var guiColorWas = GUI.color;
					Color guiColor = guiColorWas;
					GUIContent nullGuardIcon = GetNullGuardIconContent(ref nullGuardIconRect, ref guiColor);
					GUI.Label(nullGuardIconRect, nullGuardIcon);
					GUI.color = guiColorWas;
				}

				if(hasServiceParameters)
				{
					var serviceVisibilityIcon = servicesShown ? servicesShownIcon : servicesHiddenIcon;
					GUI.Label(serviceVisibilityIconRect, serviceVisibilityIcon);
				}

				EditorGUIUtility.SetIconSize(iconSizeWas);

				if(drawTargetField)
				{
					if(targetFieldRect.width > EditorGUIUtility.singleLineHeight)
					{
						bool isInitializable;
						if(initializables.Length > 0)
						{
							isInitializable = initializables.Length > 0 && InitializerEditorUtility.IsInitializable(initializables[0]);
						}
						else if(targets.Length > 0 && targets[0] is IInitializer initializer)
						{
							Type clientType = InitializerEditorUtility.GetClientType(initializer.GetType());
							isInitializable = Find.typesToWrapperTypes.ContainsKey(clientType) || InitializerEditorUtility.IsInitializable(clientType);
						}
						else
						{
							isInitializable = false;
						}

						InitializerEditorUtility.DrawClientField(targetFieldRect, targetProperty, GUIContent.none, isInitializable);
					}
				}
				
				GUIContent GetNullGuardIconContent(ref Rect nullGuardIconRect, ref Color guiColor)
				{
					if(!drawNullGuard)
					{
						return GUIContent.none;
					}
					
					GUIContent nullGuardIcon;

					if(!hasInitializers)
					{
						if((FirstGameObject?.IsAsset(true) ?? !Application.isPlaying) == false && Target is IInitializableEditorOnly initializable)
						{
							switch (initializable.InitState)
							{
								case InitState.Uninitialized:
									if(firstInitializerEditorOnly?.IsAsync ?? false)
									{
										nullGuardIcon = initStateUninitializedIcon;
										nullGuardIcon.tooltip = "Target is still being initialized asynchronously...";
									}
									else if(nullGuard.HasFlag(NullArgumentGuard.RuntimeException))
									{
										nullGuardIcon = initStateFailedIcon;
										nullGuardIcon.tooltip = "○ Target has not been initialized.";
									}
									else
									{
										nullGuardIcon = initStateUninitializedIcon;
										nullGuardIcon.tooltip = "Target has not been initialized.";
									}
									break;
								case InitState.Initializing:
									nullGuardIcon = initStateInitializingIcon;
									nullGuardIcon.tooltip = "Target initialization is in progress...";
									break;
								case InitState.Initialized:
									nullGuardIcon = initStateInitializedIcon;
									nullGuardIcon.tooltip = "◉️ Target has been initialized.";
									break;
								case InitState.Failed:
									nullGuardIcon = initStateFailedIcon;
									nullGuardIcon.tooltip = "○ Target initialization has failed.";
									break;
								default:
									throw new ArgumentOutOfRangeException(initializable.InitState.ToString());
							}
						}
						else if(!nullGuard.HasFlag(NullArgumentGuard.EditModeWarning) && !Application.isPlaying)
						{
							nullGuardIcon = nullGuardDisabledIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, false, targetCanSelfInitializeWithoutInitializer) + "\n\nNull argument guard is off.";
						}
						else if(allParametersAreServices)
						{
							if(anyParameterIsAsyncLoadedService)
							{
								nullGuardIcon = nullGuardPassedWithValueProviderValueMissing;
								nullGuardIcon.tooltip
									= GetTooltip(nullGuard, false, targetCanSelfInitializeWithoutInitializer)
									+ "\n\nAll arguments are services, but some of them are loaded asynchronously.\n\nAdding an Initializer is recommended for deferred initialization support, in case the service isn't ready yet when this client is loaded.";
							}
							else
							{
								nullGuardIcon = nullGuardPassedIcon;
								nullGuardIcon.tooltip
									= GetTooltip(nullGuard, false, targetCanSelfInitializeWithoutInitializer) +
									(targetCanSelfInitializeWithoutInitializer
									? "\n\nAll arguments are services.\n\nThe client will receive them automatically during initialization.\n\nAdding an Initializer is not necessary - unless there is a need to override some of the services for this particular client."
									: "\n\nAll arguments are services.\n\nThe client can use InitArgs.TryGet to acquire them during initialization, in which case adding an Initializer is not necessary - unless there is a need to override some of the services for this particular client");
							}
						}
						else
						{
							nullGuardIcon = nullGuardFailedIcon;

							const string suffix = "\n\n" +
							"<color=#ffd100>Missing argument detected!</color>\n\n" +
							"If this client does not need to receive Init arguments at runtime, you can disable the Null Argument Guard.\n\n" +
							"If a missing argument is a service that only becomes available at runtime, attach an Initializer to the component, and select 'Wait For Service' from the argument's dropdown.";
							nullGuardIcon.tooltip = GetTooltip(nullGuard, false, targetCanSelfInitializeWithoutInitializer) + suffix;
							return nullGuardIcon;
						}
					}

					else if(!isAsset && Target is IInitializableEditorOnly initializable)
					{
						switch (initializable.InitState)
						{
							case InitState.Uninitialized:
								if(firstInitializerEditorOnly?.IsAsync ?? false)
								{
									nullGuardIcon = initStateUninitializedIcon;
									nullGuardIcon.tooltip = "Target is still being initialized asynchronously...";
									return nullGuardIcon;
								}
								
								if(nullGuard.HasFlag(NullArgumentGuard.RuntimeException))
								{
									nullGuardIcon = initStateFailedIcon;
									nullGuardIcon.tooltip = "○ Target has not been initialized.";
									return nullGuardIcon;
								}
								
								nullGuardIcon = initStateUninitializedIcon;
								nullGuardIcon.tooltip = "Target has not been initialized.";
								return nullGuardIcon;
							case InitState.Initializing:
								nullGuardIcon = initStateInitializingIcon;
								nullGuardIcon.tooltip = "Target initialization is in progress...";
								return nullGuardIcon;
							case InitState.Initialized:
								nullGuardIcon = initStateInitializedIcon;
								nullGuardIcon.tooltip = "◉️ Target has been initialized.";
								return nullGuardIcon;
							case InitState.Failed:
								nullGuardIcon = initStateFailedIcon;
								nullGuardIcon.tooltip = "○ Target initialization has failed.";
								return nullGuardIcon;
							default:
								throw new ArgumentOutOfRangeException(initializable.InitState.ToString());
						}
					}
					
					if(nullGuardDisabled)
					{
						nullGuardIcon = nullGuardDisabledIcon;
						nullGuardIconRect.width -= 1f;
						nullGuardIconRect.height -= 1f;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nNull argument guard is off.";
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.ValueMissing)
					{
						const string suffix = "\n\n" +
						"<color=#ffd100>Missing argument detected!\n\n" +
						"If a missing argument is a service that only becomes available at runtime, select 'Wait For Service' from the its dropdown menu.\n\n" +
						"If null arguments should be allowed, then set the 'Null Argument Guard' option to 'None'.";
						
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + suffix;
						
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.InvalidValueProviderState)
					{
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nSome value providers have been configured invalidly and will not be able to provide a value at runtime.";
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.ClientNotSupported)
					{
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nSome value providers do not support the client and will not be able to provide a value at runtime.";
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.TypeNotSupported)
					{
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nSome value providers do not support the client's type and will not be able to provide a value at runtime.";
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.ValueProviderException)
					{
						nullGuardIcon = nullGuardFailedIcon;
						if(string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
						{
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nAn exception was encountered while trying to retrieve a value from one of the value providers.";
						}
						else
						{
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nAn exception was encountered while trying to retrieve a value from one of the value providers:\n" + firstInitializerEditorOnly.NullGuardFailedMessage;
						}
						return nullGuardIcon;
					}

					if(nullGuardResult == NullGuardResult.ClientException)
					{
						nullGuardIcon = nullGuardFailedIcon;
						if(string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
						{
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nAn exception was thrown by the client during its initialization.";
						}
						else
						{
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nAn exception was thrown by the client during its initialization:\n" + firstInitializerEditorOnly.NullGuardFailedMessage;
						}
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.ValueProviderValueMissing)
					{
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nSome value providers will not be able to provide a value at runtime.";
						return nullGuardIcon;
					}
					
					if(nullGuardResult == NullGuardResult.ValueProviderValueNullInEditMode)
					{
						if(!string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage) && !Application.isPlaying)
						{
							firstInitializerEditorOnly.NullGuardFailedMessage = "";
						}

						nullGuardIcon = nullGuardPassedWithValueProviderValueMissing;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nSome value providers are not able to provide a value at this moment, but may be able to do so at runtime.";
						guiColor = new Color(0.6f, 0.6f, 1f, 1f);
						return nullGuardIcon;
					}
					
					if(hasInitializers)
					{
						if(!string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
						{
							firstInitializerEditorOnly.NullGuardFailedMessage = "";
						}

						nullGuardIcon = nullGuardPassedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard, true, targetCanSelfInitializeWithoutInitializer) + "\n\nAll arguments provided.";
						return nullGuardIcon;
					}
					
					nullGuardIcon = nullGuardPassedIcon;
					nullGuardIcon.tooltip = GetTooltip(nullGuard, false, targetCanSelfInitializeWithoutInitializer) + "\n\nAll arguments provided.";
					return nullGuardIcon;
				}

				if(drawInitHeader)
				{
					EditorGUILayout.EndVertical();
					GUILayout.Space(-6f);
					EditorGUILayout.EndHorizontal();
				}
			}
			finally
			{
				if(ownerSerializedObject.IsValid())
				{
					ownerSerializedObject.ApplyModifiedProperties();
				}

				EditorGUIUtility.hierarchyMode = hierarchyModeWas;
				NowDrawing = null;
			}
		}

		private static bool CanThrowRuntimeExceptions(bool hasInitializers, bool targetCanSelfInitializeWithoutInitializer) => hasInitializers || targetCanSelfInitializeWithoutInitializer;

		private bool CanUseInitializersToStoreUnfoldedState(bool hasInitializers) => hasInitializers && initializables.Length > 0;
		private Type GetIsUnfoldedUserDataType() => Target?.GetType() ?? typeof(Object);

		private void DrawInactiveInitializerHelpBox(HelpBoxMessageType message)
		{
			bool usingOnAfterDeserialize = message == HelpBoxMessageType.TargetInitializedWhenDeserialized;
			var helpBoxText = usingOnAfterDeserialize ? ClientInitializedDuringOnAfterDeserializeTextWithSpaceForButton : ClientInitializedWhenBecomesActiveTextWithSpaceForButton;
			DrawHelpBox(MessageType.Info, new(helpBoxText), height: 75f);

			var helpBoxRect = GUILayoutUtility.GetLastRect();
			var toggleRect = helpBoxRect;
			const float TogglesLeftOffset = 30f;
			const float textRightRightOffset = 10f;
			toggleRect.x += TogglesLeftOffset;
			float buttonMaxWidth = helpBoxRect.width - 45f;
			toggleRect.width -= TogglesLeftOffset + textRightRightOffset;
			toggleRect.y += 8f;
			toggleRect.height = EditorGUIUtility.singleLineHeight;

			var buttonStyle = EditorStyles.radioButton;

			buttonStyle.CalcMinMaxWidth(useOnAfterDeserializeButtonLabel, out float buttonOptimalWidth, out _);
			toggleRect.width = Mathf.Min(buttonOptimalWidth, buttonMaxWidth);
			if(GUI.Toggle(toggleRect, usingOnAfterDeserialize, useOnAfterDeserializeButtonLabel, buttonStyle) && !usingOnAfterDeserialize)
			{
				InitializerEditorUtility.AddInitializer(targets, typeof(InactiveInitializer));
			}

			toggleRect.y += toggleRect.height + 2f;
			buttonStyle.CalcMinMaxWidth(useAwakeButtonLabel, out buttonOptimalWidth, out _);
			toggleRect.width = Mathf.Min(buttonOptimalWidth, buttonMaxWidth);
			if(GUI.Toggle(toggleRect, !usingOnAfterDeserialize, useAwakeButtonLabel, buttonStyle) && usingOnAfterDeserialize)
			{
				LayoutUtility.OnLayoutEvent(RemoveInitializerFromAllTargets);
			}
		}

		private static void DrawHelpBoxes(HelpBoxMessageType message, bool hasInitializerThatProvidesCustomInitArguments)
		{
			if(message.HasFlag(HelpBoxMessageType.TargetHasMissingDependencies))
			{
				var tooltip = hasInitializerThatProvidesCustomInitArguments ? SomeDependenciesMissingTooltipHasInitializer : SomeDependenciesMissingTooltipNoInitializer;
				SomeDependenciesMissingLabel.tooltip = tooltip;
				DrawHelpBox(MessageType.Warning, SomeDependenciesMissingLabel, HelpLinkLabel, HelpLinkURL);
			}
			
			if(message.HasFlag(HelpBoxMessageType.TargetInitializedWhenBecomesActive))
			{
				DrawHelpBox(MessageType.Info, ClientInitializedWhenBecomesActiveLabel);
			}
			else if(message.HasFlag(HelpBoxMessageType.TargetInitializedWhenDeserialized))
			{
				DrawHelpBox(MessageType.Info, ClientInitializedDuringOnAfterDeserializeLabel);
			}
		}

		private static void DrawHelpBox(MessageType type, GUIContent content, GUIContent linkLabel = null, string linkUrl = null, float height = 30f)
		{
			GUILayout.Space(3f);
			var helpBoxRect = GUILayoutUtility.GetRect(Screen.width - 55f, height, EditorStyles.helpBox);
			GUI.Label(helpBoxRect, "", EditorStyles.helpBox);
			var clipRect = helpBoxRect;
			clipRect.width -= 2f;
			GUI.BeginClip(clipRect);
			helpBoxRect.x = 0f;
			helpBoxRect.y = 0f;

			var iconRect = helpBoxRect;
			iconRect.x += 5f;
			iconRect.width = IconWidth;
			GUIContent icon = type switch
			{
				MessageType.Error => Styles.ErrorIcon,
				MessageType.Warning => Styles.WarningIcon,
				_ => Styles.InfoIcon,
			};
			icon.tooltip = content.tooltip;
			GUI.Label(iconRect, icon);
			icon.tooltip = "";

			var textRect = helpBoxRect; 
			textRect.x += 25f;
			textRect.width -= 25f;

			var textWidth = EditorStyles.label.CalcSize(content).x;
			textRect.width = textWidth + 2f;
			GUI.Label(textRect, content);
			content.tooltip = "";

			if(linkLabel is not null)
			{
				var helpLinkRect = textRect;
				helpLinkRect.x += textWidth;
				helpLinkRect.y += 5f;
				helpLinkRect.height = EditorGUIUtility.singleLineHeight;
				helpLinkRect.width = EditorStyles.label.CalcSize(linkLabel).x;
				if(EditorGUI.LinkButton(helpLinkRect, linkLabel))
				{
					Application.OpenURL(linkUrl);
				}
			}

			GUI.EndClip();
		}

		bool IsGameObjectInactive()
		{
			if(gameObjects.Length == 0)
			{
				return false;
			}

			// activeInHierarchy is always false for prefab assets, using activeSelf is more reliable.
			for(var transform = gameObjects[0].transform; transform; transform = transform.transform.parent)
			{
				if(!transform.gameObject.activeSelf)
				{
					return true;
				}
			}

			return false;
		}

		bool CanInitializerInitInactiveTarget([AllowNull] IInitializerEditorOnly initializerEditorOnly) => initializerEditorOnly != null && initializerEditorOnly.CanInitTargetWhenInactive;
		bool IsInitializableUnableToInitSelfWhenInactive() => initializables.Length == 0 || initializables[0] is not IInitializableEditorOnly initializableEditorOnly || !initializableEditorOnly.CanInitSelfWhenInactive;

		private void DrawInitHeader(Rect headerRect, ref Rect foldoutRect, GUIStyle labelStyle, bool isUnfolded, bool isCollapsible, bool hasInitializers, bool mixedInitializers, Object firstInitializer, NullGuardResult nullGuardResult)
		{
			var backgroundRect = headerRect;
			backgroundRect.y -= 3f;
			backgroundRect.x -= 18f;
			backgroundRect.width += 22f;

			var remainingRect = headerRect;
			float xMax = headerRect.xMax - IconWidth - 3f;
			if(hasServiceParameters)
			{
				xMax -= IconWidth;
			}

			if(isResponsibleForInitializerEditorLifetime)
			{
				xMax -= IconWidth;
			}

			remainingRect.x += EditorStyles.label.CalcSize(headerLabel).x + 15f;
			remainingRect.xMax = xMax;
			AfterHeaderGUI?.Invoke(remainingRect, initializerEditor);

			var foldoutClickableRect = foldoutRect;
			foldoutClickableRect.x -= 5f;
			foldoutClickableRect.width += 5f;
			foldoutClickableRect.x -= 12f;
			foldoutClickableRect.width += 12f;
			
			bool setUnfolded = isUnfolded;
			if(Event.current.type == EventType.MouseDown && foldoutClickableRect.Contains(Event.current.mousePosition))
			{
				if(Event.current.button == 0)
				{
					setUnfolded = isCollapsible ? !isUnfolded : isUnfolded;
				}
				else if(Event.current.button == 1)
				{
					if(hasInitializers)
					{
						OnInitializerContextMenuButtonPressed(firstInitializer, mixedInitializers, null);
					}
					else if(Target is { } target && target)
					{
						var initializableType = target.GetType();
						var script = target is MonoBehaviour monoBehaviour ? MonoScript.FromMonoBehaviour(monoBehaviour) : Find.Script(initializableType); 
						var menu = new GenericMenu();
						menu.AddItem(new("Show Init Section"), true, () => ToggleHideInitSection(script, initializableType));
						menu.ShowAsContext();
					}
				}

				Event.current.Use();
			}

			bool guiWasEnabled = GUI.enabled;
			if(Event.current.type is EventType.Repaint)
			{
				initArgsFoldoutBackgroundStyle.Draw(backgroundRect, false, false, false, false);
			}

			foldoutRect.x -= 12f;
			
			if(initParameterTypes.Length is 0 || nullGuardResult is not NullGuardResult.Passed and not NullGuardResult.ValueProviderValueNullInEditMode)
			{
				GUI.enabled = false;
			}

			if(Event.current.type is EventType.Repaint)
			{
				labelStyle.Draw(foldoutRect, headerLabel, GUIUtility.GetControlID(FocusType.Passive), isUnfolded);
			}

			GUI.enabled = guiWasEnabled;

			if(setUnfolded != isUnfolded && isCollapsible)
			{
				if(CanUseInitializersToStoreUnfoldedState(hasInitializers))
				{
					for(int i = 0, count = initializers.Length; i < count; i++)
					{
						var initializer = initializers[i];
						if(initializer)
						{
							#if DEV_MODE && DEBUG_SET_UNFOLDED
							Debug.Log($"SetIsInspectorExpanded({initializer.GetType().Name}, {setUnfolded})");
							#endif
							
							LayoutUtility.OnLayoutEvent(() =>
							{
								if(initializer)
								{
									InternalEditorUtility.SetIsInspectorExpanded(initializer, setUnfolded);
								}
							});
						}
					}
				}
				else
				{
					Type userDataType = GetIsUnfoldedUserDataType();

					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"SetUserData({userDataType.Name}, {setUnfolded})");
					#endif

					LayoutUtility.OnLayoutEvent(() =>
					{
						EditorPrefsUtility.SetUserData(userDataType, IsUnfoldedUserDataKey, setUnfolded);
					});
				}

				LayoutUtility.ExitGUI();
			}

			#if DEV_MODE
			if(Event.current.alt)
			{
				var color = Color.yellow;
				color.a = 0.5f;
				EditorGUI.DrawRect(foldoutClickableRect, color);
			}
			#endif
		}

		private void DrawInitializerArguments()
		{
			if(!initializerEditor)
			{
				isResponsibleForInitializerEditorLifetime = true;
				Editor.CreateCachedEditor(initializers, null, ref initializerEditor);
				if(initializerEditor is InitializerEditor setup)
				{
					setup.Setup(this);
				}
			}

			if(initializerEditor is InitializerEditor internalInitializerEditor)
			{
				var nowDrawing = LayoutUtility.NowDrawing;
				internalInitializerEditor.OnBeforeNestedInspectorGUI();
				internalInitializerEditor.DrawArgumentFields();
				internalInitializerEditor.OnAfterNestedInspectorGUI(nowDrawing);
			}
			else
			{
				initializerEditor.OnNestedInspectorGUI();
			}
		}

		private string GetInitArgumentsTooltip([DisallowNull] Type[] initParameterTypes, bool[] initParametersAreServices, bool hasInitializers)
		{
			int count = initParameterTypes.Length;

			var sb = new StringBuilder();

			if(count > 0)
			{
				if((allParametersAreServices && targetCanSelfInitializeWithoutInitializer) || (hasInitializers && nullGuardResultLastFrame.GetValueOrDefault(NullGuardResult.Passed) == NullGuardResult.Passed))
				{
					sb.Append("The client will receive ");
					sb.Append(count switch
					{
						1 => "one argument",
						2 => "two arguments",
						3 => "three arguments",
						4 => "four arguments",
						5 => "five arguments",
						6 => "six arguments",
						7 => "seven arguments",
						8 => "eight arguments",
						9 => "nine arguments",
						10 => "ten arguments",
						11 => "eleven arguments",
						12 => "twelve arguments",
						_ => $"{count} arguments"
					});

					sb.Append(" during initialization:");
				}
				else if(allParametersAreServices && targetImplementsIArgs && !hasInitializers)
				{
					sb.Append(count switch
					{
						1 => "One argument is",
						2 => "Both arguments are",
						3 => "All three arguments are",
						4 => "All four arguments are",
						5 => "All five arguments are",
						6 => "All six arguments are",
						7 => "All seven arguments are",
						8 => "All eight arguments are",
						9 => "All nine arguments are",
						10 => "All ten arguments are",
						11 => "All eleven arguments are",
						12 => "All twelve arguments are",
						_ => $"All {count} arguments are"
					});

					sb.Append(" available for the client:");
				}
				else
				{
					sb.Append("The client can receive ");
					sb.Append(count switch
					{
						1 => "one argument",
						2 => "two arguments",
						3 => "three arguments",
						4 => "four arguments",
						5 => "five arguments",
						6 => "six arguments",
						7 => "seven arguments",
						8 => "eight arguments",
						9 => "nine arguments",
						10 => "ten arguments",
						11 => "eleven arguments",
						12 => "twelve arguments",
						_ => $"{count} arguments"
					});

					sb.Append(" during initialization:");
				}
				
				for(int i = 0; i < count; i++)
				{
					sb.Append('\n');
					sb.Append(i + 1);
					sb.Append(". ");
					sb.Append(TypeUtility.ToString(initParameterTypes[i]));
					if(initParametersAreServices[i])
					{
						sb.Append(" <color=grey>(Service)</color>");
					}
				}
			}

			if(Target is IInitializer initializer)
			{
				if(!initializer.Target)
				{
					if(sb.Length > 0)
					{
						sb.Append("\n\n");
					}

					sb.Append("Client component will be attached to the game object at runtime.");
				}
				else if(initializer.Target is Component targetComponent && (initializer is not Component initializerComponent || initializerComponent.gameObject != targetComponent.gameObject))
				{
					sb.Append("\n\nClient component will be instantiated at runtime.");
				}
			}

			return sb.ToString();
		}

		private string GetServiceVisibilityTooltip([DisallowNull] Type[] initParameterTypes, [DisallowNull] bool[] initServiceParameters, bool allParametersAreServices, bool hasInitializers, bool servicesShown)
		{
			var sb = new StringBuilder();

			int serviceCount = initServiceParameters.Count(b => b);
			sb.Append(serviceCount switch
			{
				1 => "One service argument is",
				2 => "Two service arguments are",
				3 => "Three service arguments are",
				4 => "Four service arguments are",
				5 => "Five service arguments are",
				6 => "Six service arguments are",
				_ => serviceCount + " service arguments are"
			});

			sb.Append(servicesShown ? " shown:" : " hidden:");

			for(int i = 0, count = initParameterTypes.Length; i < count; i++)
			{
				if(initServiceParameters[i])
				{
					sb.Append("\n ");
					sb.Append(TypeUtility.ToString(initParameterTypes[i]));
				}
			}

			if(hasInitializers || allParametersAreServices)
			{
				sb.Append("\n\nThese services can be provided automatically during initialization.");
			}

			return sb.ToString();
		}

		private static string GetTooltip(NullArgumentGuard guard, bool hasInitializers, bool targetCanSelfInitializeWithoutInitializer)
		{
			return CanThrowRuntimeExceptions(hasInitializers, targetCanSelfInitializeWithoutInitializer) 
			? guard switch
			{
				NullArgumentGuard.EditModeWarning => "Null Argument Guard:\n◉️ Edit Mode Warning\n○ Runtime Exception",
				NullArgumentGuard.RuntimeException => "Null Argument Guard:\n○ Edit Mode Warning\n◉️ Runtime Exception",
				NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException => "Null Argument Guard:\n◉️ Edit Mode Warning\n◉️ Runtime Exception",
				_ => "Null Argument Guard:\n○ Edit Mode Warning\n○ Runtime Exception"
			}
			:  guard switch
			{
				NullArgumentGuard.EditModeWarning => "Null Argument Guard:\n◉️ Edit Mode Warning",
				_ => "Null Argument Guard:\n○ Edit Mode Warning"
			};
		}

		/// <param name="canThrowRuntimeExceptions">
		/// If component does not have an Initializer and does not derive from MonoBehaviour{T...} etc., then the only type of
		/// warning that can be enabled is showing a warning icon in the Inspector in Edit Mode. 
		/// </param>
		private void OnInitializerNullGuardButtonPressed(NullArgumentGuard nullGuard, Rect nullGuardIconRect, bool canThrowRuntimeExceptions)
		{
			var menu = new GenericMenu();

			switch(Event.current.button)
			{
				case 0:
					menu.AddItem(new("None"), nullGuard is NullArgumentGuard.None, () => SetNullArgumentGuardFlags(NullArgumentGuard.None));

					menu.AddItem(new("Edit Mode Warning"), nullGuard.IsEnabled(NullArgumentGuard.EditModeWarning), ()=> Toggle(NullArgumentGuard.EditModeWarning));

					if(canThrowRuntimeExceptions)
					{
						menu.AddItem(new("Runtime Exception"), nullGuard.IsEnabled(NullArgumentGuard.RuntimeException), () => Toggle(NullArgumentGuard.RuntimeException));
					}
					else // for now these features are not supported without an initializer
					{
						menu.AddDisabledItem(new("Runtime Exception"), false);
					}

					menu.AddItem(new("All"), nullGuard is (NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException), () => SetNullArgumentGuardFlags(NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException));
					break;
				case 1:
					menu.AddItem(new("Debug"), false, ()=> EditorApplication.ExecuteMenuItem(ServicesWindow.MenuItemName));
					menu.AddItem(new("Help"), false, ()=> Application.OpenURL("https://docs.sisus.co/init-args/common-problems-solutions/client-not-receiving-services/"));
					break;
				default:
					return;
			}

			void Toggle(NullArgumentGuard flag) => SetNullArgumentGuardFlags(nullGuard.WithFlagToggled(flag));

			menu.DropDown(nullGuardIconRect);
		}

		public void AddInitializer(Rect addButtonRect)
		{
			var target = targets[0];
			var targetType = target.GetType();
			var initializerTypes = InitializerEditorUtility.GetInitializerTypes(targetType).ToArray();
			
			int count = initializerTypes.Length;
			if(count > 0)
			{
				var menu = new GenericMenu();
				int activeOptions = 0;
				foreach(var initializerType in initializerTypes)
				{
					var label = new GUIContent(TypeUtility.ToString(initializerType));
					if(IsTargetedByInitializerOfType(initializerType))
					{
						menu.AddDisabledItem(label, true);
					}
					else
					{
						menu.AddItem(label, false, () => InitializerEditorUtility.AddInitializer(targets, initializerType));
						activeOptions++;
					}
				}

				if(activeOptions == 1)
				{
					InitializerEditorUtility.AddInitializer(targets, initializerTypes.First(initializerType => !IsTargetedByInitializerOfType(initializerType)));
				}
				else
				{
					menu.DropDown(addButtonRect);
				}
			}
			else
			{
				var menu = new GenericMenu();
				menu.AddItem(new("Generate Initializer"), false, () => InitializerEditorUtility.GenerateAndAttachInitializer(targets, target));
				menu.DropDown(addButtonRect);
			}

			GUI.changed = true;

			if(Event.current != null)
			{
				LayoutUtility.ExitGUI();
			}
		}

		private bool IsTargetedByInitializerOfType(Type initializerType)
		{
			for(int i = 0, count = gameObjects.Length; i < count; i++)
			{
				var gameObject = gameObjects[i];
				foreach(var initializer in gameObject.GetComponentsNonAlloc<IInitializer>())
				{
					if(initializer.GetType() == initializerType && initializer.Target == targets[i])
					{
						return true;
					}
				}
			}

			return false;
		}

		private void OnInitializerContextMenuButtonPressed(Object firstInitializer, bool mixedInitializers, Rect? toggleInitializerRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new("Reset"), false, Reset);

			menu.AddSeparator("");

			menu.AddItem(new("Remove"), false, () => LayoutUtility.OnLayoutEvent(RemoveInitializerFromAllTargets));

			if(MonoScript.FromMonoBehaviour(firstInitializer as MonoBehaviour) is MonoScript scriptAsset)
			{
				menu.AddSeparator("");
				menu.AddItem(new("Edit Script"), false, () => AssetDatabase.OpenAsset(scriptAsset));
				menu.AddItem(new("Ping Script"), false, () => EditorApplication.delayCall += () => EditorGUIUtility.PingObject(scriptAsset));
			}

			if(!mixedInitializers)
			{
				menu.AddSeparator("");
				menu.AddItem(new("Preset"), false, () => PresetSelector.ShowSelector(initializers, null, true));
			}

			if(toggleInitializerRect.HasValue)
			{
				menu.DropDown(toggleInitializerRect.Value);
			}
			else
			{
				menu.ShowAsContext();
			}

			void Reset()
			{
				EditorGUIUtility.editingTextField = false;

				Object destroyWhenDone;
				Object copySource;
				if(firstInitializer is Component)
				{
					var tempGameObject = new GameObject("");
					tempGameObject.SetActive(false);
					destroyWhenDone = tempGameObject;
					var tempComponent = tempGameObject.AddComponent(firstInitializer.GetType());
					tempComponent.hideFlags = HideFlags.HideInInspector;
					(tempComponent as IInitializer).Target = Target;
					copySource = tempComponent;
				}
				else if(firstInitializer is ScriptableObject)
				{
					var tempScriptableObject = ScriptableObject.CreateInstance(firstInitializer.GetType());
					(tempScriptableObject as IInitializer).Target = Target;
					copySource = tempScriptableObject;
					destroyWhenDone = tempScriptableObject;
				}
				else
				{
					return;
				}

				ForEachInitializer("", (index, initializer) =>
				{
					EditorUtility.CopySerialized(copySource, initializer);
					((IInitializer)initializer).Target = targets[index];
					initializer.hideFlags = gameObjects.Length >= index && targets[index] ? HideFlags.HideInInspector : HideFlags.None;
				});

				Object.DestroyImmediate(destroyWhenDone);
			}
		}

		private void RemoveInitializerFromAllTargets()
		{
			if(initializerEditor)
			{
				AnyPropertyDrawer.Dispose(initializerEditor.serializedObject);
			}

			ForEachInitializer("Remove Initializer", RemoveInitializer);

			if(isResponsibleForInitializerEditorLifetime && initializerEditor)
			{
				EditorDecoratorInjector.RemoveFrom(initializerEditor, ExecutionOptions.CanBeExecutedImmediately);
				Object.DestroyImmediate(initializerEditor);
				initializerEditor = null;
			}

			if(targets[0] is ScriptableObject scriptableObject
				&& TypeUtility.DerivesFromGenericBaseType(scriptableObject.GetType())
				&& ownerSerializedObject.FindProperty("initializer") is SerializedProperty initializerProperty)
			{
				initializerProperty.objectReferenceValue = null;
				ownerSerializedObject.ApplyModifiedProperties();
			}

			foreach(var target in targets)
			{
				string path = target ? AssetDatabase.GetAssetPath(target) : null;
				if(!string.IsNullOrEmpty(path))
				{
					AssetDatabase.ImportAsset(path);
				}
			}

			Changed?.Invoke(this);
		}

		void RemoveInitializer(Object initializer)
		{
			if(AssetDatabase.IsSubAsset(initializer))
			{
				AssetDatabase.RemoveObjectFromAsset(initializer);
			}

			if(initializer)
			{
				Undo.DestroyObjectImmediate(initializer);
			}
		}

		private void ForEachInitializer(string undoName, Action<int, Component> action)
		{
			if(!string.IsNullOrEmpty(undoName))
			{
				Undo.RecordObjects(initializers, undoName);
			}

			for(int i = 0, count = initializers.Length; i < count; i++)
			{
				var initializer = initializers[i] as Component;
				if(initializer)
				{
					action(i, initializer);
				}
			}

			if(initializerEditor)
			{
				initializerEditor.serializedObject.Update();

				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(initializerEditor.GetType().Name + "Repaint");
				#if DEV_MODE && DEBUG_REPAINT && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				Profiler.BeginSample("Sisus.Repaint");
				#endif
				#endif

				initializerEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				Profiler.EndSample();
				#endif
			}
		}

		private void ForEachInitializer(string undoName, Action<Object> action)
		{
			var notNullInitializers = initializers.All(i => (bool)i) ? initializers : initializers.Where(i => (bool)i).ToArray();
			if(!string.IsNullOrEmpty(undoName) && notNullInitializers.Length > 0)
			{
				Undo.RecordObjects(notNullInitializers, undoName);
			}

			for(int i = 0, count = notNullInitializers.Length; i < count; i++)
			{
				action(notNullInitializers[i]);
			}

			if(initializerEditor && initializerEditor.target)
			{
				initializerEditor.serializedObject.Update();

				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(initializerEditor.GetType().Name + "Repaint");
				#if DEV_MODE && DEBUG_REPAINT && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				Profiler.BeginSample("Sisus.Repaint");
				#endif
				#endif

				initializerEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				Profiler.EndSample();
				#endif
			}
		}

		public void Dispose()
		{
			#if DEV_MODE && DEBUG_DISPOSE
			Debug.Log($"{GetType().Name}.Dispose() with Event.current:{Event.current?.type.ToString() ?? "None"}");
			#endif

			if(Changed is not null)
			{
				var callback = Changed;
				Changed = null;
				callback(this);
			}

			ServiceChangedListener.DisposeAll(ref serviceChangedListeners);

			if(initializerEditor && isResponsibleForInitializerEditorLifetime)
			{
				Object.DestroyImmediate(initializerEditor);
				initializerEditor = null;
			}

			#if ODIN_INSPECTOR
			if(odinPropertyTree != null)
			{
				odinPropertyTree.Dispose();
				odinPropertyTree = null;
			}
			#endif

			AfterHeaderGUI = null;
			NowDrawing = null;
		}

		private void GetInitializersOnTargets(out bool hasInitializers, out Object firstInitializer)
		{
			if(initializerEditor)
			{
				initializers = initializerEditor.targets;
				firstInitializer = initializers[0];
				hasInitializers = firstInitializer;
				return;
			}

			if(lockInitializers)
			{
				firstInitializer = initializers.Length == 0 ? null : initializers[0];
				hasInitializers = firstInitializer;
				if(!hasInitializers && headerLabel.text != DefaultHeaderText)
				{
					headerLabel.text = DefaultHeaderText;
					GUI.changed = true;
				}
				return;
			}

			int targetCount = targets.Length;
			hasInitializers = false;
			Array.Resize(ref initializers, targetCount);

			firstInitializer = null;
			for(int i = 0; i < targetCount; i++)
			{
				initializers[i] = null;

				var rootObject = rootObjects[i];
				if(!InitializerEditors.InitializersOnInspectedObjects.TryGetValue(rootObject, out var initializersOnRootObject))
				{
					continue;
				}

				foreach(var initializer in initializersOnRootObject)
				{
					if(!ShouldDrawInitializerEmbedded(initializer))
					{
						continue;
					}

					var initializerAsObject = initializer as Object;
					if(!initializerAsObject)
					{
						continue;
					}

					initializers[i] = initializerAsObject;
					hasInitializers = true;

					if(!firstInitializer)
					{
						firstInitializer = initializerAsObject;
					}
				}
			}
		}

		private bool ShouldDrawInitializerEmbedded([DisallowNull] IInitializer initializer)
		{
			if(initializer is Object initializerAsObject && !initializerAsObject)
			{
				return false;
			}

			var initializerTarget = initializer.Target;
			if(!initializerTarget)
			{
				return false;
			}

			foreach(var target in targets)
			{
				if(initializerTarget == target)
				{
					return true;
				}
			}

			return false;
		}

		[Flags]
		private enum HelpBoxMessageType
		{
			None = _0,
			TargetInitializedWhenBecomesActive = _1,
			TargetInitializedWhenDeserialized = _2,
			TargetHasMissingDependencies = _3
		}

		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker constructorMarker = new(ProfilerCategory.Gui, nameof(InitializerGUI) + ".Ctr");
		private static readonly ProfilerMarker setupMarker = new(ProfilerCategory.Gui, nameof(InitializerGUI) + "." + nameof(Setup));
		private static readonly ProfilerMarker onInspectorGUIMarker = new(ProfilerCategory.Gui, nameof(InitializerGUI) + "." + nameof(OnInspectorGUI));
		#endif
	}
}