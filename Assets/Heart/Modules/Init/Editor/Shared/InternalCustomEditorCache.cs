//#define DEBUG_OVERRIDE_EDITOR

#if UNITY_2023_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Wrapper for the dictionary that Unity uses internally to match UnityEngine.Object types with the custom editors targeting them.
	/// <para>
	/// Dictionary is retrieved from the non-public field UnityEditor.CustomEditorAttributes.m_Cache.m_CustomEditorCache using reflection.
	/// </para>
	/// </summary>
	public sealed class InternalCustomEditorCache
	{
		private readonly IDictionary customEditorCache; // Dictionary<Type, CustomEditorAttributes.MonoEditorTypeStorage>
		private readonly Type monoEditorTypeStorageType;
		private readonly FieldInfo customEditorsField;
		private readonly FieldInfo customEditorsMultiEditionField;

		/// <summary>
		/// Wrapper for the dictionary that Unity uses internally to match UnityEngine.Object types with the custom editors targeting them.
		/// <para>
		/// Dictionary is retrieved from the non-public field UnityEditor.CustomEditorAttributes.m_Cache.m_CustomEditorCache using reflection.
		/// </para>
		/// </summary>
		public InternalCustomEditorCache()
		{
			if(!TryGetInternalEditorType("UnityEditor.CustomEditorAttributes", out Type customEditorAttributesType)
			|| !TryGetStaticField(customEditorAttributesType, "k_Instance", out var lazyInstanceField)
			|| !TryGetInstanceField(customEditorAttributesType, "m_Cache", out var cacheField))
			{
				return;
			}

			object lazyInstance = lazyInstanceField.GetValue(null); // Lazy<CustomEditorAttributes>
			object customEditorAttributes = lazyInstance.GetType().GetProperty(nameof(Lazy<object>.Value)).GetValue(lazyInstance); // CustomEditorAttributes
			object cache = cacheField.GetValue(customEditorAttributes); // CustomEditorAttributes.CustomEditorCache
			if(!TryGetInstanceField(cache.GetType(), "m_CustomEditorCache", out var customEditorCacheField))
			{
				return;
			}

			EnsureInternalEditorsDictionaryIsBuilt();

			customEditorCache = customEditorCacheField.GetValue(cache) as IDictionary;

			var enumerator = customEditorCache.GetEnumerator();
			if(!enumerator.MoveNext())
			{
				#if DEV_MODE
				Debug.LogError($"Custom editor cache was empty - can not to build editor type dictionaries at this time.");
				#endif
				return;
			}

			var typeAndStorage = (DictionaryEntry)enumerator.Current; // (Type, MonoEditorTypeStorage)
			var storage = typeAndStorage.Value; // MonoEditorTypeStorage
			monoEditorTypeStorageType = storage.GetType(); // MonoEditorTypeStorage

			TryGetInstanceField(monoEditorTypeStorageType, "customEditors", out customEditorsField); // List<MonoEditorType>
			TryGetInstanceField(monoEditorTypeStorageType, "customEditorsMultiEdition", out customEditorsMultiEditionField); // List<MonoEditorType>

			void EnsureInternalEditorsDictionaryIsBuilt()
			{
				if(TryGetStaticMethod(customEditorAttributes.GetType(), "FindCustomEditorTypeByType", out var findCustomEditorTypeMethod))
				{
					findCustomEditorTypeMethod.Invoke(null, new object[] { null, false });
				}
			}
		}

		public bool ContainsInspectedType(Type inspectedType) => customEditorCache.Contains(inspectedType);

		public bool ContainsEditorType(Type editorType)
		{
			foreach(DictionaryEntry inspectedToEditors in customEditorCache)
			{
				if(StorageContainsEditor(inspectedToEditors))
				{
					return true;
				}
			}

			return false;

			bool StorageContainsEditor(DictionaryEntry inspectedToEditors) // (Type, MonoEditorTypeStorage)
			{
				if(!(inspectedToEditors.Key is Type))
				{
					return false;
				}

				var storage = inspectedToEditors.Value; // MonoEditorTypeStorage
				foreach(var monoEditorType in customEditorsField.GetValue(storage) as IList)
				{
					if(CustomEditorInfo.ExtractInspectorType(monoEditorType) == editorType)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void InjectCustomEditor(Type inspectedType, Type editorType, bool canEditMultipleObjects, bool editorForChildClasses = false, bool isFallback = false)
		{
			IList customEditors;
			IList customEditorsMultiEdition;
			if(!customEditorCache.Contains(inspectedType))
			{
				var listType = customEditorsField.FieldType; // List<MonoEditorType>
				var monoEditorTypeStorage = Activator.CreateInstance(monoEditorTypeStorageType);

				// Create List<MonoEditorType> with capacity of 1 item.
				customEditors = Activator.CreateInstance(listType, 1) as IList;
				customEditorsField.SetValue(monoEditorTypeStorage, customEditors); // List<MonoEditorType>

				// If multi-editing is enabled, use same list, otherwise use an empty list
				if(canEditMultipleObjects)
				{
					customEditorsMultiEdition = Activator.CreateInstance(listType, customEditors.Count) as IList;
				}
				else
				{
					customEditorsMultiEdition = Activator.CreateInstance(listType, 0) as IList;
				}
				customEditorsMultiEditionField.SetValue(monoEditorTypeStorage, customEditorsMultiEdition); // List<MonoEditorType>

				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} default editor with {editorType.inspectorType.Name}.");
				#endif

				customEditorCache.Add(inspectedType, monoEditorTypeStorage);
			}
			else
			{
				var monoEditorTypeStorage = customEditorCache[inspectedType]; // MonoEditorTypeStorage

				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                Debug.Log($"Replacing {inspectedType.Name} custom editor with {editorType.editorType.Name}.");
				#endif

				customEditors = customEditorsField.GetValue(monoEditorTypeStorage) as IList;
				customEditorsMultiEdition = customEditorsMultiEditionField.GetValue(monoEditorTypeStorage) as IList;

				if(editorForChildClasses && !isFallback)
				{
					customEditors.Clear();

					if(canEditMultipleObjects)
					{
						customEditorsMultiEdition.Clear();
					}
				}
				else
				{
					for(int i = customEditors.Count - 1; i >= 0; i--)
					{
						var info = new CustomEditorInfo(customEditors[i]);
						if(!info.editorForChildClasses)
						{
							customEditors.RemoveAt(i);
							continue;
						}

						if(!isFallback && !info.isFallback)
						{
							customEditors[i] = new CustomEditorInfo(info.inspectorType, true, true).ToInternalType();
						}
					}

					if(canEditMultipleObjects)
					{
						for(int i = customEditorsMultiEdition.Count - 1; i >= 0; i--)
						{
							var info = new CustomEditorInfo(customEditorsMultiEdition[i]);
							if(!info.editorForChildClasses)
							{
								customEditorsMultiEdition.RemoveAt(i);
								continue;
							}

							if(!isFallback && !info.isFallback)
							{
								customEditorsMultiEdition[i] = new CustomEditorInfo(info.inspectorType, true, true).ToInternalType();
							}
						}
					}
				}
			}

			// (Type inspectorType, Type[] supportedRenderPipelineTypes, bool editorForChildClasses, bool isFallback)
			var monoEditorTypeConstructorArguments = new object[] { editorType, null, editorForChildClasses, isFallback};
			var monoEditorTypeInstance = Activator.CreateInstance(CustomEditorInfo.monoEditorTypeType, monoEditorTypeConstructorArguments);
			
			customEditors.Add(monoEditorTypeInstance);

			if(canEditMultipleObjects)
			{
				customEditorsMultiEdition.Add(monoEditorTypeInstance);
			}

			#if ODIN_INSPECTOR
			Sirenix.OdinInspector.Editor.CustomEditorUtility.SetCustomEditor(inspectedType, editorType, editorForChildClasses, isFallback);
			#endif
		}

		public void CopyTo(Dictionary<Type, CustomEditorInfoGroup> dictionary)
		{
			int count = customEditorCache.Count;
			if(count == 0)
			{
				#if DEV_MODE
				Debug.LogError($"Custom editor cache was empty - can not to build editor type dictionaries at this time.");
				#endif
				return;
			}

			dictionary.EnsureCapacity(count);

			var enumerator = customEditorCache.GetEnumerator();
			if(!enumerator.MoveNext())
			{
				#if DEV_MODE
				Debug.LogError($"Custom editor cache was empty - can not to build editor type dictionaries at this time.");
				#endif
				return;
			}

			var typeAndStorage = (DictionaryEntry)enumerator.Current; // (Type, MonoEditorTypeStorage)
			var storage = typeAndStorage.Value; // MonoEditorTypeStorage
			var storageType = storage.GetType(); // MonoEditorTypeStorage 
			if(!TryGetInstanceField(storageType, "customEditors", out var customEditorsField) // List<MonoEditorType>
				|| !TryGetInstanceField(storageType, "customEditors", out var customMultiEditorsField)) // List<MonoEditorType>
			{
				return;
			}

			AddEditorsFromStorage(typeAndStorage);

			while(enumerator.MoveNext())
			{
				AddEditorsFromStorage((DictionaryEntry)enumerator.Current);
			}

			void AddEditorsFromStorage(DictionaryEntry typeAndStorage) // MonoEditorTypeStorage
			{
				if(!(typeAndStorage.Key is Type type))
				{
					return;
				}

				var storage = typeAndStorage.Value; // MonoEditorTypeStorage
				var editors = CustomEditorInfo.Create(customEditorsField.GetValue(storage) as IList); // List<MonoEditorType>
				var multiEditors = CustomEditorInfo.Create(customMultiEditorsField.GetValue(storage) as IList); // List<MonoEditorType>
				dictionary[type] = new CustomEditorInfoGroup(editors, multiEditors);
			}
		}

		internal static bool TryGetInternalEditorType(string fullTypeName, out Type result)
		{
			#if DEV_MODE
			Debug.Assert(fullTypeName.IndexOf(".") != -1, fullTypeName);
			#endif

			result = typeof(Editor).Assembly.GetType(fullTypeName);

			if(result is null)
			{
				Debug.LogError($"Type {fullTypeName} not found in the assembly {typeof(Editor).Assembly.GetName().Name}. Contact the developer in the forums to add support for your Unity version.");
				return false;
			}

			return true;
		}

		private static bool TryGetStaticField(Type type, string fieldName, out FieldInfo result)
		{
			result = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if(result is null)
			{
				#if UNITY_2023_1_0
				Debug.LogError($"Field {type.FullName}.{fieldName} not found. Please update to 2023.1.0a14 or newer.");
				#else
				Debug.LogError($"Field {type.FullName}.{fieldName} not found. Contact the developer in the forums to add support for your Unity version.");
				#endif

				return false;
			}

			return true;
		}

		private static bool TryGetStaticMethod(Type type, string methodName, out MethodInfo result)
		{
			result = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if(result is null)
			{
				#if UNITY_2023_1_0
				Debug.LogWarning($"Method {type.FullName}.{methodName} not found. Please update to 2023.1.0a14 or newer.");
				#else
				Debug.LogWarning($"Method {type.FullName}.{methodName} not found. Contact the developer in the forums to add support for your Unity version.");
				#endif

				return false;
			}

			return true;
		}

		private static bool TryGetInstanceField(Type type, string fieldName, out FieldInfo result)
		{
			result = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(result is null)
			{
				#if UNITY_2023_1_0
				Debug.LogError($"Field {type.FullName}.{fieldName} not found. Please update to 2023.1.0a14 or newer.");
				#else
				Debug.LogError($"Field {type.FullName}.{fieldName} not found. Contact the developer in the forums to add support for your Unity version.");
				#endif

				return false;
			}

			return true;
		}
	}
}
#endif