using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	public static class CustomEditorType
	{
		private static readonly Dictionary<Type, (Type customEditorType, CustomEditor attribute, bool editorForChildClasses)> customEditorsByTargetType;
		private static readonly Func<CustomEditor, Type> inspectedTypeGetter;
		private static readonly Func<CustomEditor, bool> editorForChildClassesGetter;
		
		static CustomEditorType()
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var parameterTypes = new[] { typeof(CustomEditor) };
			var inspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", flags);
			inspectedTypeGetter = inspectedTypeField.GetGetter<Func<CustomEditor, Type>>(parameterTypes);
			
			var editorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", flags);
			editorForChildClassesGetter = editorForChildClassesField.GetGetter<Func<CustomEditor, bool>>(parameterTypes);
			
			if(inspectedTypeGetter is null)
			{
				Debug.LogWarning("Field CustomEditor.m_InspectedType not found.");
				inspectedTypeGetter = x => null;
			}
			
			if(editorForChildClassesGetter is null)
			{
				Debug.LogWarning("Field CustomEditor.m_EditorForChildClasses not found.");
				editorForChildClassesGetter = x => false;
			}
			
			var customEditorTypes = TypeCache.GetTypesWithAttribute<CustomEditor>();
			customEditorsByTargetType = new(customEditorTypes.Count);
			foreach(var customEditorType in customEditorTypes)
			{
				if(customEditorType.GetCustomAttribute<CustomEditor>() is { } customEditorAttribute
				   && inspectedTypeGetter(customEditorAttribute) is Type targetType)
				{
					if(!customEditorsByTargetType.ContainsKey(targetType))
					{
						customEditorsByTargetType.Add(targetType, (customEditorType, customEditorAttribute, editorForChildClassesGetter(customEditorAttribute)));
					}
					else if(!customEditorAttribute.isFallback)
					{
						customEditorsByTargetType[targetType] = (customEditorType, customEditorAttribute, editorForChildClassesGetter(customEditorAttribute));
					}
				}
			}
		}

		public static bool Exists(Type componentType) => Get(componentType, false) != CustomEditorUtility.genericInspectorType;
		
		public static Type Get([DisallowNull] Type componentType, bool multiEdit)
		{
#if DEV_MODE
			Debug.Assert(componentType != null);
#endif

			for(var type = componentType; type != null; type = type.BaseType)
			{
				if(customEditorsByTargetType.TryGetValue(type, out var info) && (type == componentType || info.editorForChildClasses))
				{
					return info.customEditorType;
				}
			}

			return CustomEditorUtility.GetGenericInspectorType();
		}
	}
}