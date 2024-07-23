using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	internal static class CustomEditorUtility
	{
		public static readonly Type genericInspectorType;

		#if ODIN_INSPECTOR
		public static readonly Type odinEditorType;
		#endif

		static CustomEditorUtility()
		{
			genericInspectorType = GetInternalEditorType("UnityEditor.GenericInspector");

			#if ODIN_INSPECTOR
			odinEditorType = Type.GetType("Sirenix.OdinInspector.Editor.OdinEditor, Sirenix.OdinInspector.Editor", false);
			#if DEV_MODE
			Debug.Assert(odinEditorType != null, "Sirenix.OdinInspector.Editor.OdinEditor type not found.");
			#endif
			#endif
		}

		public static Type GetGenericInspectorType()
		{
			#if ODIN_INSPECTOR
			if(odinEditorType != null)
			{
				return odinEditorType;
			}
			#endif

			return genericInspectorType;
		}

		public static bool IsGenericInspectorType([AllowNull] Type customEditorType)
		{
			#if ODIN_INSPECTOR
			if(customEditorType == odinEditorType)
			{
				return true;
			}
			#endif

			return customEditorType == genericInspectorType;
		}

		public static Type GetInternalEditorType(string fullTypeName)
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

		public static MethodInfo GetEditorInternalStaticMethod(string fullTypeName, string methodName)
		{
			return GetInternalEditorType(fullTypeName)?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static FieldInfo GetEditorStaticInternalField(string fullTypeName, string fieldName)
		{
			return GetInternalEditorType(fullTypeName)?.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
	}
}