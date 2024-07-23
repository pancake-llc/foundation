using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sisus.Init.Internal;
using Sisus.Init.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Sisus.Init.Internal.InitializableUtility;
using static Sisus.Init.Internal.InitializerUtility;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	internal static class InitializerEditorUtility
	{
		internal static readonly Color NullGuardFailedColor = new(1f, 0.82f, 0f, 1f);
		internal static readonly Color NullGuardWarningColor = new Color(0.4f, 0.6f, 1f, 1f);

		private static readonly GUIContent clientNullTooltip = new("", "A new instance will be added to this GameObject during initialization.");
		private static readonly GUIContent clientPrefabTooltip = new("", "A new instance will be created by cloning this prefab during initialization.");
		private static readonly GUIContent clientInstantiateTooltip = new("", "A new instance will be created by cloning this scene object during initialization.");
		private static readonly GUIContent clientNotInitializableTooltip = new("", "Can not inject arguments to client because it does not implement IInitializable.");
		private static GUIContent warningIcon;
		private static GUIContent prefabIcon;
		private static GUIContent gameObjectIcon;
		private static GUIContent instantiateOverlayIcon;
		private static GUIContent scriptableObjectIcon;

		private static readonly HashSet<Type> initializableEditors = new(12)
		{
			{ typeof(InitializableT1Editor) },
			{ typeof(InitializableT2Editor) },
			{ typeof(InitializableT3Editor) },
			{ typeof(InitializableT4Editor) },
			{ typeof(InitializableT5Editor) },
			{ typeof(InitializableT6Editor) },
			{ typeof(InitializableT7Editor) },
			{ typeof(InitializableT8Editor) },
			{ typeof(InitializableT9Editor) },
			{ typeof(InitializableT10Editor) },
			{ typeof(InitializableT11Editor) },
			{ typeof(InitializableT12Editor) }
		};

		private static readonly Dictionary<int, Type> initializableEditorsByArgumentCount = new(12)
		{
			{ 1,  typeof(InitializableT1Editor) },
			{ 2,  typeof(InitializableT2Editor) },
			{ 3,  typeof(InitializableT3Editor) },
			{ 4,  typeof(InitializableT4Editor) },
			{ 5,  typeof(InitializableT5Editor) },
			{ 6,  typeof(InitializableT6Editor) },
			{ 7,  typeof(InitializableT7Editor) },
			{ 8,  typeof(InitializableT8Editor) },
			{ 9,  typeof(InitializableT9Editor) },
			{ 10, typeof(InitializableT10Editor) },
			{ 11, typeof(InitializableT11Editor) },
			{ 12, typeof(InitializableT12Editor) }
		};

		private static readonly Dictionary<Type, Type> targetEditorDecoratorTypes = new()
		{
			{ typeof(InitializerBaseInternal<>), typeof(InitializerEditor) },
			{ typeof(CustomInitializer<,>), typeof(CustomInitializerEditor) },
			{ typeof(CustomInitializer<,,>), typeof(CustomInitializerEditor) },
			{ typeof(CustomInitializer<,,,>), typeof(CustomInitializerEditor) },
			{ typeof(CustomInitializer<,,,,>), typeof(CustomInitializerEditor) },
			{ typeof(CustomInitializer<,,,,,>), typeof(CustomInitializerEditor) },
			{ typeof(CustomInitializer<,,,,,,>), typeof(CustomInitializerEditor) },
			{ typeof(InitializerBase<,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(InitializerBase<,,,,,,,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(Initializer<,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(Initializer<,,,,,,,,,,,,>), typeof(InitializerEditor) },
			{ typeof(WrapperInitializerBase<,,>), typeof(WrapperInitializerEditor) },
			{ typeof(WrapperInitializerBase<,,,>), typeof(WrapperInitializerEditor) },
			{ typeof(WrapperInitializerBase<,,,,>), typeof(WrapperInitializerEditor) },
			{ typeof(WrapperInitializerBase<,,,,,>), typeof(WrapperInitializerEditor) },
			{ typeof(WrapperInitializerBase<,,,,,,>), typeof(WrapperInitializerEditor) },
			{ typeof(WrapperInitializerBase<,,,,,,,>), typeof(WrapperInitializerEditor) },
			{ typeof(Wrapper<>), typeof(WrapperEditor) },
			{ typeof(ScriptableObjectInitializerBase<,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,,,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,,,,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,,,,,>), typeof(InitializerEditor) },
			{ typeof(ScriptableObjectInitializer<,,,,,,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,,,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializerBase<,,,,,,>), typeof(InitializerBaseEditor) },
			{ typeof(StateMachineBehaviourInitializer<,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializer<,,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializer<,,,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializer<,,,,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializer<,,,,,>), typeof(InitializerEditor) },
			{ typeof(StateMachineBehaviourInitializer<,,,,,,>), typeof(InitializerEditor) },
			{ typeof(InactiveInitializerBaseInternal<>), typeof(InactiveInitializerEditor) },
			{ typeof(InactiveInitializer), typeof(InactiveInitializerEditor) }
		};

		private static readonly Dictionary<int, string[]> propertyNamesByArgumentCount = new(12)
		{
			{ 1, new string[] {"argument"} },
			{ 2, new string[] {"firstArgument", "secondArgument"} },
			{ 3, new string[] {"firstArgument", "secondArgument", "thirdArgument"} },
			{ 4, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument"} },
			{ 5, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument"} },
			{ 6, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument"} },
			{ 7, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument" } },
			{ 8, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument" } },
			{ 9, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument" } },
			{ 10, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument" } },
			{ 11, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument", "eleventhArgument" } },
			{ 12, new string[] {"firstArgument", "secondArgument", "thirdArgument", "fourthArgument", "fifthArgument", "sixthArgument", "seventhArgument", "eighthArgument", "ninthArgument", "tenthArgument", "eleventhArgument", "twelfthArgument" } }
		};

		/// <summary>
		/// Maps initializer Init argument property names to their zero-based Init argument indexes.
		/// </summary>
		private static readonly Dictionary<string, int> propertyNameToInitParameterIndex = new(13)
		{
			{ "argument", 0 },
			{ "firstArgument", 0 },
			{ "secondArgument", 1 },
			{ "thirdArgument", 2 },
			{ "fourthArgument", 3 },
			{ "fifthArgument",  4 },
			{ "sixthArgument", 5 },
			{ "seventhArgument", 6 },
			{ "eighthArgument", 7 },
			{ "ninthArgument", 8 },
			{ "tenthArgument", 9 },
			{ "eleventhArgument", 10 },
			{ "twelfthArgument", 11 }
		};

		private static Color ObjectFieldBackgroundColor => EditorGUIUtility.isProSkin ? new Color32(42, 42, 42, 255) : new Color32(237, 237, 237, 255);

		static InitializerEditorUtility()
		{
			foreach(var editorDecoratorType in TypeCache.GetTypesWithAttribute<CustomEditorDecoratorAttribute>())
			{
				var attribute = editorDecoratorType.GetCustomAttribute<CustomEditorDecoratorAttribute>();
				targetEditorDecoratorTypes[attribute.TargetType] = editorDecoratorType;
			}
		}

		internal static bool IsInitializableEditorType(Type editorType) => initializableEditors.Contains(editorType);

		internal static bool HasAnyInitializerTypes(Type clientType) => GetInitializerTypes(clientType).Any();

		internal static IEnumerable<Type> GetInitializerTypes(Type clientType)
		{
			foreach(var initializerType in TypeCache.GetTypesDerivedFrom(typeof(IInitializer)))
			{
				if(IsInitializerFor(initializerType, clientType))
				{
					yield return initializerType;
				}
			}
		}

		internal static Type[] GetInitParameterTypes([DisallowNull] Object initializable)
		{
			if(TryGetInitializer(initializable, out IInitializer initializer))
			{
				foreach(Type interfaceType in initializer.GetType().GetInterfaces())
				{
					if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
					{
						return interfaceType.GetGenericArguments().Skip(1).ToArray();
					}
				}
			}

			foreach(Type interfaceType in initializable.GetType().GetInterfaces())
			{
				if(interfaceType.IsGenericType && argumentCountsByIInitializableTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					return interfaceType.GetGenericArguments();
				}
			}

			foreach(Type possibleInitializerType in GetInitializerTypes(initializable.GetType()))
			{
				foreach(Type interfaceType in possibleInitializerType.GetInterfaces())
				{
					if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
					{
						return interfaceType.GetGenericArguments().Skip(1).ToArray();
					}
				}
			}			

			return Array.Empty<Type>();
		}

		/// <returns> Init parameter types if was able to determine them; otherwise, an empty array. </returns>
		internal static Type[] GetInitParameterTypes([AllowNull] Type initializerType, [AllowNull] Type clientType)
		{
			if(clientType is not null)
			{
				if(typeof(IWrapper).IsAssignableFrom(clientType))
				{
					foreach((Type wrappedType, Type[] wrapperTypes) in Find.typeToWrapperTypes)
					{
						if(Array.IndexOf(wrapperTypes, clientType) != -1)
						{
							clientType = wrappedType;
							break;
						}
					}
				}

				if(Find.typeToWrapperTypes.ContainsKey(clientType))
				{
					foreach(var constructor in clientType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
					{
						var parameters = constructor.GetParameters();
						if(parameters.Length > 0)
						{
							return parameters.Select(p => p.ParameterType).ToArray();
						}
					}
				}
			}


			if(initializerType is not null)
			{
				foreach(Type interfaceType in initializerType.GetInterfaces())
				{
					if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
					{
						return interfaceType.GetGenericArguments().Skip(1).ToArray();
					}
				}
			}

			if(clientType is not null)
			{
				foreach(Type interfaceType in clientType.GetInterfaces())
				{
					if(interfaceType.IsGenericType && argumentCountsByIInitializableTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
					{
						return interfaceType.GetGenericArguments();
					}
				}

				foreach(Type possibleInitializerType in GetInitializerTypes(clientType))
				{
					foreach(Type interfaceType in possibleInitializerType.GetInterfaces())
					{
						if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
						{
							return interfaceType.GetGenericArguments().Skip(1).ToArray();
						}
					}
				}

				if(!typeof(Object).IsAssignableFrom(clientType))
				{
					foreach(var constructor in clientType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
					{
						var parameters = constructor.GetParameters();
						if(parameters.Length > 0)
						{
							return parameters.Select(p => p.ParameterType).ToArray();
						}
					}
				}
			}

			return Array.Empty<Type>();
		}

		internal static Type GetClientType([AllowNull] Type initializerType)
		{
			foreach(Type interfaceType in initializerType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					return interfaceType.GetGenericArguments()[0];
				}
			}

			return null;
		}

		//internal static bool IsGenericIInitializerType(Type interfaceType) => argumentCountsByIInitializerTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition());

		internal static bool IsGenericIInitializableType(Type interfaceType) => interfaceType.IsGenericType && argumentCountsByIInitializableTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition());

		internal static bool TryGetEditorOverrideType(Type inspectedType, out Type editorType)
		{
			if(typeof(MonoBehaviour).IsAssignableFrom(inspectedType)
			|| typeof(ScriptableObject).IsAssignableFrom(inspectedType)
			|| typeof(StateMachineBehaviour).IsAssignableFrom(inspectedType))
			{
				// Don't use InitializableEditor for value providers that are drawn inlined
				// within the AnyPropertyDrawer - we don't want the Init section to appear for those.
				if(typeof(ScriptableObject).IsAssignableFrom(inspectedType) && inspectedType.IsDefined(typeof(ValueProviderMenuAttribute)))
				{
					editorType = null;
					return false;
				}

				for(var typeOrBaseType = inspectedType; typeOrBaseType != null; typeOrBaseType = typeOrBaseType.BaseType)
				{
					if(typeOrBaseType.IsGenericType
					? targetEditorDecoratorTypes.TryGetValue(typeOrBaseType.GetGenericTypeDefinition(), out editorType)
					: targetEditorDecoratorTypes.TryGetValue(typeOrBaseType, out editorType))
					{
						return true;
					}
				}

				int initArgumentCount = GetClientInitArgumentCount(inspectedType);
				if(initializableEditorsByArgumentCount.TryGetValue(initArgumentCount, out editorType))
				{
					return true;
				}
			}
			else if(targetEditorDecoratorTypes.TryGetValue(inspectedType, out editorType))
			{
				return true;
			}

			bool hasAnyInitilizer = false;

			foreach(var initializerType in TypeCache.GetTypesDerivedFrom<IInitializer>())
			{
				if(initializerType.IsAbstract)
				{
					continue;
				}

				foreach(Type interfaceType in initializerType.GetInterfaces())
				{
					if(!interfaceType.IsGenericType)
					{
						continue;
					}

					if(!argumentCountsByIInitializerTypeDefinition.TryGetValue(interfaceType.GetGenericTypeDefinition(), out int argumentCount))
					{
						continue;
					}

					var initializerClientType = interfaceType.GetGenericArguments()[0];
					if(initializerClientType.IsAssignableFrom(inspectedType)
						&& (initializerClientType != typeof(object) || inspectedType == typeof(object)))
					{
						if(initializableEditorsByArgumentCount.TryGetValue(argumentCount, out editorType))
						{
							return true;
						}

						hasAnyInitilizer = true;
					}

					if(Find.typeToWrapperTypes.TryGetValue(initializerClientType, out Type[] wrapperTypes)
						&& Array.IndexOf(wrapperTypes, inspectedType) != -1)
					{
						if(initializableEditorsByArgumentCount.TryGetValue(argumentCount, out editorType))
						{
							return true;
						}

						hasAnyInitilizer = true;
					}
				}
			}

			if(hasAnyInitilizer)
			{
				editorType = typeof(InitializableEditor);
				return true;
			}

			return false;
		}

		internal static int GetArgumentCountFromIInitializerType(Type genericIInitializerType)
		{
			var genericTypeDefinition = genericIInitializerType.GetGenericTypeDefinition();
			return argumentCountsByIInitializerTypeDefinition.TryGetValue(genericTypeDefinition, out int argumentCount) ? argumentCount : genericTypeDefinition.GetGenericArguments().Length - 1;
		}

		internal static void AddInitializer(Object[] clients, Type initializerType)
		{
			GUI.changed = true;
			int count = clients.Length;

			if(typeof(Component).IsAssignableFrom(initializerType))
			{
				for(int i = 0; i < count; i++)
				{
					var component = clients[i] as Component;
					if(component != null)
					{
						IInitializer initializer = Undo.AddComponent(component.gameObject, initializerType) as IInitializer;
						initializer.Target = component;
					}
				}

				return;
			}

			if(typeof(ScriptableObject).IsAssignableFrom(initializerType))
			{
				var selectionWas = Selection.objects;

				for(int i = 0; i < count; i++)
				{
					var scriptableObjectClient = clients[i] as ScriptableObject;
					if(scriptableObjectClient != null)
					{
						const string UNDO_NAME = "Add Initializer";
						Undo.RecordObject(scriptableObjectClient, UNDO_NAME);

						var initializerInstance = ScriptableObject.CreateInstance(initializerType);
						initializerInstance.name = "Initializer";
						Undo.RegisterCreatedObjectUndo(initializerInstance, UNDO_NAME);

						(initializerInstance as IInitializer).Target = scriptableObjectClient;
						AssetDatabase.StartAssetEditing();
						string path = AssetDatabase.GetAssetPath(scriptableObjectClient);
						EditorUtility.SetDirty(scriptableObjectClient);
						AssetDatabase.AddObjectToAsset(initializerInstance, path);
						AssetDatabase.StopAssetEditing();
						AssetDatabase.ImportAsset(path);
						
						foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))
						{
							if(asset.GetType() == initializerType)
							{
								initializerInstance = asset as ScriptableObject;
								break;
							}
						}

						if(scriptableObjectClient is IInitializableEditorOnly initializableEditorOnly)
						{
							Undo.RecordObject(scriptableObjectClient, UNDO_NAME);
							initializableEditorOnly.Initializer = initializerInstance as IInitializer;
						}
					}

					Selection.objects = selectionWas;
				}
			}
		}

		/// <param name="targets"> The top-level components or scriptable objects being inspected. </param>
		/// <param name="client"> The object for which the initializer should be generated. </param>
		public static void GenerateAndAttachInitializer(Object[] targets, object client)
		{
			GUI.changed = true;

			string initializerPath = ScriptGenerator.CreateInitializer(client);
			var initializerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(initializerPath);
			
			Debug.Log($"Initializer class created at \"{initializerPath}\".", initializerScript);
			
			var initializerGuid = AssetDatabase.AssetPathToGUID(initializerPath);
			EditorPrefs.SetString(InitializerGUI.SetInitializerTargetOnScriptsReloadedEditorPrefsKey, initializerGuid + ":" + string.Join(";", targets.Select(t => t.GetInstanceID())));

			if(initializerScript is null)
			{
				return;
			}

			var addScriptMethod = typeof(InternalEditorUtility).GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if(addScriptMethod is null)
			{
				#if DEV_MODE
				Debug.LogWarning("Method InternalEditorUtility.AddScriptComponentUncheckedUndoable not found.");
				#endif
				return;
			}

			foreach(var target in targets)
			{
				if(client is Component component)
				{
					addScriptMethod.Invoke(null, new Object[] { component.gameObject, initializerScript });
				}
			}
		}

		private static bool IsInitializerFor(Type initializerType, Type clientType)
		{
			if(initializerType.IsAbstract)
			{
				return false;
			}

			foreach(Type interfaceType in initializerType.GetInterfaces())
			{
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
				if(!argumentCountsByIInitializerTypeDefinition.ContainsKey(genericTypeDefinition))
				{
					continue;
				}

				var initializerClientType = interfaceType.GetGenericArguments()[0];
				if(initializerClientType.IsAssignableFrom(clientType) && (initializerClientType != typeof(object) || clientType == typeof(object)))
				{
					return true;
				}
				
				if(Find.typeToWrapperTypes.TryGetValue(initializerClientType, out Type[] wrapperTypes)
					&& Array.IndexOf(wrapperTypes, clientType) != -1)
				{
					return true;
				}
			}

			return false;
		}

		internal static bool CanAssignUnityObjectToField(Type type)
		{
			if(typeof(Object).IsAssignableFrom(type))
			{
				return true;
			}

			if(!type.IsInterface)
			{
				return false;
			}

			foreach(var derivedType in TypeCache.GetTypesDerivedFrom(type))
			{
				if(typeof(Object).IsAssignableFrom(derivedType) && !derivedType.IsAbstract)
				{
					return true;
				}
			}

			return false;
		}

		internal static void DrawClientField(Rect rect, SerializedProperty client, GUIContent clientLabel, bool isInitializable)
		{
			var fieldRect = EditorGUI.PrefixLabel(rect, new GUIContent(" "));
			if(fieldRect.width < 20f)
			{
				fieldRect.width = 20f;
				fieldRect.x = rect.xMax - 20f;
			}

			var reference = client.objectReferenceValue;

			EditorGUI.ObjectField(fieldRect, client, GUIContent.none);

			bool mouseovered = rect.Contains(Event.current.mousePosition) && DragAndDrop.visualMode != DragAndDropVisualMode.None;

			fieldRect.x += 2f;
			fieldRect.width -= 21f;
			fieldRect.y += 2f;
			fieldRect.height -= 3f;

			if(!isInitializable && !mouseovered && TryGetTintForNullGuardResult(NullGuardResult.ClientNotSupported, out Color setGuiColor))
			{
				GUI.color = setGuiColor;
			}

			if(reference == null)
			{
				if(mouseovered)
				{
					return;
				}

				clientLabel.tooltip = isInitializable ? clientNullTooltip.text : clientNotInitializableTooltip.text;

				EditorGUI.DrawRect(fieldRect, ObjectFieldBackgroundColor);
				GUI.Label(fieldRect, clientLabel);
			}
			else
			{
				Component component = reference as Component;
				var gameObject = component != null ? component.gameObject : null;
				bool isPrefab = gameObject != null && !gameObject.scene.IsValid();
				bool isSceneObject = gameObject != null && gameObject.scene.IsValid();
				bool isScriptableObject = reference is ScriptableObject;

				GUIContent icon;
				if(!isInitializable)
				{
					if(warningIcon == null)
					{
						warningIcon = EditorGUIUtility.IconContent("Warning");
					}

					icon = warningIcon;
				}
				else if(isPrefab)
				{
					if(prefabIcon == null)
					{
						prefabIcon = EditorGUIUtility.IconContent("Prefab Icon");
					}

					if(instantiateOverlayIcon == null)
					{
						instantiateOverlayIcon = EditorGUIUtility.IconContent("PrefabOverlayAdded Icon");
					}

					icon = prefabIcon;
				}
				else if(isSceneObject)
				{
					if(gameObjectIcon == null)
					{
						gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon");
					}

					if(instantiateOverlayIcon == null)
					{
						instantiateOverlayIcon = EditorGUIUtility.IconContent("PrefabOverlayAdded Icon");
					}

					icon = gameObjectIcon;
				}
				else if(isScriptableObject)
				{
					if(scriptableObjectIcon == null)
					{
						scriptableObjectIcon = EditorGUIUtility.IconContent("ScriptableObject Icon");
					}

					icon = scriptableObjectIcon;
				}
				else
				{
					icon = GUIContent.none;
				}

				string tooltip = GetReferenceTooltip(client.serializedObject.targetObject, reference, isInitializable).tooltip;
				clientLabel.tooltip = tooltip;
				icon.tooltip = tooltip;

				GUI.Label(fieldRect, new GUIContent("", clientLabel.tooltip));

				var iconSize = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));
				
				var iconRect = fieldRect;
				iconRect.y -= 4f;
				iconRect.x -= 22f;
				iconRect.width = 20f;
				iconRect.height = 20f;
				if(GUI.Button(iconRect, icon, EditorStyles.label))
				{
					EditorGUIUtility.PingObject(gameObject != null ? gameObject : reference);
				}

				GUI.Label(iconRect, instantiateOverlayIcon);

				EditorGUIUtility.SetIconSize(iconSize);
			}

			GUI.color = Color.white;
		}

		internal static GUIContent GetReferenceTooltip(Object objectWithField, Object reference, bool isInitializable)
		{
			if(!isInitializable)
			{
				return clientNotInitializableTooltip;
			}

			if(reference == null)
			{
				return clientNullTooltip;
			}

			Component component = reference as Component;
			if(component == null)
			{
				return GUIContent.none;
			}

			var gameObject = component.gameObject;
			if(gameObject == null)
			{
				return GUIContent.none;
			}

			var gameObjectWithField = objectWithField is Component componentWithField ? componentWithField.gameObject : null;
			if(gameObjectWithField == null || gameObjectWithField == gameObject)
			{
				return GUIContent.none;
			}

			bool isPrefab = !gameObject.scene.IsValid();
			return isPrefab ? clientPrefabTooltip : clientInstantiateTooltip;
		}

		internal static GUIContent GetLabel([DisallowNull] Type type)
		{
			if(!(type.GetCustomAttribute<AddComponentMenu>() is AddComponentMenu addComponentMenu))
			{
				return GetLabel(TypeUtility.ToString(type));
			}

			string menuPath = addComponentMenu.componentMenu;
			if(string.IsNullOrEmpty(menuPath))
			{
				return GetLabel(TypeUtility.ToString(type));
			}

			int nameStart = menuPath.LastIndexOf('/') + 1;
			return new GUIContent(nameStart <= 0 ? menuPath : menuPath.Substring(nameStart));
		}

		internal static GUIContent GetLabel(string unnicifiedTypeOrFieldName)
		{
			unnicifiedTypeOrFieldName = ObjectNames.NicifyVariableName(unnicifiedTypeOrFieldName);

			if(unnicifiedTypeOrFieldName.StartsWith("I "))
			{
				unnicifiedTypeOrFieldName = unnicifiedTypeOrFieldName.Substring(2);
			}

			return new GUIContent(unnicifiedTypeOrFieldName);
		}

		[return: MaybeNull]
		public static bool TryGetInitParameterAttributesFromMetadata(string initializerPropertyName, Type parameterType, Type metadataClass, [NotNullWhen(true)][MaybeNullWhen(false)] out Attribute[] results)
		{
			// "firstArgument" => 0, "secondArgument" => 1 etc.
			if(!propertyNameToInitParameterIndex.TryGetValue(initializerPropertyName, out int parameterIndex))
			{
				FieldInfo[] fields = metadataClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if(parameterIndex < fields.Length)
				{
					var fieldAtIndex = fields[parameterIndex];
					if(fieldAtIndex.FieldType == parameterType)
					{
						results = Attribute.GetCustomAttributes(fieldAtIndex, typeof(Attribute));
						return results.Length > 0;
					}
				}
			}

			if(!TryGetArgumentTargetFieldName(metadataClass, parameterType, parameterIndex, out string fieldName))
			{
				results = null;
				return false;
			}

			var field = metadataClass.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			results = Attribute.GetCustomAttributes(field, typeof(Attribute));
			return results.Length > 0;
		}

		#if ODIN_INSPECTOR
		internal static bool TryGetOdinDrawer(GUIContent label, [DisallowNull] SerializedProperty anyProperty, [DisallowNull] Type argumentType, PropertyTree odinPropertyTree, out InspectorProperty odinDrawer)
		{
			SerializedProperty valueProperty;
			if(typeof(Object).IsAssignableFrom(argumentType))
			{
				valueProperty = anyProperty.FindPropertyRelative(nameof(Any<object>.reference));
			}
			else
			{
				valueProperty = anyProperty.FindPropertyRelative(nameof(Any<object>.value));
				if(valueProperty is null)
				{
					valueProperty = anyProperty.FindPropertyRelative(nameof(Any<object>.reference));
				}
			}

			odinDrawer = odinPropertyTree.GetPropertyAtUnityPath(valueProperty.propertyPath);
			if(odinDrawer is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"Failed to get InspectorProperty from {odinPropertyTree.TargetType.Name} path {valueProperty.propertyPath}.");
				#endif

				return false;
			}

			odinDrawer.Label = label;
			odinDrawer.AnimateVisibility = false;
		  	return true;
		}
		#endif

		public static InitParameterGUI[] CreateParameterGUIs(SerializedObject serializedObject, Type clientType, Type[] argumentTypes)
			=> GetPropertyDrawerData(serializedObject, clientType, argumentTypes, GetAnyFieldSerializedProperties(serializedObject, argumentTypes));

		public static SerializedProperty[] GetAnyFieldSerializedProperties(SerializedObject serializedObject)
			=> GetAnyFieldSerializedProperties(serializedObject, GetInitParameterTypes(serializedObject.targetObject));

		public static SerializedProperty[] GetAnyFieldSerializedProperties(SerializedObject serializedObject, Type[] argumentTypes)
		{
			int count = argumentTypes.Length;
			var results = new SerializedProperty[count];
			string[] usualPropertyNames;
			if(!propertyNamesByArgumentCount.TryGetValue(count, out usualPropertyNames))
			{
				#if DEV_MODE
				if(count != 0)
				{
					Debug.LogWarning($"propertyNamesByArgumentCount[{count}] value missing");
				}
				#endif

				usualPropertyNames = Array.Empty<string>();
			}

			for(int i = 0; i < count; i++)
			{
				var serializedProperty = serializedObject.FindProperty(usualPropertyNames[i]);
				if(serializedProperty is not null)
				{
					results[i] = serializedProperty;
					continue;
				}

				var argumentAnyType = typeof(Any<>).MakeGenericType(argumentTypes[i]);
				serializedProperty = serializedObject.GetIterator();
				serializedProperty.NextVisible(true);
				while(serializedProperty.NextVisible(false))
				{
					if(serializedProperty.GetMemberInfo() is FieldInfo field && field.FieldType == argumentAnyType)
					{
						results[i] = serializedProperty;
						break;
					}
				}
			}
			
			return results;
		}

		internal static Type GetMetaDataClassType(Type initializerType) => initializerType.GetNestedType(InitializerEditor.InitArgumentMetadataClassName, BindingFlags.Public | BindingFlags.NonPublic);

		internal static string GetMemberNameInInitializer([DisallowNull] Type metadataClass, string nameInMetadata)
		{
			var members = GetArgumentMetadata(metadataClass);
			if(!propertyNamesByArgumentCount.TryGetValue(members.Length - 1, out var namesInInitializer))
			{
				return nameInMetadata;
			}

			for(int i = 0; i < members.Length; i++)
			{
				if(string.Equals(members[i].Name, nameInMetadata, StringComparison.OrdinalIgnoreCase))
				{
					return namesInInitializer[i];
				}
			}

			#if DEV_MODE
			Debug.Log($"\"{nameInMetadata}\" not found among options: {string.Join(", ", members.Select(m => m.Name))}.");
			#endif

			return namesInInitializer.Length == 1 ? namesInInitializer[0] : nameInMetadata;
		}

		private static MemberInfo[] GetArgumentMetadata([DisallowNull] Type metadataClass)
		{ 
			var members = metadataClass.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			Array.Sort(members, CompareOrderOfDefinition);
			return members;
			static int CompareOrderOfDefinition(MemberInfo x, MemberInfo y) => x.MetadataToken.CompareTo(y.MetadataToken);
		}

		private static InitParameterGUI[] GetPropertyDrawerData(SerializedObject serializedObject, Type clientType, Type[] argumentTypes, SerializedProperty[] serializedProperties)
		{
			var target = serializedObject.targetObject;
			var initializerType = target.GetType();
			var metadataClass = GetMetaDataClassType(initializerType);

			int argumentCount = argumentTypes.Length;
			var results = new InitParameterGUI[argumentCount];

			var members = metadataClass is null ? Array.Empty<MemberInfo>() : GetArgumentMetadata(metadataClass);

			// If the Init class has one member per init argument + constructor, then we can try extracting
			// attributes and labels from the non-contructor members defined in it and use them when visualizing
			// the Init arguments in the Inspector.
			if(members.Length == argumentCount + 1)
			{
				for(int i = 0, count = serializedProperties.Length; i < count; i++)
				{
					var serializedProperty = serializedProperties[i];
					if(serializedProperty is null)
					{
						#if DEV_MODE
						Debug.LogWarning($"{clientType.Name}.serializedProperties[{i}] was null.");
						#endif

						var newResults = new InitParameterGUI[results.Length - 1];
						if(i < count - 1)
						{
							Array.Copy(results, i + 1, newResults, i, newResults.Length - i);
						}
						results = newResults;

						continue;
					}

					var argumentType = argumentTypes[i];
					TryGetInitParameterAttributesFromMetadata(serializedProperty.name, argumentType, metadataClass, out Attribute[] attributes);
					var label = GetLabel(members[i]);
					results[i] = new InitParameterGUI
					(
						label,
						serializedProperty,
						argumentType,
						attributes
					);
				}
			}
			else
			{
				for(int i = 0, count = serializedProperties.Length; i < count; i++)
				{
					var serializedProperty = serializedProperties[i];
					if(serializedProperty is null)
					{
						#if DEV_MODE
						Debug.LogWarning($"{clientType.Name}.serializedProperties[{i}] was null.");
						#endif

						var newResults = new InitParameterGUI[results.Length - 1];
						if(i < count - 1)
						{
							Array.Copy(results, i + 1, newResults, i, newResults.Length - i);
						}
						results = newResults;

						continue;
					}

					var argumentType = argumentTypes[i];
					var label = GetArgumentLabel(clientType, argumentType, i);

					results[i] = new InitParameterGUI
					(
						label,
						serializedProperty,
						argumentType
					);
				}
			}

			return results;
		}

		internal static bool TryGetAttributeBasedPropertyDrawer([DisallowNull] SerializedProperty serializedProperty, [DisallowNull] Attribute[] attributes, out PropertyDrawer propertyDrawer)
		{
			foreach(var attribute in attributes)
			{
				if(attribute is PropertyAttribute propertyAttribute
					&& TryGetAttributeBasedPropertyDrawer(serializedProperty, propertyAttribute, out propertyDrawer))
				{
					return true;
				}
			}

			propertyDrawer = null;
			return false;
		}

		internal static bool TryGetAttributeBasedPropertyDrawer([DisallowNull] SerializedProperty serializedProperty, [AllowNull] PropertyAttribute propertyAttribute, out PropertyDrawer propertyDrawer)
		{
			if(!TryGetDrawerType(propertyAttribute, out Type drawerType))
			{
				propertyDrawer = null;
				return false;
			}

			propertyDrawer = CreateInstance(drawerType) as PropertyDrawer;

			if(propertyDrawer == null)
			{
				return false;
			}

			if(propertyAttribute != null)
			{
				var attributeField = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
				#if UNITY_2019_1_OR_NEWER && ENABLE_UI_SUPPORT
				//var element = propertyDrawer.CreatePropertyGUI(serializedProperty);
				#endif
				attributeField.SetValue(propertyDrawer, propertyAttribute);
			}

			if(serializedProperty.GetMemberInfo() is FieldInfo fieldInfo)
			{
				typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(propertyDrawer, fieldInfo);
			}

			return true;
		}

		private static bool TryGetDrawerType([DisallowNull] PropertyAttribute propertyAttribute, out Type drawerType)
		{
			var propertyAttributeType = propertyAttribute.GetType();
			var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
			var useForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
			drawerType = null;

			foreach(var propertyDrawerType in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
			{
				foreach(var attribute in propertyDrawerType.GetCustomAttributes<CustomPropertyDrawer>())
				{
					var targetType = typeField.GetValue(attribute) as Type;
					if(targetType == propertyAttributeType)
					{
						drawerType = propertyDrawerType;
						return true;
					}

					if(targetType.IsAssignableFrom(propertyAttributeType) && (bool)useForChildrenField.GetValue(attribute))
					{
						drawerType = propertyDrawerType;
					}
				}
			}

			return drawerType != null;
		}

		internal static object CreateInstance(Type type)
		{
			try
			{
				return Activator.CreateInstance(type);
			}
			catch
			{
				return FormatterServices.GetUninitializedObject(type);
			}
		}

		internal static GUIContent GetArgumentLabel(Type clientType, Type parameterType, int parameterIndex)
		{
			if(TryGetArgumentTargetMember(clientType, parameterType, parameterIndex, false, out var member))
			{
				var label = GetLabel(member);
				if(member.GetCustomAttribute<TooltipAttribute>() is TooltipAttribute tooltip)
				{
					label.tooltip = tooltip.tooltip;
				}

				return label;
			}
			
			return GetLabel(parameterType);
		}

		internal static GUIContent GetLabel([DisallowNull] MemberInfo member)
		{
			var label = GetLabel(member.Name);
			label.tooltip = GetTooltip(member);
			return label;
		}

		internal static string GetTooltip(MemberInfo member) => member.GetCustomAttribute<TooltipAttribute>() is TooltipAttribute tooltip ? tooltip.tooltip : "";

		/// <summary>
		/// Pr
		/// </summary>
		/// <param name="clientType"></param>
		/// <param name="parameterType">
		/// Type which returned member must be assignable from.
		/// </param>
		/// <param name="argumentIndex">
		/// Zero-based index of the Init function argument whose target member is retrieved.
		/// <para>
		/// If the nth public member of <paramref name="clientType"/> is of type <paramref name="parameterType"/>
		/// then that will be returned.
		/// </para>
		/// </param>
		/// <param name="member">
		/// 
		/// </param>
		/// <returns></returns>
		internal static bool TryGetArgumentTargetMember(Type clientType, Type parameterType, int argumentIndex, bool requirePublicSetter, out MemberInfo member)
			=> InjectionUtility.TryGetInitArgumentTargetMember(clientType, parameterType, argumentIndex, requirePublicSetter, out member);

		internal static bool TryGetArgumentTargetFieldName(Type clientType, Type parameterType, int argumentIndex, out string targetFieldName)
		{
			if(InjectionUtility.TryGetInitArgumentTargetMember(clientType, parameterType, argumentIndex, false, out var member))
			{
				targetFieldName = member.Name;
				return true;
			}

			targetFieldName = null;
			return false;
		}

		internal static bool IsInitializable(object client) => client is IOneArgument or ITwoArguments or IThreeArguments or IFourArguments or IFiveArguments or ISixArguments or ISevenArguments or IEightArguments or INineArguments or ITenArguments or IElevenArguments or ITwelveArguments;

		internal static bool IsInitializable(Type clientType) => GetClientInitArgumentCount(clientType) > 0;

		internal static int GetClientInitArgumentCount(Type clientType)
		{
			if(typeof(IOneArgument).IsAssignableFrom(clientType))
			{
				return 1;
			}

			if(typeof(ITwoArguments).IsAssignableFrom(clientType))
			{
				return 2;
			}

			if(typeof(IThreeArguments).IsAssignableFrom(clientType))
			{
				return 3;
			}

			if(typeof(IFourArguments).IsAssignableFrom(clientType))
			{
				return 4;
			}

			if(typeof(IFiveArguments).IsAssignableFrom(clientType))
			{
				return 5;
			}

			if(typeof(ISixArguments).IsAssignableFrom(clientType))
			{
				return 6;
			}

			if(typeof(ISevenArguments).IsAssignableFrom(clientType))
			{
				return 7;
			}

			if(typeof(IEightArguments).IsAssignableFrom(clientType))
			{
				return 8;
			}

			if(typeof(INineArguments).IsAssignableFrom(clientType))
			{
				return 9;
			}

			if(typeof(ITenArguments).IsAssignableFrom(clientType))
			{
				return 10;
			}

			if(typeof(IElevenArguments).IsAssignableFrom(clientType))
			{
				return 11;
			}

			if(typeof(ITwelveArguments).IsAssignableFrom(clientType))
			{
				return 12;
			}

			return 0;
		}

		internal static bool TryGetTintForNullGuardResult(NullGuardResult nullGuardResult, out Color color)
		{
			if(nullGuardResult == NullGuardResult.Passed)
			{
				color = Color.white;
				return false;
			}

			if(nullGuardResult == NullGuardResult.ValueProviderValueNullInEditMode)
			{
				color = NullGuardWarningColor;
				return true;
			}

			color = NullGuardFailedColor;
			return true;
		}
	}
}