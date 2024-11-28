//#define DEBUG_DISPOSE
#define DEBUG_REPAINT

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
#if DEV_MODE
using UnityEngine.Profiling;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
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
		public const string ServiceVisibilityEditorPrefsKey = "InitArgs.InitializerServiceVisibility";
		private const string HideInitSectionUserDataKey = "hideInitSection";
		private const string NullArgumentGuardUserDataKey = "nullArgumentGuard";
		private const string DefaultHeaderText = "Init";
		private const string ClientInitializedDuringOnAfterDeserializeText = "Component will be initialized when the game object is deserialized.";
		private const string SpaceForButtons = "\n\n\n";
		private const string ClientInitializedDuringOnAfterDeserializeTextWithSpaceForButton = SpaceForButtons + ClientInitializedDuringOnAfterDeserializeText;
		private const string ClientInitializedWhenBecomesActiveText = "Component will be initialized when the game object becomes active.";
		private const string ClientInitializedWhenBecomesActiveTextWithSpaceForButton = SpaceForButtons + ClientInitializedWhenBecomesActiveText;
		private const string IsUnfoldedUserDataKey = "initArgsUnfolded";
		private const string AddInitializerTooltip = "Attach an Initializer.\n\nThis can be used to customize the arguments received by this client during initialization.";
		private const string AddStateMachineInitializerTooltip = "Attach a State Machine Behaviour Initializer.\n\nThis can be used to customize the arguments received by the state machine behaviour during initialization.";
		private static readonly GUIContent useAwakeButtonLabel = new(" Use Awake", "Initialize target later during the Awake event when the game object becomes active?");
		private static readonly GUIContent useOnAfterDeserializeButtonLabel = new(" Use OnAfterDeserialize", "Initialize target earlier during the OnAfterDeserialize event before the game object becomes active?");

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
		private readonly GUIContent contextMenuIcon = new();
		private readonly GUIContent nullGuardDisabledIcon = new();
		private readonly GUIContent nullGuardPassedWithValueProviderValueMissing = new();
		private readonly GUIContent nullGuardPassedIcon = new();
		private readonly GUIContent nullGuardFailedIcon = new();
		//private readonly GUIContent nullGuardStillInitializingIcon = new();
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
		private bool targetImplementsIArgs;
		private bool targetDerivesFromGenericBaseType;
		//private bool targetCanMaybeSelfInitialize; // allParametersAreServices && targetImplementsIArgs
		//private bool targetCanForSureSelfInitialize; // allParametersAreServices && targetDerivesFromGenericBaseType
		private bool? hadInitializerLastFrame;
		private NullGuardResult? nullGuardResultLastFrame;
		private Object[] initializers = new Object[1];
		private Editor initializerEditor;
		//private Action drawArguments;
		//private InitializerEditorWrapper drawArguments;
		//private InitializerEditor initializerEditor;
		private bool lockInitializers;
		private bool shouldUpdateInitArgumentDependentState;

		#if ODIN_INSPECTOR
		private PropertyTree odinPropertyTree;
		internal PropertyTree OdinPropertyTree => odinPropertyTree ??= PropertyTree.Create(ownerSerializedObject);
		#endif

		internal Editor InitializerEditor => initializerEditor;

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

			#if DEV_MODE
			using ProfilerScope x = new("InitializerGUI.Ctr");
			#endif

			this.ownerSerializedObject = ownerSerializedObject;
			this.targets = ownerSerializedObject.targetObjects;
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

				targetDerivesFromGenericBaseType = InitializableUtility.CanSelfInitializeWithoutInitializer(target);
				targetImplementsIArgs = targetDerivesFromGenericBaseType || InitializableUtility.TryGetIArgsInterface(target.GetType(), out _);
			}

			this.initParameterTypes = initParameterTypes;
			initParametersAreServices = new bool[initParameterTypes.Length];

			this.gameObjects = gameObjects;
			rootObjects = gameObjects.Length > 0 ? gameObjects : targets;

			isResponsibleForInitializerEditorLifetime = initializerEditor is null;
			//this.initializerEditor = initializerEditor;
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
			UpdateInitArgumentDependentState(hasInitializers);
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

		private void UpdateInitArgumentDependentState(bool hasInitializers)
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

			// targetCanMaybeSelfInitialize = allParametersAreServices && targetImplementsIArgs;
			// targetCanForSureSelfInitialize = allParametersAreServices && targetDerivesFromGenericBaseType;

			UpdateTooltips(hasInitializers);
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
			Type targetType;
			MonoScript targetScript;
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

			return IsInitSectionHidden(targetScript, targetType);
		}

		public static bool IsInitSectionHidden([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType) => GetBoolUserData(initializableScript, initializableType, HideInitSectionUserDataKey);

		public static void ToggleHideInitSection([AllowNull] MonoScript initializableScript, [DisallowNull] Type initializableType)
		{
			const string userDataKey = HideInitSectionUserDataKey;
			bool setValue = !GetBoolUserData(initializableScript, initializableType, userDataKey);
			Menu.SetChecked(ComponentMenuItems.ShowInitSection, setValue);
			SetUserData(initializableScript, initializableType, userDataKey, setValue);
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
			bool alltargetsHandled = true;

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
							alltargetsHandled = false;
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
				//bool done = false;
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
						if (initializer is not IInitializerEditorOnly initializerEditorOnly)
						{
							alltargetsHandled = false;
							continue;
						}

						initializerEditorOnly.NullArgumentGuard = value;
					}

					tempSerializedObject.Update();
					tempSerializedObject.ApplyModifiedProperties();
				}
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
			if(hasInitializers && firstInitializer.GetType().GetNestedType(EditorOnly.InitializerEditor.InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic) is Type metadata && metadata.GetCustomAttributes<DisplayNameAttribute>().FirstOrDefault() is DisplayNameAttribute displayName)
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
			using ProfilerScope x = new("InitializerGUI.Setup");
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

			if(hasServiceParameters)
			{
				servicesHiddenIcon.image = EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image;
				servicesShownIcon.image = EditorGUIUtility.IconContent("animationvisibilitytoggleon").image;
			}
			else
			{
				servicesHiddenIcon.image = null;
				servicesShownIcon.image = null;
			}

			if(LayoutUtility.NowDrawing)
			{
				LayoutUtility.NowDrawing.Repaint();
			}
		}

		private void UpdateTooltips(bool hasInitializers)
		{
			addInitializerIcon.tooltip = Target is Animator ? AddStateMachineInitializerTooltip : AddInitializerTooltip;
			if(hasServiceParameters)
			{
				servicesShownIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initParametersAreServices, true);
				servicesHiddenIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initParametersAreServices, false);
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
			using ProfilerScope x = new("InitializerGUI.OnInspectorGUI");
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
					UpdateInitArgumentDependentState(hasInitializers);
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
					UpdateInitArgumentDependentState(hasInitializers);
				}

				EditorGUIUtility.hierarchyMode = true;
				EditorGUI.indentLevel = 0;

				var firstInitializerEditorOnly = firstInitializer as IInitializerEditorOnly;

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
				bool isCollapsible = drawInitHeader && (hasInitializers || helpBoxMessage != HelpBoxMessageType.None);

				if(drawInitHeader)
				{
					var backgroundStyle = isCollapsible ? initializerBackgroundStyle : noInitializerBackgroundStyle;
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

				if(drawTargetField)
				{
					if(isResponsibleForInitializerEditorLifetime)
					{
						foldoutRect.x -= 12f;
					}

					foldoutRect.width = EditorGUIUtility.labelWidth - foldoutRect.x;
				}
				else if(!isResponsibleForInitializerEditorLifetime)
				{
					foldoutRect.x -= 12f;
					foldoutRect.width -= 38f + IconWidth;
				}
				else
				{
					foldoutRect.width = EditorGUIUtility.labelWidth - foldoutRect.x - IconWidth;
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
					if(drawInitHeader)
					{
						if(isUnfolded && helpBoxMessage == HelpBoxMessageType.TargetInitializedWhenBecomesActive)
						{
							DrawInactiveInitializerHelpBox(helpBoxMessage);
						}

						DrawInitHeader(headerRect, ref foldoutRect, labelStyle, isUnfolded, isCollapsible, hasInitializers, mixedInitializers, firstInitializer);
					}

					if(GUI.Button(addInitializerOrContextMenuRect, addInitializerIcon, Styles.AddButtonStyle))
					{
						if(OnAddInitializerButtonPressedOverride != null)
						{
							OnAddInitializerButtonPressedOverride.Invoke(addInitializerOrContextMenuRect);
							return;
						}

						AddInitializer(addInitializerOrContextMenuRect);
					}

					// Don't draw null argument guard unless target implements interface that is necessary for using InitArgs.TryGet.
					bool drawNullGuard = targetImplementsIArgs;
					if(drawNullGuard)
					{
						var nullGuard = GetNullArgumentGuardFlags(Target);

						var nullGuardIconRect = addInitializerOrContextMenuRect;
						nullGuardIconRect.x -= addInitializerOrContextMenuRect.width;
						nullGuardIconRect.y -= 2f;

						if(GUI.Button(nullGuardIconRect, GUIContent.none, EditorStyles.label))
						{
							OnInitializerNullGuardButtonPressed(nullGuard, nullGuardIconRect, CanThrowRuntimeExceptions(hasInitializers));
						}

						var guiColorWas = GUI.color;
						GUIContent nullGuardIcon;
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
										nullGuardIcon.tooltip = "❌ Target has not been initialized.";
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
									nullGuardIcon.tooltip = "✔️ Target has been initialized.";
									break;
								case InitState.Failed:
									nullGuardIcon = initStateFailedIcon;
									nullGuardIcon.tooltip = "❌ Target initialization has failed.";
									break;
								default:
									throw new ArgumentOutOfRangeException(initializable.InitState.ToString());
							}
						}
						else if( (!nullGuard.HasFlag(NullArgumentGuard.EnabledForPrefabs) && (gameObjects.FirstOrDefault()?.IsAsset(false) ?? true))
						 || (!nullGuard.HasFlag(NullArgumentGuard.EditModeWarning) && !Application.isPlaying) )
						{
							nullGuardIcon = nullGuardDisabledIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, false) + "\n\nNull argument guard is off.";
						}
						else if(allParametersAreServices)
						{
							if(anyParameterIsAsyncLoadedService)
							{
								nullGuardIcon = nullGuardPassedWithValueProviderValueMissing;
								nullGuardIcon.tooltip
									= GetTooltip(nullGuard, false)
									  + "\n\nAll arguments are services, but some of them are loaded asynchronously.\n\nAdding an Initializer is recommended for deferred initialization support, in case the service isn't ready yet when this client is loaded.";
							}
							else
							{
								nullGuardIcon = nullGuardPassedIcon;
								nullGuardIcon.tooltip
									= GetTooltip(nullGuard, false) +
									  (targetDerivesFromGenericBaseType
										  ? "\n\nAll arguments are services.\n\nThe client will receive them automatically during initialization.\n\nAdding an Initializer is not necessary - unless there is a need to override some of the services for this particular client."
										  : "\n\nAll arguments are services.\n\nThe client can use InitArgs.TryGet to acquire them during initialization, in which case adding an Initializer is not necessary - unless there is a need to override some of the services for this particular client");
							}
						}
						else
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) +"\n\nMissing argument detected.\n\nIf the argument should be allowed to be null, then set the 'Null Argument Guard' option to 'None'.\n\nIf the missing argument is a service that only becomes available at runtime select 'Service (Local)' from the dropdown.";
						}

						GUI.Label(nullGuardIconRect, nullGuardIcon);
						GUI.color = guiColorWas;
					}
				}
				else
				{
					if(!isResponsibleForInitializerEditorLifetime)
					{
						addInitializerOrContextMenuRect.x += addInitializerOrContextMenuRect.width;
					}
					else if(GUI.Button(addInitializerOrContextMenuRect, GUIContent.none, EditorStyles.label))
					{
						OnInitializerContextMenuButtonPressed(firstInitializer, mixedInitializers, addInitializerOrContextMenuRect);
					}

					var nullGuardIconRect = addInitializerOrContextMenuRect;
					nullGuardIconRect.x -= addInitializerOrContextMenuRect.width;

					bool drawNullGuard = firstInitializerEditorOnly is { ShowNullArgumentGuard: true };
					NullGuardResult nullGuardResult = NullGuardResult.Passed;
					if(!allParametersAreServices && firstInitializerEditorOnly != null)
					{
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

					if(!nullGuardResultLastFrame.HasValue || nullGuardResultLastFrame.Value != nullGuardResult)
					{
						nullGuardResultLastFrame = nullGuardResult;
						UpdateInitArgumentDependentState(hasInitializers);
					}

					var nullGuard = drawNullGuard ? firstInitializerEditorOnly.NullArgumentGuard : NullArgumentGuard.None;

					if(drawNullGuard && GUI.Button(nullGuardIconRect, GUIContent.none, EditorStyles.label))
					{
						OnInitializerNullGuardButtonPressed(nullGuard, nullGuardIconRect, CanThrowRuntimeExceptions(hasInitializers));
					}

					bool servicesShown = ServicesShown;
					var serviceVisibilityIconRect = nullGuardIconRect;
					if(drawNullGuard)
					{
						serviceVisibilityIconRect.x -= nullGuardIconRect.width;
					}

					if(hasServiceParameters && GUI.Button(serviceVisibilityIconRect, GUIContent.none, EditorStyles.label))
					{
						servicesShown = !servicesShown;
						ServicesShown = servicesShown;
						EditorPrefs.SetBool(ServiceVisibilityEditorPrefsKey, servicesShown);
					}

					if(isUnfolded)
					{
						if(helpBoxMessage == HelpBoxMessageType.TargetInitializedWhenDeserialized)
						{
							DrawInactiveInitializerHelpBox(helpBoxMessage);
						}
						else if(helpBoxMessage != HelpBoxMessageType.None)
						{
							DrawHelpBox(helpBoxMessage);
						}

						GUILayout.Space(3f);

						DrawInitializerArguments();
					}

					if(drawInitHeader)
					{
						DrawInitHeader(headerRect, ref foldoutRect, labelStyle, isUnfolded, isCollapsible, hasInitializers, mixedInitializers, firstInitializer);
					}

					GUI.Label(addInitializerOrContextMenuRect, contextMenuIcon);

					var iconSizeWas = EditorGUIUtility.GetIconSize();
					EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

					if(drawNullGuard)
					{
						GUIContent nullGuardIcon;

						bool nullGuardDisabled;
						nullGuardDisabled =
							!nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning)
							|| (!nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs) && PrefabUtility.IsPartOfPrefabAsset(firstInitializer));

						var guiColorWas = GUI.color;

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
										nullGuardIcon.tooltip = "❌ Target has not been initialized.";
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
									nullGuardIcon.tooltip = "✔️ Target has been initialized.";
									break;
								case InitState.Failed:
									nullGuardIcon = initStateFailedIcon;
									nullGuardIcon.tooltip = "❌ Target initialization has failed.";
									break;
								default:
									throw new ArgumentOutOfRangeException(initializable.InitState.ToString());
							}
						}
						else if(nullGuardDisabled)
						{
							nullGuardIcon = nullGuardDisabledIcon;
							nullGuardIconRect.width -= 1f;
							nullGuardIconRect.height -= 1f;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nNull argument guard is off.";
						}
						else if(nullGuardResult == NullGuardResult.ValueMissing)
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) +"\n\nMissing argument detected.\n\nIf the argument should be allowed to be null, then set the 'Null Argument Guard' option to 'None'.\n\nIf the missing argument is a service that only becomes available at runtime select 'Service (Local)' from the dropdown.";
						}
						else if(nullGuardResult == NullGuardResult.InvalidValueProviderState)
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nSome value providers have been configured invalidly and will not be able to provide a value at runtime.";
						}
						else if(nullGuardResult == NullGuardResult.ClientNotSupported)
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nSome value providers do not support the client and will not be able to provide a value at runtime.";
						}
						else if(nullGuardResult == NullGuardResult.TypeNotSupported)
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nSome value providers do not support the client's type and will not be able to provide a value at runtime.";
						}
						else if(nullGuardResult == NullGuardResult.ValueProviderException)
						{
							nullGuardIcon = nullGuardFailedIcon;
							if(string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
							{
								nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nAn exception was encountered while trying to retrieve a value from one of the value providers.";
							}
							else
							{
								nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nAn exception was encountered while trying to retrieve a value from one of the value providers:\n" + firstInitializerEditorOnly.NullGuardFailedMessage;
							}
						}
						else if(nullGuardResult == NullGuardResult.ClientException)
	                    {
                    		nullGuardIcon = nullGuardFailedIcon;
                    		if(string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
                    		{
                    			nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nAn exception was thrown by the client during its initialization.";
                    		}
                    		else
                    		{
                    			nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nAn exception was thrown by the client during its initialization:\n" + firstInitializerEditorOnly.NullGuardFailedMessage;
                    		}
	                    }
						else if(nullGuardResult == NullGuardResult.ValueProviderValueMissing)
						{
							nullGuardIcon = nullGuardFailedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nSome value providers will not be able to provide a value at runtime.";
						}
						else if(nullGuardResult == NullGuardResult.ValueProviderValueNullInEditMode)
						{
							if(!string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage) && !Application.isPlaying)
							{
								firstInitializerEditorOnly.NullGuardFailedMessage = "";
							}

							nullGuardIcon = nullGuardPassedWithValueProviderValueMissing;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nSome value providers are not able to provide a value at this moment, but may be able to do so at runtime.";
							GUI.color = new Color(0.6f, 0.6f, 1f, 1f);
						}
						else
						{
							if(!string.IsNullOrEmpty(firstInitializerEditorOnly.NullGuardFailedMessage))
							{
								firstInitializerEditorOnly.NullGuardFailedMessage = "";
							}

							nullGuardIcon = nullGuardPassedIcon;
							nullGuardIcon.tooltip = GetTooltip(nullGuard, true) + "\n\nAll arguments provided.";
						}

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
						var targetFieldRect = headerRect;
						targetFieldRect.height = EditorGUIUtility.singleLineHeight;
						if(drawNullGuard)
						{
							targetFieldRect.width -= nullGuardIconRect.width;
						}

						if(drawAddInitializerButton || drawContextMenuButton)
						{
							targetFieldRect.width -= addInitializerOrContextMenuRect.width;
						}

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
				}

				if(drawInitHeader)
				{
					EditorGUILayout.EndVertical();
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

			bool CanThrowRuntimeExceptions(bool hasInitializers) => hasInitializers || TypeUtility.DerivesFromGenericBaseType(Target.GetType());
		}

		private bool CanUseInitializersToStoreUnfoldedState(bool hasInitializers) => hasInitializers && initializables.Length > 0;
		private Type GetIsUnfoldedUserDataType() => Target?.GetType() ?? typeof(Object);

		private void DrawInactiveInitializerHelpBox(HelpBoxMessageType message)
		{
			bool usingOnAfterDeserialize = message == HelpBoxMessageType.TargetInitializedWhenDeserialized;
			string helpBoxText = usingOnAfterDeserialize ? ClientInitializedDuringOnAfterDeserializeTextWithSpaceForButton : ClientInitializedWhenBecomesActiveTextWithSpaceForButton;
			DrawHelpBox(helpBoxText);

			var helpBoxRect = GUILayoutUtility.GetLastRect();
			var buttonRect = helpBoxRect;
			const float textLeftOffset = 35f;
			const float textRightRightOffset = 10f;
			buttonRect.x += textLeftOffset;
			float buttonMaxWidth = helpBoxRect.width - 45f;
			buttonRect.width -= textLeftOffset + textRightRightOffset;
			buttonRect.y += 3f;
			buttonRect.height = EditorGUIUtility.singleLineHeight;

			var buttonStyle = EditorStyles.radioButton;

			buttonStyle.CalcMinMaxWidth(useOnAfterDeserializeButtonLabel, out float buttonOptimalWidth, out _);
			buttonRect.width = Mathf.Min(buttonOptimalWidth, buttonMaxWidth);
			if(GUI.Toggle(buttonRect, usingOnAfterDeserialize, useOnAfterDeserializeButtonLabel, buttonStyle) && !usingOnAfterDeserialize)
			{
				InitializerEditorUtility.AddInitializer(targets, typeof(InactiveInitializer));
			}

			buttonRect.y += buttonRect.height;
			buttonStyle.CalcMinMaxWidth(useAwakeButtonLabel, out buttonOptimalWidth, out _);
			buttonRect.width = Mathf.Min(buttonOptimalWidth, buttonMaxWidth);
			if(GUI.Toggle(buttonRect, !usingOnAfterDeserialize, useAwakeButtonLabel, buttonStyle) && usingOnAfterDeserialize)
			{
				LayoutUtility.ApplyWhenSafe(RemoveInitializerFromAllTargets);
			}
		}

		private static void DrawHelpBox(HelpBoxMessageType message)
		{
			bool usingOnAfterDeserialize = message == HelpBoxMessageType.TargetInitializedWhenDeserialized;
			string helpBoxText = usingOnAfterDeserialize ? ClientInitializedDuringOnAfterDeserializeText : ClientInitializedWhenBecomesActiveText;
			DrawHelpBox(helpBoxText);
		}

		private static void DrawHelpBox(string helpBoxText)
		{
			GUILayout.Space(3f);
			EditorGUILayout.HelpBox(helpBoxText, MessageType.Info, true);
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

		private void DrawInitHeader(Rect headerRect, ref Rect foldoutRect, GUIStyle labelStyle, bool isUnfolded, bool isCollapsible, bool hasInitializers, bool mixedInitializers, Object firstInitializer)
		{
			var backgroundRect = headerRect;
			backgroundRect.y -= 3f;
			backgroundRect.x -= 18f;
			backgroundRect.width += 22f;
			if(Event.current.type is EventType.Repaint)
			{
				initArgsFoldoutBackgroundStyle.Draw(backgroundRect, false, false, false, false);
			}

			foldoutRect.x -= 12f;

			if(Event.current.type is EventType.Repaint)
			{
				labelStyle.Draw(foldoutRect, headerLabel, GUIUtility.GetControlID(FocusType.Passive), isUnfolded);
			}

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

			bool guiWasEnabled = GUI.enabled;
			if(!isCollapsible)
			{
				GUI.enabled = false;
			}

			var foldoutClickableRect = foldoutRect;
			foldoutClickableRect.x -= 5f;
			foldoutClickableRect.width += 10f;

			if(!isResponsibleForInitializerEditorLifetime)
			{
				foldoutClickableRect.x -= 12f;
				foldoutClickableRect.width += 12f;
			}

			bool setUnfolded = isUnfolded;
			if(Event.current.type == EventType.MouseDown && foldoutClickableRect.Contains(Event.current.mousePosition))
			{
				if(Event.current.button == 0)
				{
					setUnfolded = isCollapsible ? !isUnfolded : isUnfolded;
				}
				else if(Event.current.button == 1)
				{
					OnInitializerContextMenuButtonPressed(firstInitializer, mixedInitializers, null);
				}

				Event.current.Use();
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
							#if DEV_MODE && DEBUG_ENABLED
							Debug.Log($"SetIsInspectorExpanded({initializer.GetType().Name}, {setUnfolded})");
							#endif

							LayoutUtility.ApplyWhenSafe(() =>
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

					LayoutUtility.ApplyWhenSafe(() =>
					{
						EditorPrefsUtility.SetUserData(userDataType, IsUnfoldedUserDataKey, setUnfolded);
					});
				}

				LayoutUtility.ExitGUI();
			}
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

			if((allParametersAreServices && targetDerivesFromGenericBaseType) || (hasInitializers && nullGuardResultLastFrame.GetValueOrDefault(NullGuardResult.Passed) == NullGuardResult.Passed))
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
				sb.Append(TypeUtility.ToString(initParameterTypes[i]));
				if(initParametersAreServices[i])
				{
					sb.Append(" <color=grey>(Service)</color>");
				}
			}

			if(Target is IInitializer initializer)
			{
				if(initializer.Target == null)
				{
					sb.Append("\n\nIt will be attached to the game object at runtime.");
				}
				else if(initializer.Target is Component targetComponent && (initializer is not Component initializerComponent || initializerComponent.gameObject != targetComponent.gameObject))
				{
					sb.Append("\n\nIt will be instantiated at runtime.");
				}
			}

			return sb.ToString();
		}

		private string GetServiceVisibilityTooltip([DisallowNull] Type[] initParameterTypes, [DisallowNull] bool[] initServiceParameters, bool servicesShown)
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

			sb.Append("\n\nThese services will be provided automatically during initialization.");

			return sb.ToString();
		}

		private static string GetTooltip(NullArgumentGuard guard, bool hasInitializer)
		{
			return hasInitializer ?
			(guard switch
			{
				NullArgumentGuard.EditModeWarning => "Null Argument Guard:\n✔️ Edit Mode Warning\n❌ Runtime Exception\n❌ Enabled For Prefabs",
				NullArgumentGuard.RuntimeException => "Null Argument Guard:\n❌ Edit Mode Warning\n✔️ Runtime Exception\n❌ Enabled For Prefabs",
				NullArgumentGuard.EnabledForPrefabs => "Null Argument Guard:\n❌ Edit Mode Warning\n❌ Runtime Exception\n✔️ Enabled For Prefabs",
				NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException => "Null Argument Guard:\n✔️ Edit Mode Warning\n✔️ Runtime Exception\n❌ Enabled For Prefabs",
				NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs => "Null Argument Guard:\n❌ Edit Mode Warning\n✔️ Runtime Exception\n✔️ Enabled For Prefabs",
				NullArgumentGuard.EditModeWarning | NullArgumentGuard.EnabledForPrefabs => "Null Argument Guard:\n✔️ Edit Mode Warning\n✔️ Runtime Exception\n❌ Enabled For Prefabs",
				NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs => "Null Argument Guard:\n✔️ Edit Mode Warning\n✔️ Runtime Exception\n✔️ Enabled For Prefabs",
				_ => "Null Argument Guard:\n❌ Edit Mode Warning\n❌ Runtime Exception\n❌ Enabled For Prefabs"
			})
			:  guard switch
			{
				NullArgumentGuard.EditModeWarning => "Null Argument Guard:\n✔️ Edit Mode Warning",
				_ => "Null Argument Guard:\n❌ Edit Mode Warning"
			};
		}

		private void OnInitializerNullGuardButtonPressed(NullArgumentGuard nullGuard, Rect nullGuardIconRect, bool canThrowRuntimeExceptions)
		{
			var menu = new GenericMenu();

			switch(Event.current.button)
			{
				case 0:
					menu.AddItem(new GUIContent("None"), nullGuard == NullArgumentGuard.None, () => Set(NullArgumentGuard.None));
					menu.AddItem(new GUIContent("Edit Mode Warning"), nullGuard.IsEnabled(NullArgumentGuard.EditModeWarning), ()=> Toggle(NullArgumentGuard.EditModeWarning));
					if(canThrowRuntimeExceptions)
					{
						menu.AddItem(new GUIContent("Runtime Exception"), nullGuard.IsEnabled(NullArgumentGuard.RuntimeException), () => Toggle(NullArgumentGuard.RuntimeException));
						menu.AddItem(new GUIContent("Enabled For Prefabs"), nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs), () => Toggle(NullArgumentGuard.EnabledForPrefabs));
						menu.AddItem(new GUIContent("All"), nullGuard == (NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs), () => Set(NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs));
					}
					else // for now these features are not supported without an initializer
					{
						menu.AddDisabledItem(new GUIContent("Runtime Exception"), false);
						menu.AddDisabledItem(new GUIContent("Enabled For Prefabs"), false);
					}
					break;
				case 1:
					menu.AddItem(new GUIContent("Debug"), false, ()=> EditorApplication.ExecuteMenuItem(ServicesWindow.MenuItemName));
					menu.AddItem(new GUIContent("Help"), false, ()=> Application.OpenURL("https://docs.sisus.co/init-args/common-problems-solutions/client-not-receiving-services/"));
					break;
				default:
					return;
			}

			void Set(NullArgumentGuard flags) => SetNullArgumentGuardFlags(flags);
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
				menu.AddItem(new GUIContent("Generate Initializer"), false, () => InitializerEditorUtility.GenerateAndAttachInitializer(targets, target));
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

			menu.AddItem(new GUIContent("Reset"), false, Reset);

			menu.AddSeparator("");

			menu.AddItem(new GUIContent("Remove"), false, () => LayoutUtility.ApplyWhenSafe(RemoveInitializerFromAllTargets));

			menu.AddItem(new GUIContent("Copy"), false, Copy);

			menu.AddItem(new GUIContent("Paste"), false, Paste);

			menu.AddSeparator("");

			if(MonoScript.FromMonoBehaviour(firstInitializer as MonoBehaviour) is MonoScript scriptAsset)
			{
				menu.AddItem(new GUIContent("Edit Script"), false, () => AssetDatabase.OpenAsset(scriptAsset));
				menu.AddItem(new GUIContent("Ping Script"), false, () => EditorApplication.delayCall += () => EditorGUIUtility.PingObject(scriptAsset));
			}

			if(!mixedInitializers)
			{
				menu.AddItem(new GUIContent("Preset"), false, () => PresetSelector.ShowSelector(initializers, null, true));
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

			void Copy()
			{
				if(firstInitializer is Component component)
				{
					ComponentUtility.CopyComponent(component);
				}
				else
				{
					EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(firstInitializer, true);
				}
			}

			void Paste()
			{
				EditorGUIUtility.editingTextField = false;

				if(firstInitializer is Component)
				{
					ForEachInitializer("", i => ComponentUtility.PasteComponentValues(i));
				}
				else if(!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
				{
					ForEachInitializer("",  i => JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, i));
				}
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
				Profiler.BeginSample("Sisus.Repaint");
				#endif

				initializerEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
				Profiler.EndSample();
				#endif
			}
		}

		private void ForEachInitializer(string undoName, Action<Component> action)
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
					action(initializer);
				}
			}

			if(initializerEditor)
			{
				initializerEditor.serializedObject.Update();
				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(initializerEditor.GetType().Name + "Repaint");
				Profiler.BeginSample("Sisus.Repaint");
				#endif

				initializerEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
				Profiler.EndSample();
				#endif
			}
		}

		private void ForEachInitializer(string undoName, Action<Object> action)
		{
			if(!string.IsNullOrEmpty(undoName))
			{
				Undo.RecordObjects(initializers, undoName);
			}

			for(int i = 0, count = initializers.Length; i < count; i++)
			{
				var initializer = initializers[i];
				if(initializer)
				{
					action(initializer);
				}
			}

			if(initializerEditor && initializerEditor.target)
			{
				initializerEditor.serializedObject.Update();
				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(initializerEditor.GetType().Name + "Repaint");
				Profiler.BeginSample("Sisus.Repaint");
				#endif

				initializerEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
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

		private enum HelpBoxMessageType
		{
			None,
			TargetInitializedWhenBecomesActive,
			TargetInitializedWhenDeserialized
		}
	}
}