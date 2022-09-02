//#define DEBUG_OVERRIDE_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Pancake.Init;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor.Init
{
	[InitializeOnLoad]
	internal static class InitializableEditorInjector
	{
		public static bool IsDone { get; private set; } = false;

		private static readonly Dictionary<Type, Type> interfaceGenericTypeDefinitionToEditorTypes = new Dictionary<Type, Type>()
		{
			{ typeof(IInitializable<>), typeof(InitializableT1Editor) },
			{ typeof(IInitializable<,>), typeof(InitializableT2Editor) },
			{ typeof(IInitializable<,,>), typeof(InitializableT3Editor) },
			{ typeof(IInitializable<,,,>), typeof(InitializableT4Editor) },
			{ typeof(IInitializable<,,,,>), typeof(InitializableT5Editor) },
			{ typeof(IInitializable<,,,,,>), typeof(InitializableT6Editor) }
		};

		private static readonly Dictionary<Type, Type> baseClassGenericTypeDefinitionToEditorTypes = new Dictionary<Type, Type>()
		{
			{ typeof(Initializer<,>), typeof(InitializerT1Editor) },
			{ typeof(Initializer<,,>), typeof(InitializerT2Editor) },
			{ typeof(Initializer<,,,>), typeof(InitializerT3Editor) },
			{ typeof(Initializer<,,,,>), typeof(InitializerT4Editor) },
			{ typeof(Initializer<,,,,,>), typeof(InitializerT5Editor) },
			{ typeof(Initializer<,,,,,,>), typeof(InitializerT6Editor) },
			{ typeof(WrapperInitializer<,,>), typeof(WrapperInitializerT1Editor) },
			{ typeof(WrapperInitializer<,,,>), typeof(WrapperInitializerT2Editor) },
			{ typeof(WrapperInitializer<,,,,>), typeof(WrapperInitializerT3Editor) },
			{ typeof(WrapperInitializer<,,,,,>), typeof(WrapperInitializerT4Editor) },
			{ typeof(WrapperInitializer<,,,,,,>), typeof(WrapperInitializerT5Editor) },
			{ typeof(WrapperInitializer<,,,,,,,>), typeof(WrapperInitializerT6Editor) }
		};

		private static readonly HashSet<Type> initializableEditors = new HashSet<Type>()
		{
			{ typeof(InitializableT1Editor) },
			{ typeof(InitializableT2Editor) },
			{ typeof(InitializableT3Editor) },
			{ typeof(InitializableT4Editor) },
			{ typeof(InitializableT5Editor) },
			{ typeof(InitializableT6Editor) }
		};

		private static Type genericInspectorType;

		#if ODIN_INSPECTOR
		private static Type odinEditorType;
		private static int timesToWaitForOdin = 50;
		private static bool ShouldWaitForOdinToInjectItsEditor()
		{
			if(timesToWaitForOdin <= 0)
			{
				#if DEV_MODE
				Debug.Log("Waited long enough for Odin. Injecting custom Initializer Editors...");
				#endif
				return false;
			}

			timesToWaitForOdin--;

			var internalEditorsField = GetEditorStaticInternalField("UnityEditor.CustomEditorAttributes", "kSCustomEditors");
			if(internalEditorsField is null || odinEditorType is null)
			{
				return false;
			}

			var internalDictionary = internalEditorsField.GetValue(null) as IDictionary;

			foreach(DictionaryEntry typeAndList in internalDictionary)
			{
				var internalList = typeAndList.Value as IList;

				foreach(var item in internalList)
				{
					if(MonoEditorType.m_InspectorTypeField.GetValue(item) as Type == odinEditorType)
					{
						return false;
					}
				}
			}

			return true;
		}
		#endif

		static InitializableEditorInjector()
		{
			IsDone = false;

			genericInspectorType = GetInternalEditorType("UnityEditor.GenericInspector");

			#if ODIN_INSPECTOR
			odinEditorType = Type.GetType("Sirenix.OdinInspector.Editor.OdinEditor, Sirenix.OdinInspector.Editor", false);
			#if DEV_MODE
			Debug.Assert(odinEditorType != null, "Sirenix.OdinInspector.Editor.OdinEditor type not found.");
			#endif
			#endif

			ReplaceInternalEditors();
		}

		private static void ReplaceInternalEditors()
		{
			#if ODIN_INSPECTOR
			if(ShouldWaitForOdinToInjectItsEditor())
			{
				EditorApplication.delayCall += ReplaceInternalEditors;
				return;
			}
			RebuildInspectorEditors();
			#endif

			EnsureInternalEditorsDictionaryIsBuilt();

			var kSCustomEditorsField = GetEditorStaticInternalField("UnityEditor.CustomEditorAttributes", "kSCustomEditors");
			if(kSCustomEditorsField is null)
			{
				#if DEV_MODE
				Debug.LogError("Field CustomEditorAttributes.kSCustomEditors not found.");
				#endif
			}
			else
			{
				ReplaceInternalEditors(kSCustomEditorsField);
			}

			var kSCustomMultiEditorsField = GetEditorStaticInternalField("UnityEditor.CustomEditorAttributes", "kSCustomMultiEditors");
			if(kSCustomMultiEditorsField is null)
			{
				#if DEV_MODE
				Debug.LogError("Field CustomEditorAttributes.kSCustomMultiEditors not found.");
				#endif
			}
			else
			{
				ReplaceInternalEditors(kSCustomMultiEditorsField);
			}

			IsDone = true;
		}

		private static void EnsureInternalEditorsDictionaryIsBuilt()
		{
			var findCustomEditorTypeMethod = GetEditorInternalStaticMethod("UnityEditor.CustomEditorAttributes", "FindCustomEditorTypeByType");
			if(findCustomEditorTypeMethod is null)
			{
				#if DEV_MODE
				Debug.LogError("Method CustomEditorAttributes.FindCustomEditorTypeByType not found.");
				#endif
				return;
			}

			findCustomEditorTypeMethod.Invoke(null, new object[] { null, false });
		}

		private static void ReplaceInternalEditors(FieldInfo internalEditorsField)
		{
			var internalDictionary = internalEditorsField.GetValue(null) as IDictionary;
			var thisAssembly = typeof(InitializableEditorInjector).Assembly;

			foreach(var type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
			{
				if(type.IsAbstract || !TryGetEditorOverrideType(type, out Type overrideEditorType))
				{
					continue;
				}

				Type customEditorType = GetCustomEditorType(type, false);
				if(customEditorType.Assembly == thisAssembly)
				{
					#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                    Debug.Log($"Won't override {type.Name} existing editor {GetCustomEditorType(type, false).Name}");
					#endif
					continue;
				}

				#if DRAW_INIT_SECTION_WITHOUT_EDITOR
				// Still use the custom editor when possible,
				// because it visualizes non-serialized fields in play mode.
				if(customEditorType != genericInspectorType)
				{
					continue;
				}

				#if ODIN_INSPECTOR
				if(customEditorType != odinEditorType)
				{
					continue;
				}
				#endif

				#endif

				ReplaceOrAddInternalEditorFor(internalDictionary, type, overrideEditorType);
			}
		}

		private static bool TryGetEditorOverrideType(Type inspectedType, out Type editorType)
		{
			for(var baseType = inspectedType.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if(baseType.IsGenericType && baseClassGenericTypeDefinitionToEditorTypes.TryGetValue(baseType.GetGenericTypeDefinition(), out editorType))
				{
					return true;
				}
			}

			foreach(var interfaceType in inspectedType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && interfaceGenericTypeDefinitionToEditorTypes.TryGetValue(interfaceType.GetGenericTypeDefinition(), out editorType))
				{
					return true;
				}
			}

			editorType = null;
			return false;
		}

		private static void ReplaceOrAddInternalEditorFor(IDictionary internalDictionary, Type inspectedType, Type overrideEditorType)
		{
			if(!internalDictionary.Contains(inspectedType))
			{
				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} default editor with {overrideEditorType.Name}.");
				#endif

				var add = new MonoEditorType(inspectedType, overrideEditorType, null, false, false);
				internalDictionary.Add(inspectedType, add.ToInternalTypeList());
			}
			else
			{
				var list = internalDictionary[inspectedType] as IList;

				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} editor {(MonoEditorType.m_InspectorTypeField.GetValue(list[0]) as Type).Name} with {overrideEditorType.Name}.");
				#endif

				list.Clear();
				var add = new MonoEditorType(inspectedType, overrideEditorType, null, true, false);
				list.Add(add.ToInternalType());
			}
		}

		public static void CreateCachedEditor(Object[] targets, ref UnityEditor.Editor editor)
		{
			var editorType = GetCustomEditorType(targets[0].GetType(), targets.Length > 1);

			if(initializableEditors.Contains(editorType))
			{
				#if DEV_MODE
				Debug.LogWarning(targets[0]);
				#endif

				#if ODIN_INSPECTOR
				editorType = odinEditorType != null ? odinEditorType : genericInspectorType;
				#else
                editorType = genericInspectorType;
				#endif
			}

			UnityEditor.Editor.CreateCachedEditor(targets[0], editorType, ref editor);
		}

		public static Type GetCustomEditorType([NotNull] Type componentType, bool multiEdit)
		{
			#if DEV_MODE
			Debug.Assert(componentType != null);
			#endif

			var inspectedType = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			var editorForChildClasses = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Type result = null;

			foreach(var customEditorType in TypeCache.GetTypesWithAttribute<CustomEditor>())
			{
				foreach(var attribute in customEditorType.GetCustomAttributes<CustomEditor>())
				{
					var targetType = inspectedType.GetValue(attribute) as Type;
					if(targetType == null)
					{
						continue;
					}

					if(targetType == componentType)
					{
						if(!attribute.isFallback)
						{
							return customEditorType;
						}

						result = customEditorType;
						break;
					}

					if(result == null && targetType.IsAssignableFrom(componentType) && (bool)editorForChildClasses.GetValue(attribute))
					{
						result = customEditorType;
						break;
					}
				}
			}

			if(result != null)
			{
				return result;
			}

			#if ODIN_INSPECTOR
			return odinEditorType ?? genericInspectorType;
			#else
			return genericInspectorType;
			#endif
		}

		private static MethodInfo GetEditorInternalStaticMethod(string fullTypeName, string methodName)
		{
			return GetInternalEditorType(fullTypeName)?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static FieldInfo GetEditorStaticInternalField(string fullTypeName, string fieldName)
		{
			return GetInternalEditorType(fullTypeName)?.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static Type GetInternalEditorType(string fullTypeName)
		{
			#if DEV_MODE
			Debug.Assert(fullTypeName.IndexOf(".") != -1, fullTypeName);
			#endif

			var type = typeof(UnityEditor.Editor).Assembly.GetType(fullTypeName);

			#if DEV_MODE
			Debug.Assert(type != null, $"Type {fullTypeName} was not found in assembly {typeof(Editor).Assembly.GetName().Name}.");
			#endif

			return type;
		}

		#if ODIN_INSPECTOR
		private static void RebuildInspectorEditors()
		{
			var selectionWas = Selection.objects;
			Selection.objects = Array.Empty<Object>();
			EditorApplication.delayCall += () => Selection.objects = selectionWas;
		}
		#endif

		private class MonoEditorType
		{
			internal static readonly FieldInfo m_InspectedTypeField;
			internal static readonly FieldInfo m_InspectorTypeField;
			internal static readonly FieldInfo m_RenderPipelineTypeField;
			internal static readonly FieldInfo m_EditorForChildClassesField;
			internal static readonly FieldInfo m_IsFallbackField;

			private static Type internalMonoEditorType => GetInternalEditorType("UnityEditor.CustomEditorAttributes").GetNestedType("MonoEditorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			internal readonly Type inspectedType;
			internal readonly Type inspectorType;
			internal readonly Type renderPipelineType;
			internal readonly bool editorForChildClasses;
			internal readonly bool isFallback;

			static MonoEditorType()
			{
				m_InspectedTypeField = internalMonoEditorType.GetField("m_InspectedType");
				m_InspectorTypeField = internalMonoEditorType.GetField("m_InspectorType");
				m_RenderPipelineTypeField = internalMonoEditorType.GetField("m_RenderPipelineType");
				m_EditorForChildClassesField = internalMonoEditorType.GetField("m_EditorForChildClasses");
				m_IsFallbackField = internalMonoEditorType.GetField("m_IsFallback");

				#if DEV_MODE
				Debug.Assert(m_InspectedTypeField != null, nameof(m_InspectedTypeField));
				Debug.Assert(m_InspectorTypeField != null, nameof(m_InspectorTypeField));
				Debug.Assert(m_RenderPipelineTypeField != null, nameof(m_RenderPipelineTypeField));
				Debug.Assert(m_EditorForChildClassesField != null, nameof(m_EditorForChildClassesField));
				Debug.Assert(m_IsFallbackField != null, nameof(m_IsFallbackField));
				#endif
			}

			public MonoEditorType(Type inspectedType, Type inspectorType, Type renderPipelineType, bool editorForChildClasses, bool isFallback)
			{
				this.inspectedType = inspectedType;
				this.inspectorType = inspectorType;
				this.renderPipelineType = renderPipelineType;
				this.editorForChildClasses = editorForChildClasses;
				this.isFallback = isFallback;

				#if DEV_MODE
				Debug.Assert(typeof(Object).IsAssignableFrom(inspectedType), inspectedType.Name);
				Debug.Assert(!typeof(Editor).IsAssignableFrom(inspectedType), inspectedType.Name);
				Debug.Assert(typeof(Editor).IsAssignableFrom(inspectorType), inspectorType.Name);
				#endif
			}

			public MonoEditorType(object obj)
			{
				inspectedType = m_InspectedTypeField.GetValue(obj) as Type;
				inspectorType = m_InspectorTypeField.GetValue(obj) as Type;
				renderPipelineType = m_RenderPipelineTypeField.GetValue(obj) as Type;
				editorForChildClasses = (bool)m_EditorForChildClassesField.GetValue(obj);
				isFallback = (bool)m_IsFallbackField.GetValue(obj);
			}

			public object ToInternalType()
			{
				object instance;
				try
				{
					instance = Activator.CreateInstance(internalMonoEditorType);
				}
				#if DEV_MODE
				catch(Exception e)
				{
					Debug.LogError(e);
				#else
                catch
                {
				#endif
					instance = FormatterServices.GetUninitializedObject(internalMonoEditorType);
				}

				m_InspectedTypeField.SetValue(instance, inspectedType);
				m_InspectorTypeField.SetValue(instance, inspectorType);
				m_RenderPipelineTypeField.SetValue(instance, renderPipelineType);
				m_EditorForChildClassesField.SetValue(instance, editorForChildClasses);
				m_IsFallbackField.SetValue(instance, isFallback);

				return instance;
			}

			public IList ToInternalTypeList()
			{
				var listType = typeof(List<>).MakeGenericType(internalMonoEditorType);
				var list = Activator.CreateInstance(listType) as IList;
				list.Add(ToInternalType());
				return list;
			}
		}
	}
}