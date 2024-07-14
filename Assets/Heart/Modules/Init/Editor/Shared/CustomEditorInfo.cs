using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Contains information about a single custom editor targeting a particular <see cref="UnityEngine.Object"/> class.
	/// <para>
	/// Mirrors the internal UnityEditor.CustomEditorAttributes.MonoEditorType type.
	/// </para>
	/// </summary>
	public readonly struct CustomEditorInfo
	{
		internal readonly static Type monoEditorTypeType;
		internal static readonly FieldInfo inspectorTypeField;
		internal static readonly FieldInfo supportedRenderPipelineTypesField;
		internal static readonly FieldInfo editorForChildClassesField;
		internal static readonly FieldInfo isFallbackField;

		internal readonly Type inspectorType;
		internal readonly Type[] supportedRenderPipelineTypes;
		internal readonly bool editorForChildClasses;
		internal readonly bool isFallback;

		/// <summary>
		/// Contains information about a single custom editor targeting a particular <see cref="UnityEngine.Object"/> class.
		/// <para>
		/// Mirrors the internal UnityEditor.CustomEditorAttributes.MonoEditorType type.
		/// </para>
		/// </summary>
		static CustomEditorInfo()
		{
			monoEditorTypeType = GetInternalEditorType("UnityEditor.CustomEditorAttributes").GetNestedType("MonoEditorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			inspectorTypeField = monoEditorTypeType.GetField("inspectorType");
			supportedRenderPipelineTypesField = monoEditorTypeType.GetField("supportedRenderPipelineTypes");
			editorForChildClassesField = monoEditorTypeType.GetField("editorForChildClasses");
			isFallbackField = monoEditorTypeType.GetField("isFallback");

			#if DEV_MODE
			Debug.Assert(inspectorTypeField != null, nameof(inspectorTypeField));
			Debug.Assert(supportedRenderPipelineTypesField != null, nameof(supportedRenderPipelineTypesField));
			Debug.Assert(editorForChildClassesField != null, nameof(editorForChildClassesField));
			Debug.Assert(isFallbackField != null, nameof(isFallbackField));
			#endif
		}

		public CustomEditorInfo(Type inspectorType, bool editorForChildClasses = false, bool isFallback = false)
		{
			this.inspectorType = inspectorType;
			supportedRenderPipelineTypes = Type.EmptyTypes;
			this.editorForChildClasses = editorForChildClasses;
			this.isFallback = isFallback;

			#if DEV_MODE
			Debug.Assert(typeof(Editor).IsAssignableFrom(inspectorType), inspectorType.Name);
			#endif
		}

		public CustomEditorInfo(Type inspectorType, Type[] supportedRenderPipelineTypes, bool editorForChildClasses, bool isFallback)
		{
			this.inspectorType = inspectorType;
			this.supportedRenderPipelineTypes = supportedRenderPipelineTypes;
			this.editorForChildClasses = editorForChildClasses;
			this.isFallback = isFallback;

			#if DEV_MODE
			Debug.Assert(typeof(Editor).IsAssignableFrom(inspectorType), inspectorType.Name);
			#endif
		}

		public CustomEditorInfo(object obj)
		{
			inspectorType = inspectorTypeField.GetValue(obj) as Type;
			supportedRenderPipelineTypes = (Type[])supportedRenderPipelineTypesField.GetValue(obj) ?? Type.EmptyTypes;
			editorForChildClasses = (bool)editorForChildClassesField.GetValue(obj);
			isFallback = (bool)isFallbackField.GetValue(obj);

			#if DEV_MODE
			//Debug.Assert(typeof(Object).IsAssignableFrom(inspectedType), inspectedType.Name); // Fails for some classes when Odin Inspector is installed
			//Debug.Assert(!typeof(Editor).IsAssignableFrom(inspectedType), inspectedType.Name); // Fails for internal AssetStoreAssetInspector
			Debug.Assert(typeof(Editor).IsAssignableFrom(inspectorType), inspectorType.Name);
			#endif
		}

		public static CustomEditorInfo[] Create(IList internalList) // List<MonoEditorType>
		{ 
			int count = internalList.Count;
			var result = new CustomEditorInfo[count];
			for(int i = 0; i < count; i++)
			{
				result[i] = new CustomEditorInfo(internalList[i]);
			}

			return result;
		}

		public static Type ExtractInspectorType(object internalMonoEditorType) => inspectorTypeField.GetValue(internalMonoEditorType) as Type;

		public object ToInternalType()
		{
			object instance;
			try
			{
				instance = Activator.CreateInstance(monoEditorTypeType);
			}
			#if DEV_MODE
			catch(Exception e)
			{
				Debug.LogError(e);
			#else
			catch
			{
			#endif
				instance = FormatterServices.GetUninitializedObject(monoEditorTypeType);
			}

			inspectorTypeField.SetValue(instance, inspectorType);
			supportedRenderPipelineTypesField.SetValue(instance, supportedRenderPipelineTypes);
			editorForChildClassesField.SetValue(instance, editorForChildClasses);
			isFallbackField.SetValue(instance, isFallback);

			return instance;
		}

		public IList ToInternalTypeList()
		{
			var listType = typeof(List<>).MakeGenericType(monoEditorTypeType);
			var list = Activator.CreateInstance(listType) as IList;
			list.Add(ToInternalType());
			return list;
		}

		private static Type GetInternalEditorType(string fullTypeName)
		{
			#if DEV_MODE
			Debug.Assert(fullTypeName.IndexOf(".") != -1, fullTypeName);
			#endif

			var type = typeof(Editor).Assembly.GetType(fullTypeName);

			#if DEV_MODE
			Debug.Assert(type != null, $"Type {fullTypeName} was not found in assembly {typeof(Editor).Assembly.GetName().Name}.");
			#endif

			return type;
		}
	}
}