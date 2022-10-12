using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	public sealed class InitializerDrawer : IDisposable
	{
		public static bool ServicesShown
		{
			get => EditorPrefs.GetBool(ServiceVisibilityKey, true);
			set => EditorPrefs.SetBool(ServiceVisibilityKey, value);
		}

		public const string SetInitializerTargetOnScriptsReloadedKey = "InitArgs.SetInitializerTarget";
		public const string ServiceVisibilityKey = "InitArgs.InitializerServiceVisibility";
		private const string InitArgsDefaultLabel = "Init";
		private static readonly Type genericInspectorType;

		private readonly Object[] targets;
		private bool isResponsibleForInitializerEditorLifetime;
		private readonly GameObject[] gameObjects;

		private GUIStyle initArgsFoldoutBackgroundStyle;
		private GUIStyle initArgsFoldoutStyle;
		private GUIStyle noInitArgsLabelStyle;
		private GUIContent addInitializerIcon;
		private GUIContent contextMenuIcon;
		private GUIContent nullGuardDisabledIcon;
		private GUIContent nullGuardPassedIcon;
		private GUIContent nullGuardFailedIcon;
		private readonly GUIContent servicesHiddenIcon = new GUIContent();
		private readonly GUIContent servicesShownIcon = new GUIContent();
		private readonly GUIContent initArgsLabel = new GUIContent();
		private GUIStyle initializerBackgroundStyle = null;
		private GUIStyle noInitializerBackgroundStyle = null;
		private bool hasServiceParameters;

		private Object[] initializers = new Component[1];
		private UnityEditor.Editor initializerEditor;

		[CanBeNull]
		private Object Target => targets[0];

		private bool lockInitializers = false;

		public Action<Rect> OnAddInitializerButtonPressedOverride { get; set; }

		public Object[] Initializers
		{
			get => initializers;

			set
			{
				initializers = value;
				lockInitializers = true;
				initArgsLabel.text = initializers.Length == 0 || initializers[0] == null ? InitArgsDefaultLabel : "Init → " + ObjectNames.NicifyVariableName(Target.GetType().Name);
				GUI.changed = true;
			}
		}

		static InitializerDrawer() => genericInspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GenericInspector");

		public InitializerDrawer(Type clientType, Object[] targets, Type[] initParameterTypes, UnityEditor.Editor initializerEditor = null, GameObject[] gameObjects = null)
		{
			this.targets = targets;

			if(gameObjects == null)
			{
				int count = targets.Length;
				gameObjects = new GameObject[count];

				for(int i = 0; i < count; i++)
				{
					var component = targets[i] as Component;
					gameObjects[i] = component != null ? component.gameObject : null;
				}
			}

			bool[] initServiceParameters = initParameterTypes.Select(ServiceUtility.IsDefiningTypeOfAnyServiceAttribute).ToArray();
			hasServiceParameters = initServiceParameters.Any(b => b);

			this.gameObjects = gameObjects;
			initArgsLabel.text = InitArgsDefaultLabel;
			initArgsLabel.tooltip = GetInitArgumentsTooltip(clientType, initParameterTypes, initServiceParameters);
			this.initializerEditor = initializerEditor;
			isResponsibleForInitializerEditorLifetime = initializerEditor == null;

			if(hasServiceParameters)
			{
				servicesShownIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initServiceParameters, true);
				servicesHiddenIcon.tooltip = GetServiceVisibilityTooltip(initParameterTypes, initServiceParameters, false);
			}
			else
			{
				servicesShownIcon.tooltip = "";
				servicesHiddenIcon.tooltip = "";
			}

			Setup();
		}

		private void Setup()
		{
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

			addInitializerIcon = new GUIContent(EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Initializer"));
			if(Target is StateMachineBehaviour)
			{
				addInitializerIcon.tooltip = "Add State Machine Behaviour Initializer";
			}

			contextMenuIcon = EditorGUIUtility.IconContent("_Menu");
			nullGuardDisabledIcon = EditorGUIUtility.IconContent("DebuggerDisabled");
			nullGuardPassedIcon = EditorGUIUtility.IconContent("Installed@2x");
			nullGuardFailedIcon = EditorGUIUtility.IconContent("DebuggerEnabled");

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
		}

		public static void OnAssemblyCompilationStarted(ref InitializerDrawer drawer, string compilingAssemblyName)
		{
			if(drawer is null || drawer.initializerEditor == null)
			{
				return;
			}

			string initializerAssemblyName = Path.GetFileName(drawer.initializerEditor.GetType().Assembly.Location);
			if(string.Equals(compilingAssemblyName, initializerAssemblyName))
			{
				drawer.Dispose();
				drawer = null;
			}
		}

		public void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;

			GetInitializersOnTargets(out bool hasInitializers, out Object firstInitializer);

			bool mixedInitializers = false;
			if(hasInitializers)
			{
				for(int i = 0, targetCount = targets.Length; i < targetCount; i++)
				{
					if(initializers[i] == null)
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

			if(initializerBackgroundStyle == null)
			{
				Setup();
			}

			EditorGUILayout.BeginVertical(hasInitializers ? initializerBackgroundStyle : noInitializerBackgroundStyle);

			var labelStyle = hasInitializers && !mixedInitializers ? initArgsFoldoutStyle : noInitArgsLabelStyle;
			var headerRect = EditorGUILayout.GetControlRect(false, 20f, labelStyle);

			bool initializerUnfolded = hasInitializers && !mixedInitializers && InternalEditorUtility.GetIsInspectorExpanded(firstInitializer);
			bool guiWasEnabled = GUI.enabled;
			if(!hasInitializers)
			{
				GUI.enabled = false;
			}

			const float ICON_WIDTH = 20f;
			var foldoutRect = headerRect;
			if(!isResponsibleForInitializerEditorLifetime)
			{
				foldoutRect.width = EditorGUIUtility.labelWidth - foldoutRect.x;
			}
			else
			{
				foldoutRect.width -= 38f;
				foldoutRect.x -= 12f;
			}

			foldoutRect.y -= 1f;
			foldoutRect.width -= ICON_WIDTH;

			var addInitializerOrContextMenuRect = headerRect;
			addInitializerOrContextMenuRect.x += addInitializerOrContextMenuRect.width - ICON_WIDTH;
			addInitializerOrContextMenuRect.width = ICON_WIDTH;

			bool setInitializerUnfolded = initializerUnfolded;
			var foldoutClickableRect = foldoutRect;
			foldoutClickableRect.x -= 5f;
			foldoutClickableRect.width +=  10f;

			if(!isResponsibleForInitializerEditorLifetime)
			{
				foldoutClickableRect.x -= 12f;
				foldoutClickableRect.width += 12f;
			}

			if(Event.current.type == EventType.MouseDown && foldoutClickableRect.Contains(Event.current.mousePosition))
			{
				if(Event.current.button == 0)
				{
					setInitializerUnfolded = !setInitializerUnfolded;
				}
				else if(Event.current.button == 1)
				{
					OnInitializerContextMenuButtonPressed(targets, firstInitializer, mixedInitializers, null);
				}

				Event.current.Use();
			}

			GUI.enabled = guiWasEnabled;

			if(setInitializerUnfolded != initializerUnfolded && hasInitializers)
			{
				for(int i = 0, targetCount = targets.Length; i < targetCount; i++)
				{
					var initializer = initializers[i];
					if(initializer != null)
					{
						InternalEditorUtility.SetIsInspectorExpanded(initializer, setInitializerUnfolded);
					}
				}

				initializerUnfolded = setInitializerUnfolded;
				GUIUtility.ExitGUI();
			}

			if(!hasInitializers)
			{
				DrawInitHeader();

				if(GUI.Button(addInitializerOrContextMenuRect, addInitializerIcon, "RL FooterButton"))
				{
					if(OnAddInitializerButtonPressedOverride != null)
					{
						OnAddInitializerButtonPressedOverride.Invoke(addInitializerOrContextMenuRect);
						return;
					}

					AddInitializer(addInitializerOrContextMenuRect);
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
					OnInitializerContextMenuButtonPressed(targets, firstInitializer, mixedInitializers, addInitializerOrContextMenuRect);
				}

				var nullGuardIconRect = addInitializerOrContextMenuRect;
				nullGuardIconRect.y -= 1f;
				nullGuardIconRect.x -= addInitializerOrContextMenuRect.width;

				var initializerEditorOnly = firstInitializer as IInitializerEditorOnly;
				var nullGuard = initializerEditorOnly != null ? initializerEditorOnly.NullArgumentGuard : NullArgumentGuard.None;

				if(initializerEditorOnly != null && GUI.Button(nullGuardIconRect, GUIContent.none, EditorStyles.label))
				{
					OnInitializerNullGuardButtonPressed(nullGuard, nullGuardIconRect);
				}

				bool servicesShown = ServicesShown;
				var serviceVisibilityIconRect = nullGuardIconRect;
				serviceVisibilityIconRect.x -= nullGuardIconRect.width;
				if(hasServiceParameters && GUI.Button(serviceVisibilityIconRect, GUIContent.none, EditorStyles.label))
				{
					servicesShown = !servicesShown;
					ServicesShown = servicesShown;
					EditorPrefs.SetBool(ServiceVisibilityKey, servicesShown);
				}

				if(initializerUnfolded)
				{
					DrawInitializerArguments();
				}

				DrawInitHeader();

				GUI.Label(addInitializerOrContextMenuRect, contextMenuIcon);

				if(initializerEditorOnly != null)
				{
					bool nullGuardPassedAndFullyEnabled;
					GUIContent nullGuardIcon;
					
					bool nullGuardDisabled =
						!nullGuard.IsEnabled(Application.isPlaying ? NullArgumentGuard.RuntimeException : NullArgumentGuard.EditModeWarning)
						|| (!nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs) && PrefabUtility.IsPartOfPrefabAsset(firstInitializer));

					if(nullGuardDisabled)
					{
						nullGuardIcon = nullGuardDisabledIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard) + "\n\nNull argument guard is off.";
						nullGuardPassedAndFullyEnabled = false;
					}
					else if(initializerEditorOnly.HasNullArguments)
					{
						nullGuardIcon = nullGuardFailedIcon;
						nullGuardIcon.tooltip = "Missing argument detected.\n\nIf the argument is allowed to be null set the 'Null Argument Guard' option to 'None'.\n\nIf the missing argument is a service that only becomes available at runtime set the option to 'Runtime Exception'.";
						nullGuardPassedAndFullyEnabled = false;
					}
					else
					{
						if(!string.IsNullOrEmpty(initializerEditorOnly.NullGuardFailedMessage))
						{
							initializerEditorOnly.NullGuardFailedMessage = "";
						}

						nullGuardIcon = nullGuardPassedIcon;
						nullGuardIcon.tooltip = GetTooltip(nullGuard) + "\n\nAll arguments provided.";
						nullGuardPassedAndFullyEnabled = nullGuard.IsEnabled(NullArgumentGuard.EditModeWarning) && nullGuard.IsEnabled(NullArgumentGuard.RuntimeException);
					}

					var iconSizeWas = EditorGUIUtility.GetIconSize();
					EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));

					var guiColorWas = GUI.color;
					if(nullGuardPassedAndFullyEnabled)
					{
						GUI.color = Color.yellow;
					}

					GUI.Label(nullGuardIconRect, nullGuardIcon);
					
					GUI.color = guiColorWas;
					
					if(hasServiceParameters)
					{
						var serviceVisibilityIcon = servicesShown ? servicesShownIcon : servicesHiddenIcon;
						GUI.Label(serviceVisibilityIconRect, serviceVisibilityIcon);
					}

					EditorGUIUtility.SetIconSize(iconSizeWas);
				}
			}

			EditorGUILayout.EndVertical();

			void DrawInitHeader()
			{
				if(Event.current.type != EventType.Repaint)
				{
					return;
				}

				var backgroundRect = headerRect;
				backgroundRect.y -= 3f;
				backgroundRect.x -= 18f;
				backgroundRect.width += 22f;
				initArgsFoldoutBackgroundStyle.Draw(backgroundRect, false, false, false, false);

				if((initializers.Length == 0 || initializers[0] == null) && initArgsLabel.text != InitArgsDefaultLabel)
				{
					initArgsLabel.text = InitArgsDefaultLabel;
					GUI.changed = true;
				}

				foldoutRect.x = 22f;
				labelStyle.Draw(foldoutRect, initArgsLabel, 0, initializerUnfolded);
			}
		}

		private void DrawInitializerArguments()
		{
			if(initializerEditor == null)
			{
				isResponsibleForInitializerEditorLifetime = true;
				UnityEditor.Editor.CreateCachedEditor(initializers, null, ref initializerEditor);
				if(initializerEditor is InitializerEditor setup)
				{
					setup.Setup(false);
				}
			}

			if(initializerEditor is InitializerEditor internalInitializerEditor)
			{
				internalInitializerEditor.serializedObject.Update();
				internalInitializerEditor.DrawArgumentFields();
				internalInitializerEditor.serializedObject.ApplyModifiedProperties();
			}
			else if(initializerEditor.GetType() == genericInspectorType)
			{
				HideScriptReferenceField();
				initializerEditor.OnInspectorGUI();
			}
			else
			{
				initializerEditor.OnInspectorGUI();
			}
		}

		private string GetInitArgumentsTooltip([JetBrains.Annotations.NotNull] Type clientType, [JetBrains.Annotations.NotNull] Type[] initParameterTypes, [JetBrains.Annotations.NotNull] bool[] initServiceParameters)
		{
			var className = clientType.Name;
			var types = initParameterTypes;

			var sb = new StringBuilder();
			sb.Append(className);
			int count = types.Length;
			sb.Append(count switch
			{
				1 => " accepts one Init argument:",
				2 => " accepts two Init arguments:",
				3 => " accepts three Init arguments:",
				4 => " accepts four Init arguments:",
				5 => " accepts five Init arguments:",
				_ => $" accepts {count} Init arguments:"
			});

			for(int i = 0; i < count; i++)
			{
				sb.Append('\n');
				sb.Append(GetInitArgumentTooltipText(types[i], initServiceParameters[i]));
			}

			return sb.ToString();
		}

		private string GetServiceVisibilityTooltip([JetBrains.Annotations.NotNull] Type[] initParameterTypes, [JetBrains.Annotations.NotNull] bool[] initServiceParameters, bool servicesShown)
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

		private string GetInitArgumentTooltipText(Type type, bool isService) => isService ? TypeUtility.ToString(type) + " (Service)" : TypeUtility.ToString(type);

		private static string GetTooltip(NullArgumentGuard guard)
		{
			switch(guard)
			{
				default:
					return "Null Argument Guard:\n☐ Edit Mode Warning\n☐ Runtime Exception\n☐ Enabled For Prefabs";
				case NullArgumentGuard.EditModeWarning:
					return "Null Argument Guard:\n☑ Edit Mode Warning\n☐ Runtime Exception\n☐ Enabled For Prefabs";
				case NullArgumentGuard.RuntimeException:
					return "Null Argument Guard:\n☐ Edit Mode Warning\n☑ Runtime Exception\n☐ Enabled For Prefabs";
				case NullArgumentGuard.EnabledForPrefabs:
					return "Null Argument Guard:\n☐ Edit Mode Warning\n☐ Runtime Exception\n☑ Enabled For Prefabs";
				case NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException:
					return "Null Argument Guard:\n☑ Edit Mode Warning\n☑ Runtime Exception\n☐ Enabled For Prefabs";
				case NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs:
					return "Null Argument Guard:\n☐ Edit Mode Warning\n☑ Runtime Exception\n☑ Enabled For Prefabs";
				case NullArgumentGuard.EditModeWarning | NullArgumentGuard.EnabledForPrefabs:
					return "Null Argument Guard:\n☑ Edit Mode Warning\n☑ Runtime Exception\n☐ Enabled For Prefabs";
				case NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs:
					return "Null Argument Guard:\n☑ Edit Mode Warning\n☑ Runtime Exception\n☑ Enabled For Prefabs";
			}
		}

		private void OnInitializerNullGuardButtonPressed(NullArgumentGuard nullGuard, Rect nullGuardIconRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("None"), nullGuard == NullArgumentGuard.None, () => ForEachInitializer("Disable Null Guard", (init) => init.NullArgumentGuard = NullArgumentGuard.None));
			menu.AddItem(new GUIContent("Edit Mode Warning"), nullGuard.IsEnabled(NullArgumentGuard.EditModeWarning), () => ForEachInitializer("Set Null Guard", (init) => init.NullArgumentGuard = nullGuard.WithFlagToggled(NullArgumentGuard.EditModeWarning)));
			menu.AddItem(new GUIContent("Runtime Exception"), nullGuard.IsEnabled(NullArgumentGuard.RuntimeException), () => ForEachInitializer("Set Null Guard", (init) => init.NullArgumentGuard = nullGuard.WithFlagToggled(NullArgumentGuard.RuntimeException)));
			menu.AddItem(new GUIContent("Enabled For Prefabs"), nullGuard.IsEnabled(NullArgumentGuard.EnabledForPrefabs), () => ForEachInitializer("Set Null Guard", (init) => init.NullArgumentGuard = nullGuard.WithFlagToggled(NullArgumentGuard.EnabledForPrefabs)));
			menu.AddItem(new GUIContent("All"), nullGuard == (NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs), () => ForEachInitializer("Enable Null Guard", (init) => init.NullArgumentGuard = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException | NullArgumentGuard.EnabledForPrefabs));

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
						menu.AddItem(label, false, () => AddComponent(initializerType));
						activeOptions++;
					}
				}

				if(activeOptions == 1)
				{
					AddComponent(initializerTypes[0]);
				}
				else
				{
					menu.DropDown(addButtonRect);
				}
			}
			else if((target is MonoBehaviour monoBehaviour && MonoScript.FromMonoBehaviour(monoBehaviour) is MonoScript monoScript) || Find.Script(targetType, out monoScript))
			{
				var menu = new GenericMenu();

				menu.AddItem(new GUIContent("Generate Initializer"), false, () =>
				{
					string initializerPath = ScriptGenerator.CreateInitializer(monoScript);

					var addScriptMethod = typeof(InternalEditorUtility).GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
					if(addScriptMethod is null)
					{
						#if DEV_MODE
						Debug.LogWarning("Method InternalEditorUtilityAddScriptComponentUncheckedUndoable not found.");
						#endif
						return;
					}

					var initializerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(initializerPath);
					var initializerGuid = AssetDatabase.AssetPathToGUID(initializerPath);

					EditorPrefs.SetString(SetInitializerTargetOnScriptsReloadedKey, initializerGuid + ":" + string.Join(";", targets.Select(t => t.GetInstanceID())));

					if(initializerScript != null)
					{
						Debug.Log($"Initializer class created at {initializerPath}.", initializerScript);

						var gameObject = gameObjects[0];
						addScriptMethod.Invoke(null, new Object[] { gameObject, initializerScript });
					}

					GUI.changed = true;
				});

				menu.DropDown(addButtonRect);
			}
			else
			{
				EditorUtility.DisplayDialog("Initializer Not Found", $"No Initializer class was found for {targetType.Name}.", "Ok");
			}

			GUI.changed = true;
			if(Event.current != null)
			{
				GUIUtility.ExitGUI();
			}
		}

		private bool IsTargetedByInitializerOfType(Type initializerType)
		{
			for(int i = 0, count = gameObjects.Length; i < count; i++)
			{
				var gameObject = gameObjects[i];
				foreach(var initializer in gameObject.GetComponents<IInitializer>())
				{
					if(initializer.GetType() == initializerType && initializer.Target == targets[i])
					{
						return true;
					}
				}
			}

			return false;
		}

		private void AddComponent(Type type)
		{
			foreach(var gameObject in gameObjects)
			{
				gameObject.AddComponent(type);
			}

			GUI.changed = true;
		}

		private void OnInitializerContextMenuButtonPressed(Object[] targets, Object firstInitializer, bool mixedInitializers, Rect? toggleInitializerRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Reset"), false, () =>
			{
				EditorGUIUtility.editingTextField = false;

				var tempGameObject = new GameObject();
				tempGameObject.SetActive(false);
				var newInstance = tempGameObject.AddComponent(firstInitializer.GetType());
				newInstance.hideFlags = HideFlags.HideInInspector;
				(newInstance as IInitializer).Target = Target;
				ComponentUtility.CopyComponent(newInstance);
				Object.DestroyImmediate(tempGameObject);

				ForEachInitializer("", (index, initializer) =>
				{
					ComponentUtility.PasteComponentValues(initializer);
					(initializer as IInitializer).Target = targets[index];
					initializer.hideFlags = targets[index] != null ? HideFlags.HideInInspector : HideFlags.None;
				});
			});

			menu.AddSeparator("");

			menu.AddItem(new GUIContent("Remove"), false, () =>
			{
				if(initializerEditor != null)
				{
					Undo.DestroyObjectImmediate(initializerEditor);
				}

				ForEachInitializer("Remove Initializer", Undo.DestroyObjectImmediate);
			});

			menu.AddItem(new GUIContent("Copy"), false, () => ComponentUtility.CopyComponent(firstInitializer as Component));

			menu.AddItem(new GUIContent("Paste"), false, () =>
			{
				EditorGUIUtility.editingTextField = false;
				ForEachInitializer("", (i)=> ComponentUtility.PasteComponentValues(i));
			});

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
		}

		private void ForEachInitializer(string undoName, Action<IInitializerEditorOnly> action)
		{
			if(!string.IsNullOrEmpty(undoName))
			{
				Undo.RecordObjects(initializers, undoName);
			}

			for(int i = 0, count = initializers.Length; i < count; i++)
			{
				if(initializers[i] is IInitializerEditorOnly initializer)
				{
					action(initializer);
				}
			}

			if(initializerEditor != null && initializerEditor.serializedObject != null)
			{
				initializerEditor.serializedObject.Update();
				initializerEditor.Repaint();
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
				if(initializer != null)
				{
					action(i, initializer);
				}
			}

			if(initializerEditor != null && initializerEditor.serializedObject != null)
			{
				initializerEditor.serializedObject.Update();
				initializerEditor.Repaint();
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
				if(initializer != null)
				{
					action(initializer);
				}
			}

			if(initializerEditor != null && initializerEditor.serializedObject != null)
			{
				initializerEditor.serializedObject.Update();
				initializerEditor.Repaint();
			}
		}

		public void Dispose()
		{
			if(isResponsibleForInitializerEditorLifetime && initializerEditor != null)
			{
				Object.DestroyImmediate(initializerEditor);
				initializerEditor = null;
				isResponsibleForInitializerEditorLifetime = false;
			}
		}

		private void GetInitializersOnTargets(out bool hasInitializers, out Object firstInitializer)
		{
			if(initializerEditor != null)
			{
				initializers = initializerEditor.targets;
				firstInitializer = initializers[0];
				hasInitializers = firstInitializer != null;
				return;
			}

			if(lockInitializers)
			{
				firstInitializer = initializers.Length == 0 ? null : initializers[0];
				hasInitializers = firstInitializer != null;
				if(!hasInitializers && initArgsLabel.text != InitArgsDefaultLabel)
				{
					initArgsLabel.text = InitArgsDefaultLabel;
					GUI.changed = true;
				}
				return;
			}

			int gameObjectCount = gameObjects.Length;
			int targetCount = targets.Length;
			hasInitializers = false;
			Array.Resize(ref initializers, targetCount);

			firstInitializer = null;
			for(int i = 0; i < gameObjectCount; i++)
			{
				initializers[i] = null;

				var gameObject = gameObjects[i];
				if(!InitializerEditors.InitializersOnInspectedObjects.TryGetValue(gameObject, out var initializersOnGameObject))
				{
					continue;
				}

				foreach(var initializer in initializersOnGameObject)
				{
					if(!ShouldDrawInitializerEmbedded(initializer))
					{
						continue;
					}

					var initializerComponent = initializer as Component;
					if(initializerComponent == null)
					{
						continue;
					}

					initializers[i] = initializerComponent;
					hasInitializers = true;

					if(firstInitializer == null)
					{
						firstInitializer = initializerComponent;
					}
				}
			}
		}

		private bool ShouldDrawInitializerEmbedded(IInitializer initializer)
		{
			var initializerTarget = initializer.Target;

			foreach(var target in targets)
			{
				if(initializerTarget == target)
				{
					return true;
				}
			}

			if(initializerTarget is Animator animator)
			{
				foreach(var stateMachineBehaviour in animator.GetBehaviours<StateMachineBehaviour>())
				{
					if(targets[0].GetType() == stateMachineBehaviour.GetType())
					{
						return true;
					}
				}
			}

			return false;
		}

		private static void HideScriptReferenceField() => GUILayout.Space(-EditorGUIUtility.singleLineHeight - 1f);
	}
}