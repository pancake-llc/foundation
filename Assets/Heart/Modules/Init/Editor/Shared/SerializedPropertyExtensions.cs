using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Shared.EditorOnly
{
	/// <summary>
	/// Extension methods for <see cref="SerializedProperty"/>.
	/// </summary>
	public static class SerializedPropertyExtensions
	{
		private readonly ref struct NameOrIndex
		{
			public readonly string name;
			public readonly int collectionIndex;

			public NameOrIndex(string name)
			{
				this.name = name;
				collectionIndex = -1;
			}

			public NameOrIndex(int collectionIndex)
			{
				name = null;
				this.collectionIndex = collectionIndex;
			}
		}

		private const BindingFlags AnyInstanceDeclaredOnly = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		private static readonly Regex collectionIndexRegex = new(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

		public static object GetValue([DisallowNull] this SerializedProperty serializedProperty)
		{
			#if DEV_MODE
			using var x = getValueMarker.Auto();
			#endif
			
			#if UNITY_6000_0_OR_NEWER
			// boxedValue does not support arrays or lists. source: https://docs.unity3d.com/ScriptReference/SerializedProperty-boxedValue.html
			if(!serializedProperty.isArray || serializedProperty.propertyType is not SerializedPropertyType.Generic) // SerializedPropertyType.Generic is needed to filter out string fields (isArray is true for them).
			{
				return serializedProperty.boxedValue;
			}
			#endif

			switch(serializedProperty.propertyType)
			{
				case SerializedPropertyType.Integer: return serializedProperty.intValue;
				case SerializedPropertyType.Boolean: return serializedProperty.boolValue;
				case SerializedPropertyType.Float: return serializedProperty.floatValue;
				case SerializedPropertyType.String: return serializedProperty.stringValue;
				case SerializedPropertyType.Color: return serializedProperty.colorValue;
				case SerializedPropertyType.ObjectReference: return serializedProperty.objectReferenceValue;
				case SerializedPropertyType.LayerMask: return serializedProperty.intValue;
				case SerializedPropertyType.Vector2: return serializedProperty.vector2Value;
				case SerializedPropertyType.Vector3: return serializedProperty.vector3Value;
				case SerializedPropertyType.Vector4: return serializedProperty.vector4Value;
				case SerializedPropertyType.Rect: return serializedProperty.rectValue;
				case SerializedPropertyType.ArraySize: return serializedProperty.arraySize;
				case SerializedPropertyType.Character: return serializedProperty.intValue;
				case SerializedPropertyType.AnimationCurve: return serializedProperty.animationCurveValue;
				case SerializedPropertyType.Bounds: return serializedProperty.boundsValue;
				case SerializedPropertyType.Quaternion: return serializedProperty.quaternionValue;
				case SerializedPropertyType.ExposedReference: return serializedProperty.exposedReferenceValue;
				case SerializedPropertyType.FixedBufferSize: return serializedProperty.fixedBufferSize;
				case SerializedPropertyType.Vector2Int: return serializedProperty.vector2IntValue;
				case SerializedPropertyType.Vector3Int: return serializedProperty.vector3IntValue;
				case SerializedPropertyType.RectInt: return serializedProperty.rectIntValue;
				case SerializedPropertyType.BoundsInt: return serializedProperty.boundsIntValue;
				case SerializedPropertyType.ManagedReference: return serializedProperty.managedReferenceValue;
				case SerializedPropertyType.Gradient: return serializedProperty.gradientValue;
				case SerializedPropertyType.Hash128: return serializedProperty.hash128Value;
				case SerializedPropertyType.Enum:
				#if UNITY_2022_1_OR_NEWER
					return serializedProperty.numericType switch
					{
						SerializedPropertyNumericType.Int8 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.intValue),
						SerializedPropertyNumericType.UInt8 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int16 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.uintValue),
						SerializedPropertyNumericType.UInt16 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int32 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.intValue),
						SerializedPropertyNumericType.UInt32 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int64 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.longValue),
						SerializedPropertyNumericType.UInt64 => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.ulongValue),
						_ => Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.longValue)
					};
				#else
					return Enum.ToObject(serializedProperty.GetMemberType(), serializedProperty.longValue);
				#endif
			}

			var propertyPath = serializedProperty.propertyPath;
			object value = serializedProperty.serializedObject.targetObject;

			int startIndex = 0;
			NameOrIndex p = default;
			SerializedProperty sp = null;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref p))
			{
				sp = p.collectionIndex != -1
				   ? sp.GetArrayElementAtIndex(p.collectionIndex)
				   : sp is null ? serializedProperty.serializedObject.FindProperty(p.name) : sp.FindPropertyRelative(p.name);

				value = sp.propertyType switch
				{
					SerializedPropertyType.Integer => sp.intValue,
					SerializedPropertyType.Boolean => sp.boolValue,
					SerializedPropertyType.Float => sp.floatValue,
					SerializedPropertyType.String => sp.stringValue,
					SerializedPropertyType.Color => sp.colorValue,
					SerializedPropertyType.ObjectReference => sp.objectReferenceValue,
					SerializedPropertyType.LayerMask => sp.intValue,
					SerializedPropertyType.Vector2 => sp.vector2Value,
					SerializedPropertyType.Vector3 => sp.vector3Value,
					SerializedPropertyType.Vector4 => sp.vector4Value,
					SerializedPropertyType.Rect => sp.rectValue,
					SerializedPropertyType.ArraySize => sp.arraySize,
					SerializedPropertyType.Character => sp.intValue,
					SerializedPropertyType.AnimationCurve => sp.animationCurveValue,
					SerializedPropertyType.Bounds => sp.boundsValue,
					SerializedPropertyType.Quaternion => sp.quaternionValue,
					SerializedPropertyType.ExposedReference => sp.exposedReferenceValue,
					SerializedPropertyType.FixedBufferSize => sp.fixedBufferSize,
					SerializedPropertyType.Vector2Int => sp.vector2IntValue,
					SerializedPropertyType.Vector3Int => sp.vector3IntValue,
					SerializedPropertyType.RectInt => sp.rectIntValue,
					SerializedPropertyType.BoundsInt => sp.boundsIntValue,
					SerializedPropertyType.ManagedReference => sp.managedReferenceValue,
					SerializedPropertyType.Gradient => sp.gradientValue,
					SerializedPropertyType.Hash128 => sp.hash128Value,
					SerializedPropertyType.Enum =>
					#if UNITY_2022_1_OR_NEWER
					sp.numericType switch
					{
						SerializedPropertyNumericType.Int8 => Enum.ToObject(GetMemberType(value, p.name), sp.intValue),
						SerializedPropertyNumericType.UInt8 => Enum.ToObject(GetMemberType(value, p.name), sp.uintValue),
						SerializedPropertyNumericType.Int16 => Enum.ToObject(GetMemberType(value, p.name), sp.uintValue),
						SerializedPropertyNumericType.UInt16 => Enum.ToObject(GetMemberType(value, p.name), sp.uintValue),
						SerializedPropertyNumericType.Int32 => Enum.ToObject(GetMemberType(value, p.name), sp.intValue),
						SerializedPropertyNumericType.UInt32 => Enum.ToObject(GetMemberType(value, p.name), sp.uintValue),
						SerializedPropertyNumericType.Int64 => Enum.ToObject(GetMemberType(value, p.name), sp.longValue),
						SerializedPropertyNumericType.UInt64 => Enum.ToObject(GetMemberType(value, p.name), sp.ulongValue),
						_ => Enum.ToObject(GetMemberType(value, p.name), sp.longValue)
					},
					#else
						Enum.ToObject(GetMemberType(value, p.name), sp.longValue),
					#endif
					_ => GetValue(value, p.collectionIndex, p.name)
				};
			}

			return value;
		}

		public static bool TryGetValue<TValue>([DisallowNull] this SerializedProperty serializedProperty, out TValue value)
		{
			#if DEV_MODE
			using var x = tryGetValueMarker.Auto();
			#endif

			switch(serializedProperty.propertyType)
			{
				case SerializedPropertyType.Integer: value = (TValue)(object)serializedProperty.intValue; return true;
				case SerializedPropertyType.Boolean: value = (TValue)(object)serializedProperty.boolValue; return true;
				case SerializedPropertyType.Float: value = (TValue)(object) serializedProperty.floatValue; return true;
				case SerializedPropertyType.String: value = (TValue)(object) serializedProperty.stringValue; return true;
				case SerializedPropertyType.Color: value = (TValue)(object) serializedProperty.colorValue; return true;
				case SerializedPropertyType.ObjectReference: value = (TValue)(object) serializedProperty.objectReferenceValue; return true;
				case SerializedPropertyType.LayerMask: value = (TValue)(object) serializedProperty.intValue; return true;
				case SerializedPropertyType.Vector2: value = (TValue)(object) serializedProperty.vector2Value; return true;
				case SerializedPropertyType.Vector3: value = (TValue)(object) serializedProperty.vector3Value; return true;
				case SerializedPropertyType.Vector4: value = (TValue)(object) serializedProperty.vector4Value; return true;
				case SerializedPropertyType.Rect: value = (TValue)(object) serializedProperty.rectValue; return true;
				case SerializedPropertyType.ArraySize: value = (TValue)(object) serializedProperty.arraySize; return true;
				case SerializedPropertyType.Character: value = (TValue)(object) serializedProperty.intValue; return true;
				case SerializedPropertyType.AnimationCurve: value = (TValue)(object) serializedProperty.animationCurveValue; return true;
				case SerializedPropertyType.Bounds: value = (TValue)(object) serializedProperty.boundsValue; return true;
				case SerializedPropertyType.Quaternion: value = (TValue)(object) serializedProperty.quaternionValue; return true;
				case SerializedPropertyType.ExposedReference: value = (TValue)(object) serializedProperty.exposedReferenceValue; return true;
				case SerializedPropertyType.FixedBufferSize: value = (TValue)(object) serializedProperty.fixedBufferSize; return true;
				case SerializedPropertyType.Vector2Int: value = (TValue)(object) serializedProperty.vector2IntValue; return true;
				case SerializedPropertyType.Vector3Int: value = (TValue)(object) serializedProperty.vector3IntValue; return true;
				case SerializedPropertyType.RectInt: value = (TValue)(object) serializedProperty.rectIntValue; return true;
				case SerializedPropertyType.BoundsInt: value = (TValue)(object) serializedProperty.boundsIntValue; return true;
				case SerializedPropertyType.ManagedReference: value = (TValue) serializedProperty.managedReferenceValue; return true;
				case SerializedPropertyType.Gradient: value = (TValue)(object) serializedProperty.gradientValue; return true;
				case SerializedPropertyType.Hash128: value = (TValue)(object) serializedProperty.hash128Value; return true;
				case SerializedPropertyType.Enum:
					#if UNITY_2022_1_OR_NEWER
					value = serializedProperty.numericType switch
					{
						SerializedPropertyNumericType.Int8 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.intValue),
						SerializedPropertyNumericType.UInt8 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int16 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.uintValue),
						SerializedPropertyNumericType.UInt16 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int32 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.intValue),
						SerializedPropertyNumericType.UInt32 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.uintValue),
						SerializedPropertyNumericType.Int64 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.longValue),
						SerializedPropertyNumericType.UInt64 => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.ulongValue),
						_ => value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.longValue)
					};
					#else
					value = (TValue)Enum.ToObject(typeof(TValue), serializedProperty.longValue);
					#endif
					return true;
			}

			var propertyPath = serializedProperty.propertyPath;
			object owner = serializedProperty.serializedObject.targetObject;

			int startIndex = 0;
			NameOrIndex p = default;
			SerializedProperty sp = null;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref p))
			{
				sp = p.collectionIndex != -1
				   ? sp.GetArrayElementAtIndex(p.collectionIndex)
				   : sp is null ? serializedProperty.serializedObject.FindProperty(p.name) : sp.FindPropertyRelative(p.name);

				switch (sp.propertyType)
				{
					case SerializedPropertyType.Integer:
						owner = sp.intValue;
						break;
					case SerializedPropertyType.Boolean:
						owner = sp.boolValue;
						break;
					case SerializedPropertyType.Float:
						owner = sp.floatValue;
						break;
					case SerializedPropertyType.String:
						owner = sp.stringValue;
						break;
					case SerializedPropertyType.Color:
						owner = sp.colorValue;
						break;
					case SerializedPropertyType.ObjectReference:
						owner = sp.objectReferenceValue;
						break;
					case SerializedPropertyType.LayerMask:
						owner = sp.intValue;
						break;
					case SerializedPropertyType.Vector2:
						owner = sp.vector2Value;
						break;
					case SerializedPropertyType.Vector3:
						owner = sp.vector3Value;
						break;
					case SerializedPropertyType.Vector4:
						owner = sp.vector4Value;
						break;
					case SerializedPropertyType.Rect:
						owner = sp.rectValue;
						break;
					case SerializedPropertyType.ArraySize:
						owner = sp.arraySize;
						break;
					case SerializedPropertyType.Character:
						owner = sp.intValue;
						break;
					case SerializedPropertyType.AnimationCurve:
						owner = sp.animationCurveValue;
						break;
					case SerializedPropertyType.Bounds:
						owner = sp.boundsValue;
						break;
					case SerializedPropertyType.Quaternion:
						owner = sp.quaternionValue;
						break;
					case SerializedPropertyType.ExposedReference:
						owner = sp.exposedReferenceValue;
						break;
					case SerializedPropertyType.FixedBufferSize:
						owner = sp.fixedBufferSize;
						break;
					case SerializedPropertyType.Vector2Int:
						owner = sp.vector2IntValue;
						break;
					case SerializedPropertyType.Vector3Int:
						owner = sp.vector3IntValue;
						break;
					case SerializedPropertyType.RectInt:
						owner = sp.rectIntValue;
						break;
					case SerializedPropertyType.BoundsInt:
						owner = sp.boundsIntValue;
						break;
					case SerializedPropertyType.ManagedReference:
						owner = sp.managedReferenceValue;
						break;
					case SerializedPropertyType.Gradient:
						owner = sp.gradientValue;
						break;
					case SerializedPropertyType.Hash128:
						owner = sp.hash128Value;
						break;
					case SerializedPropertyType.Enum:
						#if UNITY_2022_1_OR_NEWER
						owner = sp.numericType switch
						{
							SerializedPropertyNumericType.Int8 => Enum.ToObject(GetMemberType(owner, p.name), sp.intValue),
							SerializedPropertyNumericType.UInt8 => Enum.ToObject(GetMemberType(owner, p.name), sp.uintValue),
							SerializedPropertyNumericType.Int16 => Enum.ToObject(GetMemberType(owner, p.name), sp.uintValue),
							SerializedPropertyNumericType.UInt16 => Enum.ToObject(GetMemberType(owner, p.name), sp.uintValue),
							SerializedPropertyNumericType.Int32 => Enum.ToObject(GetMemberType(owner, p.name), sp.intValue),
							SerializedPropertyNumericType.UInt32 => Enum.ToObject(GetMemberType(owner, p.name), sp.uintValue),
							SerializedPropertyNumericType.Int64 => Enum.ToObject(GetMemberType(owner, p.name), sp.longValue),
							SerializedPropertyNumericType.UInt64 => Enum.ToObject(GetMemberType(owner, p.name), sp.ulongValue),
							_ => Enum.ToObject(GetMemberType(owner, p.name), sp.longValue)
						};
						#else
						owner = Enum.ToObject(GetMemberType(owner, p.name), sp.longValue);
						#endif
						break;
					default:
						if(p.collectionIndex is -1 ? !TryGetValueUsingReflection(owner, p.name, out owner) : !TryGetElement(owner, p.collectionIndex, out owner))
						{
							value = default;
							return false;
						}
						break;
				}
			}

			value = owner is TValue result ? result : default;
			return true;
		}

		[return: MaybeNull]
		public static Type GetMemberType([DisallowNull] this SerializedProperty serializedProperty)
		{
			string propertyPath = serializedProperty.propertyPath;
			Type previousType = serializedProperty.serializedObject.targetObject.GetType();
			Type type = previousType;

			int startIndex = 0;
			NameOrIndex property = default;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref property))
			{
				previousType = type;
				type = GetMemberType(previousType, property);
			}

			return type;
		}

		public static MemberInfo GetMemberInfo([DisallowNull] this SerializedProperty serializedProperty)
		{
			string propertyPath = serializedProperty.propertyPath;
			Type previousType = serializedProperty.serializedObject.targetObject.GetType();
			Type type = previousType;

			int startIndex = 0;
			NameOrIndex property = default;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref property))
			{
				previousType = type;
				type = GetMemberType(previousType, property);
			}

			return GetMember(previousType, property.name);
		}

		public static void SetValue(this SerializedProperty serializedProperty, object value)
		{
			#if UNITY_6000_0_OR_NEWER
			serializedProperty.boxedValue = value;
			#else
			switch(serializedProperty.propertyType)
			{
				#if UNITY_2021_3_OR_NEWER
				case SerializedPropertyType.ManagedReference:
					serializedProperty.managedReferenceValue = value;
					return;
				#endif
				case SerializedPropertyType.String:
					serializedProperty.stringValue = (string)value;
					return;
				case SerializedPropertyType.Boolean:
					serializedProperty.boolValue = (bool)value;
					return;
				case SerializedPropertyType.Float:
					serializedProperty.floatValue = (float)value;
					return;
			}

			string propertyPath = serializedProperty.propertyPath;
			var serializedObject = serializedProperty.serializedObject;
			object obj = serializedProperty.serializedObject.targetObject;

			int startIndex = 0;
			NameOrIndex property = default;
			NameOrIndex nextPropertyOrDefault = default;
			TryGetNextProperty(propertyPath, ref startIndex, ref property);
			while(TryGetNextProperty(propertyPath, ref startIndex, ref nextPropertyOrDefault))
			{
				obj = GetValue(obj, property.collectionIndex, property.name);
				property = nextPropertyOrDefault;
			}

			if(obj.GetType().IsValueType)
			{
				Debug.LogError($"Cannot use SerializedObject.SetValue on a struct object. Either change {obj.GetType().Name} to a class, or use SetValue with a parent SerializedProperty.");
				return;
			}

			Undo.RecordObject(serializedObject.targetObject, $"Set {nextPropertyOrDefault.name}");

			SetValue(obj, property, value);

			EditorUtility.SetDirty(serializedObject.targetObject);
			serializedObject.ApplyModifiedProperties();
			#endif
		}

		private static void SetValue(object obj, NameOrIndex property, object value)
		{
			if(property.name is null)
			{
				((IList)obj)[property.collectionIndex] = value;
				return;
			}

			SetClassMemberValue(obj, property.name, value);
		}

		private static void SetClassMemberValue(object obj, string memberName, object value)
		{
			for(var type = obj.GetType(); type != null; type = type.BaseType)
			{
				var field = type.GetField(memberName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					field.SetValue(obj, value);
					return;
				}

				var property = type.GetProperty(memberName, AnyInstanceDeclaredOnly);
				if(property != null && property.CanRead)
				{
					property.SetValue(obj, value);
					return;
				}
			}

			throw new MissingMemberException(obj.GetType().Name, memberName);
		}

		private static bool TryGetNextProperty(string propertyPath, ref int startIndex, ref NameOrIndex result)
		{
			if(startIndex >= propertyPath.Length)
			{
				return false;
			}

			var collectionIndexMatch = collectionIndexRegex.Match(propertyPath, startIndex);
			if(collectionIndexMatch.Success)
			{
				startIndex += collectionIndexMatch.Length + 1;
				int collectionIndex = int.Parse(collectionIndexMatch.Groups[1].Value);
				result = new NameOrIndex(collectionIndex);
				return true;
			}

			int endIndex = propertyPath.IndexOf('.', startIndex);
			if(endIndex == -1)
			{
				result = new NameOrIndex(propertyPath.Substring(startIndex));
				startIndex = propertyPath.Length;
				return true;
			}

			result = new NameOrIndex(propertyPath.Substring(startIndex, endIndex - startIndex));
			startIndex = endIndex + 1;
			return true;
		}

		public static int GetArrayElementIndex([DisallowNull] this SerializedProperty serializedProperty)
		{
			string propertyPath = serializedProperty.propertyPath;
			int i = propertyPath.LastIndexOf('[');
			if(i == -1)
			{
				return -1;
			}

			int start = i + 1;
			int end = propertyPath.IndexOf(']', start);
			string indexString = propertyPath.Substring(start, end - start);
			return int.TryParse(indexString, out int index) ? index : -1;
		}

		private static object GetValue(object containingObject, int collectionIndex, string propertyName)
		{
			if(containingObject is null)
			{
				return null;
			}

			if(collectionIndex != -1)
			{
				var list = (IList)containingObject;
				return list.Count <= collectionIndex ? null : list[collectionIndex];
			}

			for(var type = containingObject.GetType(); type != null; type = type.BaseType)
			{
				if(type.GetField(propertyName, AnyInstanceDeclaredOnly) is { } fieldInfo)
				{
					return fieldInfo.GetValue(containingObject);
				}

				if(type.GetProperty(propertyName, AnyInstanceDeclaredOnly) is { CanRead : true } propertyInfo)
				{
					return propertyInfo.GetValue(containingObject);
				}
			}

			return null;
		}

		private static bool TryGetValueUsingReflection(object containingObject, string propertyName, out object value)
		{
			if(containingObject is null)
			{
				value = null;
				return false;
			}

			for(var type = containingObject.GetType(); type != null; type = type.BaseType)
			{
				if(type.GetField(propertyName, AnyInstanceDeclaredOnly) is { } fieldInfo)
				{
					value = fieldInfo.GetValue(containingObject);
					return true;
				}

				if(type.GetProperty(propertyName, AnyInstanceDeclaredOnly) is { CanRead : true } propertyInfo)
				{
					value = propertyInfo.GetValue(containingObject);
					return true;
				}
			}

			value = null;
			return false;
		}

		private static bool TryGetElement(object containingObject, int collectionIndex, out object value)
		{
			if(containingObject is null)
			{
				value = null;
				return false;
			}

			#if DEV_MODE
			Debug.Assert(containingObject is IList, containingObject.GetType().FullName);
			#endif

			var list = (IList)containingObject;
			if(list.Count <= collectionIndex)
			{
				value = null;
				return false;
			}

			value = list[collectionIndex];
			return true;
		}

		private static Type GetMemberType(Type type, NameOrIndex property)
		{
			if(property.name is null)
			{
				if(type.IsArray)
				{
					return type.GetElementType();
				}

				return type.GetGenericArguments()[0];
			}

			return GetMemberType(type, property.name);
		}

		private static Type GetMemberType(Type type, string propertyName)
		{
			for(; type != null; type = type.BaseType)
			{
				var field = type.GetField(propertyName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					return field.FieldType;
				}

				var property = type.GetProperty(propertyName, AnyInstanceDeclaredOnly);
				if(property != null)
				{
					return property.PropertyType;
				}
			}

			return null;
		}

		private static MemberInfo GetMember(Type type, string propertyName)
		{
			for(; type != null; type = type.BaseType)
			{
				var field = type.GetField(propertyName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					return field;
				}

				var property = type.GetProperty(propertyName, AnyInstanceDeclaredOnly);
				if(property != null)
				{
					return property;
				}
			}

			return null;
		}

		private static Type GetMemberType(object containingObject, string propertyName)
		{
			if(containingObject is null)
			{
				return null;
			}

			for(var type = containingObject.GetType(); type != null; type = type.BaseType)
			{
				var field = type.GetField(propertyName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					return field.FieldType;
				}

				var property = type.GetProperty(propertyName, AnyInstanceDeclaredOnly);
				if(property != null)
				{
					return property.PropertyType;
				}
			}

			return null;
		}
		
		#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
		private static readonly ProfilerMarker getValueMarker = new(ProfilerCategory.Gui, "SerializedPropertyExtensions.GetValue");
		private static readonly ProfilerMarker tryGetValueMarker = new(ProfilerCategory.Gui, "SerializedPropertyExtensions.TryGetValue");
		#endif
	}
}