using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Sisus.Init.Internal.TypeUtility;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Extension methods for <see cref="SerializedProperty"/>.
	/// </summary>
	internal static class SerializedPropertyExtensions
	{
		private readonly struct Property
		{
			public readonly string name;
			public readonly int collectionIndex;

			public Property(string name)
			{
				this.name = name;
				collectionIndex = -1;
			}

			public Property(int collectionIndex)
			{
				name = null;
				this.collectionIndex = collectionIndex;
			}
		}

		private const BindingFlags AnyInstanceDeclaredOnly = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		private static readonly Regex collectionIndexRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);
		private static readonly Dictionary<Type, bool> hasFoldoutCache = new();

		public static object GetValue([DisallowNull] this SerializedProperty serializedProperty)
		{
			if(serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
			{
				return serializedProperty.managedReferenceValue;
			}

			string propertyPath = serializedProperty.propertyPath;
			object value = serializedProperty.serializedObject.targetObject;

			int startIndex = 0;
			Property property = default;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref property))
			{
				value = GetValue(value, property);
			}

			return value;
		}

		public static MemberInfo GetMemberInfo([DisallowNull] this SerializedProperty serializedProperty)
		{
			string propertyPath = serializedProperty.propertyPath;
			Type previousType = serializedProperty.serializedObject.targetObject.GetType();
			Type type = previousType;
            
			int startIndex = 0;
			Property property = default;
			while(TryGetNextProperty(propertyPath, ref startIndex, ref property))
			{
				previousType = type;
				type = GetMemberType(previousType, property);
			}

			return GetMember(previousType, property.name);
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

		public static void SetValue(this SerializedProperty serializedProperty, object value)
		{
			#if UNITY_2021_3_OR_NEWER
			if(serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
			{
				serializedProperty.managedReferenceValue = value;
				return;
			}
			#endif

			string propertyPath = serializedProperty.propertyPath;
			var serializedObject = serializedProperty.serializedObject;
			object obj = serializedProperty.serializedObject.targetObject;

			int startIndex = 0;
			Property property = default;
			Property nextPropertyOrDefault = default;
			TryGetNextProperty(propertyPath, ref startIndex, ref property);
			while(TryGetNextProperty(propertyPath, ref startIndex, ref nextPropertyOrDefault))
			{
				obj = GetValue(obj, property);
				property = nextPropertyOrDefault;
			}

			if(obj.GetType().IsValueType)
			{
				throw new InvalidOperationException($"Cannot use SerializedObject.SetValue on a struct object. Either change {obj.GetType().Name} to a class, or use SetValue with a parent SerializedProperty.");
			}

			Undo.RecordObject(serializedObject.targetObject, $"Set {nextPropertyOrDefault.name}");

			SetValue(obj, property, value);

			EditorUtility.SetDirty(serializedObject.targetObject);
			serializedObject.ApplyModifiedProperties();
		}

		private static void SetValue(object obj, Property property, object value)
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

		private static bool TryGetNextProperty(string propertyPath, ref int startIndex, ref Property result)
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
				result = new Property(collectionIndex);
				return true;
			}

			int endIndex = propertyPath.IndexOf('.', startIndex);
			if(endIndex == -1)
			{
				result = new Property(propertyPath.Substring(startIndex));
				startIndex = propertyPath.Length;
				return true;
			}

			result = new Property(propertyPath.Substring(startIndex, endIndex - startIndex));
			startIndex = endIndex + 1;
			return true;
		}

		private static object GetValue(object obj, Property property)
		{
			return property.name is null ? ((IList)obj)[property.collectionIndex] : GetValue(obj, property.name);
		}

		private static Type GetMemberType(Type type, Property property)
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

		private static object GetValue(object obj, string propertyName)
		{
			if(obj is null)
			{
				return null;
			}

			for(var type = obj.GetType(); type != null; type = type.BaseType)
			{
				var field = type.GetField(propertyName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					return field.GetValue(obj);
				}

				var property = type.GetProperty(propertyName, AnyInstanceDeclaredOnly);
				if(property != null && property.CanRead)
				{
					return property.GetValue(obj);
				}
			}

			return null;
		}

		private static Type GetType(object obj, string propertyName)
		{
			if(obj is null)
			{
				return null;
			}

			for(var type = obj.GetType(); type != null; type = type.BaseType)
			{
				var field = type.GetField(propertyName, AnyInstanceDeclaredOnly);
				if(field != null)
				{
					return field.FieldType;
				}

				var property = type.GetProperty(propertyName, AnyInstanceDeclaredOnly);
				if(property != null && property.CanRead)
				{
					return property.PropertyType;
				}
			}

			return null;
		}

		internal static bool HasFoldoutInInspector([DisallowNull] this SerializedProperty property, [DisallowNull] Type valueType)
		{
			if(!property.hasChildren)
			{
				return false;
			}

			if(property.isExpanded)
			{
				return true;
			}

			if(!hasFoldoutCache.TryGetValue(valueType, out bool hasFoldout))
			{
				hasFoldout = property.propertyType switch
				{
					SerializedPropertyType.Generic or SerializedPropertyType.ManagedReference => IsSerializableByUnity(valueType) && HasAnySerializedFields(valueType),
					_ => false,
				};

				hasFoldoutCache.Add(valueType, hasFoldout);
			}

			return hasFoldout;

			static bool HasAnySerializedFields([DisallowNull] Type valueType)
			{
				for(var type = valueType; !IsNullOrBaseType(type); type = type.BaseType)
				{
					foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
					{
						if((field.IsPublic && IsSerializableByUnity(field.FieldType)
						|| field.GetCustomAttribute<SerializeField>() is not null)
						&& field.GetCustomAttribute<HideInInspector>() is null)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}