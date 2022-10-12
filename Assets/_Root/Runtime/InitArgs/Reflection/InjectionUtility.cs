using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[assembly: InternalsVisibleTo("InitArgs.Netcode")]
[assembly: InternalsVisibleTo("InitArgs.FishNet")]
namespace Pancake.Init.Reflection
{
	using Setters = Dictionary<string, MemberInfo>;
	using SettersByType = Dictionary<Type, Dictionary<string, MemberInfo>>;

	/// <summary>
	/// Utility class for injecting values to fields and properties during initialization
	/// using reflection.
	/// <para>
	/// Makes it possible to assign to readonly fields and properties outside of the constructor.
	/// </para>
	/// </summary>
	public static class InjectionUtility
	{
		private const BindingFlags AnyDeclaredInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		private static readonly SettersByType settersByType = new SettersByType(8);

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TArgument>([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] TArgument value) where TClient : IArgs<TArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TFirstArgument, TSecondArgument>
			([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// <param name="client"> The client that contains the field or property. </param>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <param name="value"> The value to assign to the field or property. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="client"/> or <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found on the client
		/// or if property by given name is not auto-implemented and does not have a set accessor.
		/// </exception>
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([JetBrains.Annotations.NotNull] TClient client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			InjectInternal(client, memberName, value);
		}

		/// <summary>
		/// Gets the name of the field or property into which the <paramref name="parameter">constructor argument</paramref>
		/// accepted by the object of type <paramref name="classType"/> is assigned to in the constructor of the object.
		/// <para>
		/// Uses reflection to figure out the name by searching for a field or property with their name closely
		/// matching that of the <paramref name="parameter"/>, or failing that, their type matching the type of the parameter exactly.
		/// </para>
		/// </summary>
		/// <param name="classType"> Type of the class containing the field. </param>
		/// <param name="parameterType"> The type of the constructor parameter. </param>
		/// <returns> The name of an instance field or property matching the <paramref name="parameter"/>. </returns>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property is found with their name closely matching that of the <paramref name="parameter"/>
		/// or their type matching the type of the parameter exactly.
		/// </exception>
		/// <exception cref="AmbiguousMatchException">
		/// Thrown if no field or property is found with their name closely matching that of the <paramref name="parameter"/>,
		/// and more than one field or property is found with their type matching the type of the parameter exactly.
		/// </exception>
		public static string GetConstructorArgumentTargetFieldName([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] Type parameterType)
		{
			string argumentTypeName = parameterType.Name;
			string memberName = GetConstructorArgumentTargetFieldName(classType, parameterType, argumentTypeName);
			if(memberName != null)
            {
				return memberName;
            }

			if(argumentTypeName.Length == 1)
            {
				throw new MissingMemberException(classType.Name, argumentTypeName);
			}
			
			// Try without first letter to support for example IInputManager -> _inputManager;
			memberName = GetConstructorArgumentTargetFieldName(classType, parameterType, argumentTypeName.Substring(1));
			if(memberName != null)
			{
				return memberName;
			}

			throw new MissingMemberException(classType.Name, argumentTypeName);
		}

		internal static void OnInitializableReset(MonoBehaviour initializable)
		{
			var type = initializable.GetType();
			foreach(var initializer in initializable.GetComponents<IInitializer>())
			{
				if(initializer.Target == null && initializer.TargetIsAssignableOrConvertibleToType(type))
				{
					initializer.Target = initializable;
					return;
				}
			}
		}

		[CanBeNull]
		private static string GetConstructorArgumentTargetFieldName([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] Type argumentType, string argumentTypeName)
        {
			if(AssignableClassMemberExists(classType, argumentTypeName, argumentType))
            {
				return argumentTypeName;
            }

			string camelCase;
			if(char.IsLower(argumentTypeName[0]))
			{
				camelCase = argumentTypeName;

				string PascalCase = char.ToUpperInvariant(argumentTypeName[0]) + argumentTypeName.Substring(1);

				if(AssignableClassMemberExists(classType, PascalCase, argumentType))
				{
					return PascalCase;
				}

				// guiContent -> GuiContent
				// guiContent -> GUIContent
				for(int current = 1, next = 2, count = PascalCase.Length - 1; current < count; current++, next++)
				{
					if(char.IsLower(PascalCase[current]))
					{
						PascalCase = PascalCase.Substring(0, current) + char.ToUpperInvariant(PascalCase[current]) + PascalCase.Substring(next);

						if(AssignableClassMemberExists(classType, camelCase, argumentType))
						{
							return PascalCase;
						}
					}
				}
			}
			else
			{
				camelCase = char.ToLowerInvariant(argumentTypeName[0]) + argumentTypeName.Substring(1);

				// GUIContent -> guiContent
				// ID123 -> id123
				for(int current = 1, next = 2, count = camelCase.Length - 1; current < count; current++, next++)
				{
					if(char.IsUpper(camelCase[current]) && (next == count || !char.IsLower(camelCase[next])))
					{
						camelCase = camelCase.Substring(0, current) + char.ToLowerInvariant(camelCase[current]) + camelCase.Substring(next);
					}
				}

				if(AssignableClassMemberExists(classType, camelCase, argumentType))
				{
					return camelCase;
				}
			}

			string _camelCase = "_" + camelCase;

			if(AssignableClassMemberExists(classType, _camelCase, argumentType))
			{
				return _camelCase;
			}

			string m_camelCase = "m_" + camelCase;

			if(AssignableClassMemberExists(classType, m_camelCase, argumentType))
			{
				return m_camelCase;
			}

			FieldInfo fieldOfMatchingType = null;
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				foreach(var field in type.GetFields(AnyDeclaredInstance))
				{
					if(field.FieldType != argumentType)
					{
						continue;
					}

					if(fieldOfMatchingType != null)
					{
						throw new AmbiguousMatchException($"Unable to determine target field on client {classType.Name} for injecting parameter {argumentTypeName}; no field or property was found with name closely matching that of the parameter, and more than one field was found with their type matching the type of the parameter exactly.");
					}

					fieldOfMatchingType = field;
				}
			}

			if(fieldOfMatchingType != null)
            {
				return fieldOfMatchingType.Name;
			}

			PropertyInfo propertyOfMatchingType = null;
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				foreach(var property in classType.GetProperties(AnyDeclaredInstance))
				{
					if(property.PropertyType != argumentType)
					{
						continue;
					}

					fieldOfMatchingType = GetPropertyBackingField(type, property.Name);

					if(fieldOfMatchingType == null && !property.CanWrite)
                    {
						continue;
					}

					if(propertyOfMatchingType != null)
					{
						throw new AmbiguousMatchException($"Unable to determine target property on client {classType.Name} for injecting parameter {argumentTypeName}; no field or property was found with name closely matching that of the parameter, and more than one property was found with their type matching the type of the parameter exactly.");
					}

					propertyOfMatchingType = property;
				}
			}

			if(fieldOfMatchingType != null)
            {
				return fieldOfMatchingType.Name;
            }

			if(propertyOfMatchingType != null)
            {
				return propertyOfMatchingType.Name;
			}

			return null;
		}

		[CanBeNull]
		internal static bool TryGetConstructorArgumentTarget([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] Type argumentType, string argumentTypeName, out MemberInfo member)
        {
			if(TryGetAssignableClassMember(classType, argumentTypeName, argumentType, out member))
            {
				return true;
            }

			string camelCase;
			if(char.IsLower(argumentTypeName[0]))
			{
				camelCase = argumentTypeName;

				string PascalCase = char.ToUpperInvariant(argumentTypeName[0]) + argumentTypeName.Substring(1);

				if(TryGetAssignableClassMember(classType, PascalCase, argumentType, out member))
				{
					return true;
				}

				// guiContent -> GuiContent
				// guiContent -> GUIContent
				for(int current = 1, next = 2, count = PascalCase.Length - 1; current < count; current++, next++)
				{
					if(char.IsLower(PascalCase[current]))
					{
						PascalCase = PascalCase.Substring(0, current) + char.ToUpperInvariant(PascalCase[current]) + PascalCase.Substring(next);

						if(TryGetAssignableClassMember(classType, camelCase, argumentType, out member))
						{
							return true;
						}
					}
				}
			}
			else
			{
				camelCase = char.ToLowerInvariant(argumentTypeName[0]) + argumentTypeName.Substring(1);

				// GUIContent -> guiContent
				// ID123 -> id123
				for(int current = 1, next = 2, count = camelCase.Length - 1; current < count; current++, next++)
				{
					if(char.IsUpper(camelCase[current]) && (next == count || !char.IsLower(camelCase[next])))
					{
						camelCase = camelCase.Substring(0, current) + char.ToLowerInvariant(camelCase[current]) + camelCase.Substring(next);
					}
				}

				if(TryGetAssignableClassMember(classType, camelCase, argumentType, out member))
				{
					return true;
				}
			}

			string _camelCase = "_" + camelCase;

			if(TryGetAssignableClassMember(classType, _camelCase, argumentType, out member))
			{
				return true;
			}

			string m_camelCase = "m_" + camelCase;

			if(TryGetAssignableClassMember(classType, m_camelCase, argumentType, out member))
			{
				return true;
			}

			FieldInfo fieldOfMatchingType = null;
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				foreach(var field in type.GetFields(AnyDeclaredInstance))
				{
					if(field.FieldType != argumentType)
					{
						continue;
					}

					if(fieldOfMatchingType != null)
					{
						fieldOfMatchingType = null;
						type = typeof(object);
						break;
					}

					fieldOfMatchingType = field;
				}
			}

			if(fieldOfMatchingType != null)
            {
				member = fieldOfMatchingType;
				return true;
			}

			PropertyInfo propertyOfMatchingType = null;
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				foreach(var property in classType.GetProperties(AnyDeclaredInstance))
				{
					if(property.PropertyType != argumentType)
					{
						continue;
					}

					fieldOfMatchingType = GetPropertyBackingField(type, property.Name);

					if(fieldOfMatchingType == null && !property.CanWrite)
                    {
						continue;
					}

					if(propertyOfMatchingType != null)
					{
						propertyOfMatchingType = null;
						type = typeof(object);
						break;
					}

					propertyOfMatchingType = property;
				}
			}

			if(fieldOfMatchingType != null)
            {
				member = fieldOfMatchingType;
				return true;
			}

			if(propertyOfMatchingType != null)
            {
				member = propertyOfMatchingType;
				return true;
			}

			return false;
		}

		private static bool TryGetAssignableClassMember([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] string memberName, [JetBrains.Annotations.NotNull] Type assignedType, out MemberInfo member)
        {
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				var field = type.GetField(memberName, AnyDeclaredInstance);
				if(field != null && IsAssignableFrom(field.FieldType, assignedType))
				{
					member = field;
					return true;
				}

				var property = type.GetProperty(memberName, AnyDeclaredInstance);
				if(property == null || !IsAssignableFrom(property.PropertyType, assignedType))
                {
					continue;
                }

				if(property.CanWrite)
                {
					member = property;
					return true;
                }

				field = GetPropertyBackingField(type, memberName);
				if(field != null)
				{
					member = field;
					return true;
				}
			}

			member = null;
			return false;
		}

		/// <summary>
		/// Returns a value indicating whether or not field or property by name <paramref name="memberName"/> on class <paramref name="classType"/> is serialized or not.
		/// </summary>
		/// <param name="classType"> Type of the class that contains the field or property. </param>
		/// <param name="memberName"> The name of an instance field or property that exists on class <paramref name="classType"/> or one of its inherited types. </param>
		/// <returns> <see langword="true"/> if field or property by name <paramref name="memberName"/> is serialized; otherwise <see langword="false"/>. </returns>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by name <paramref name="memberName"/> is found on class <paramref name="classType"/> or any of its inherited types.
		/// </exception>
		public static bool IsClassMemberSerialized([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] string memberName)
        {
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				var field = classType.GetField(memberName, AnyDeclaredInstance);
				if(field != null)
				{
					return IsFieldSerialized(field);
				}

				var property = classType.GetProperty(memberName, AnyDeclaredInstance);
				if(property is null)
				{
					continue;
				}

				field = GetPropertyBackingField(type, memberName);
				if(field != null)
				{
					return field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null;
				}
			}

			throw new MissingMemberException($"Failed to find field or property by name '{memberName}' on class '{classType.Name} or any of its inherited types.");
		}

		internal static string GetConstructorArgumentName(string fieldName)
		{
			if(string.IsNullOrEmpty(fieldName))
            {
				return "";
            }

			char firstChar;
			if(fieldName[0] == '<')
            {
				int end = fieldName.IndexOf('>');
				if(end != -1)
                {
					firstChar = char.ToLowerInvariant(fieldName[1]);
					return firstChar + fieldName.Substring(2, end - 2);
                }
            }

			firstChar = char.ToLowerInvariant(fieldName[0]);
			return firstChar + fieldName.Substring(1);
		}

        private static bool IsFieldSerialized(FieldInfo field)
        {
            if(field.IsInitOnly)
            {
                return false;
            }

            if(field.GetCustomAttribute<NonSerializedAttribute>() != null)
            {
                return false;
            }

            if(field.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }

			if(field.GetCustomAttribute<SerializeReference>() != null)
			{
				return true;
			}

			if(!field.IsPublic)
			{
				return false;
			}

			var type = field.FieldType;
			return type.IsSerializable && !type.IsAbstract;
        }

		private static bool AssignableClassMemberExists([JetBrains.Annotations.NotNull] Type classType, [JetBrains.Annotations.NotNull] string memberName, [JetBrains.Annotations.NotNull] Type assignedType)
        {
			for(var type = classType; !IsNullOrBaseType(type); type = type.BaseType)
			{
				var field = type.GetField(memberName, AnyDeclaredInstance);
				if(field != null && IsAssignableFrom(field.FieldType, assignedType))
				{
					return true;
				}

				var property = type.GetProperty(memberName, AnyDeclaredInstance);
				if(property == null || !IsAssignableFrom(property.PropertyType, assignedType))
                {
					continue;
                }

				if(property.CanWrite)
                {
					return true;
                }

				field = GetPropertyBackingField(type, memberName);
				if(field != null)
				{
					return true;
				}

				throw new MissingMemberException($"Unable to assign to property {property.Name}: Property '{type.Name}.{property.Name}' is not an auto-property and does not have a set accessor.");
			}

			return false;
		}

		private static void InjectInternal([JetBrains.Annotations.NotNull] object client, [JetBrains.Annotations.NotNull] string memberName, [CanBeNull] object value)
		{
			#if DEBUG
			if(client is null)
            {
				throw new ArgumentNullException(nameof(client));
			}
			if(memberName is null)
			{
				throw new ArgumentNullException(nameof(memberName));
			}
			#endif

			var setter = GetClassMemberForInjection(client, memberName);

			if(setter is FieldInfo cachedField)
            {
				cachedField.SetValue(client, value);
				return;
			}

			if(setter is PropertyInfo cachedProperty)
            {
				cachedProperty.SetValue(client, value, null);
				return;
			}
			
			throw new MissingMemberException(client.GetType().Name, memberName);
		}

		[JetBrains.Annotations.NotNull]
		private static MemberInfo GetClassMemberForInjection([JetBrains.Annotations.NotNull] object client, [JetBrains.Annotations.NotNull] string memberName)
		{
			if(settersByType.TryGetValue(client.GetType(), out var setters))
            {
				if(setters.TryGetValue(memberName, out var setter))
                {
					return setter;
                }
			}
			else
            {
				setters = new Setters(1);
				settersByType[client.GetType()] = setters;
			}

			for(var type = client.GetType(); !IsNullOrBaseType(type); type = type.BaseType)
			{
				FieldInfo field = type.GetField(memberName, AnyDeclaredInstance);
				if(field != null)
				{
					setters[memberName] = field;
					return field;
				}

				field = GetPropertyBackingField(type, memberName);
				if(field != null)
				{
					setters[memberName] = field;
					return field;
				}

				var property = type.GetProperty(memberName);
				if(property is null)
				{
					continue;
				}

				if(!property.CanWrite)
				{
					throw new MissingMemberException($"Unable to assign to property {property.Name}: Property '{type.Name}.{property.Name}' is not an auto-property and does not have a set accessor.");
				}

				setters[memberName] = property;
				return property;
			}

			throw new MissingMemberException(client.GetType().Name, memberName);
		}

		private static FieldInfo GetPropertyBackingField(Type owningClassType, string propertyName)
		{
			return owningClassType.GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private static string GetBackingFieldName(string propertyName)
		{
			return string.Concat("<", propertyName, ">k__BackingField");
		}

		private static bool IsNullOrBaseType([CanBeNull] Type type)
        {
			if(type is null)
            {
				return true;
            }

			if(type.IsGenericType)
            {
				var typeDefinition = type.GetGenericTypeDefinition();
				return typeDefinition == typeof(MonoBehaviour<>) || typeDefinition == typeof(ConstructorBehaviour<>) || typeDefinition == typeof(ScriptableObject<>);
            }

			return type == typeof(MonoBehaviour) || type == typeof(ScriptableObject) || type == typeof(object);
        }

		private static bool IsAssignableFrom(Type fieldOrPropertyType, Type assignedType)
        {
			if(fieldOrPropertyType.IsAssignableFrom(assignedType))
            {
				return true;
            }

			if(fieldOrPropertyType.IsGenericType)
            {
				var typeDefinition = fieldOrPropertyType.GetGenericTypeDefinition();
				if(typeDefinition == typeof(Any<>))
                {
					return fieldOrPropertyType.GetGenericArguments()[0].IsAssignableFrom(assignedType);
				}
            }

			return false;
        }
	}
}