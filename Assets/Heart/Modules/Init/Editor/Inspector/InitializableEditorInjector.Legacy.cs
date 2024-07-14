//#define DEBUG_OVERRIDE_EDITOR
//#define DRAW_INIT_SECTION_WITHOUT_EDITOR

#if !UNITY_2023_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class InitializableEditorInjector
	{
		public static bool IsDone { get; private set; } = false;

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

			foreach(var type in TypeCache.GetTypesDerivedFrom<Component>()
								.Concat(TypeCache.GetTypesDerivedFrom<ScriptableObject>())
								.Concat(TypeCache.GetTypesDerivedFrom<StateMachineBehaviour>()))
			{
				if(type.IsAbstract || !InitializerEditorUtility.TryGetEditorOverrideType(type, out Type overrideEditorType))
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
				if(!CustomEditorUtility.IsGenericInspectorType(customEditorType))
				{
					continue;
				}
				#endif

				ReplaceOrAddInternalEditorFor(internalDictionary, type, overrideEditorType);
			}
		}

		private static void ReplaceOrAddInternalEditorFor(IDictionary internalDictionary, Type inspectedType, Type overrideEditorType)
		{
			const bool EditorForChildClasses = false;
			const bool IsFallback = false;
			var add = new MonoEditorType(inspectedType, overrideEditorType, null, EditorForChildClasses, IsFallback);

			if(!internalDictionary.Contains(inspectedType))
			{
				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} default editor with {overrideEditorType.Name}.");
				#endif

				internalDictionary.Add(inspectedType, add.ToInternalTypeList());

			}
			else
			{
				var list = internalDictionary[inspectedType] as IList;

				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} editor {(MonoEditorType.m_InspectorTypeField.GetValue(list[0]) as Type).Name} with {overrideEditorType.Name}.");
				#endif

				list.Clear();
				list.Add(add.ToInternalType());
			}

			#if ODIN_INSPECTOR
			Sirenix.OdinInspector.Editor.CustomEditorUtility.SetCustomEditor(inspectedType, overrideEditorType, EditorForChildClasses, IsFallback);
			#endif
		}

		public static void CreateCachedEditor(Object[] targets, ref Editor editor)
		{
			var editorType = GetCustomEditorType(targets[0].GetType(), targets.Length > 1);

			if(InitializerEditorUtility.IsInitializableEditorType(editorType))
			{
				#if DEV_MODE
				Debug.LogWarning(targets[0]);
				#endif

				editorType = CustomEditorUtility.GetGenericInspectorType();
			}

			Editor.CreateCachedEditor(targets[0], editorType, ref editor);
		}

		public static bool HasCustomEditor(Type componentType) => GetCustomEditorType(componentType, false) != CustomEditorUtility.genericInspectorType;

		public static Type GetCustomEditorType([DisallowNull] Type componentType, bool multiEdit)
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

			return CustomEditorUtility.GetGenericInspectorType();
		}

		private static MethodInfo GetEditorInternalStaticMethod(string fullTypeName, string methodName)
			=> CustomEditorUtility.GetEditorInternalStaticMethod(fullTypeName, methodName);

		private static FieldInfo GetEditorStaticInternalField(string fullTypeName, string fieldName)
			=> CustomEditorUtility.GetEditorStaticInternalField(fullTypeName, fieldName);

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

			private static Type internalMonoEditorType => CustomEditorUtility.GetInternalEditorType("UnityEditor.CustomEditorAttributes").GetNestedType("MonoEditorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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
#endif