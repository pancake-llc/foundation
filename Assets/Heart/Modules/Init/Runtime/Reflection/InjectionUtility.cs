using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init.Reflection
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
		private const BindingFlags PublicDeclaredInstance = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		private const BindingFlags AnyDeclaredInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		private static readonly SettersByType settersByType = new SettersByType(8);

		/// <summary>
		/// Assigns an argument received during initialization of client to an instance
		/// field or property by given name found on the client.
		/// <para>
		/// Init only fields and properties are supported, however properties that are
		/// not auto-implemented and do not have a set accessor are not supported.
		/// </para>
		/// </summary>
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
		public static void Inject<TClient, TArgument>([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] TArgument value) where TClient : IArgs<TArgument>
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
		/// </summary>
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
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
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
		/// </summary>
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
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
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
		/// </summary>
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
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
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
		/// </summary>
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
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
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
		/// </summary>
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
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
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
		/// </summary>
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
		public static void Inject<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] TClient client, [DisallowNull] string memberName, [AllowNull] object value)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
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
		public static string GetConstructorArgumentTargetFieldName([DisallowNull] Type classType, [DisallowNull] Type parameterType)
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

		[return: MaybeNull]
		private static string GetConstructorArgumentTargetFieldName([DisallowNull] Type classType, [DisallowNull] Type argumentType, string argumentTypeName)
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
			for(var type = classType; !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
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
			for(var type = classType; !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
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

		/// <summary>
		/// Tries to get the member field or property into which an Init function or constructor parameter
		/// of the given <paramref name="argumentType">type</paramref> and at the given <paramref name="parameterIndex"/>
		/// will likely be assigned to during initialization.
		/// <para>
		/// This information could be useful for purposes such as retrieving the name of the member for displaying in the Inspector,
		/// or retrieving attributes from the field to draw it using custom property drawers in the Inspector.
		/// </para>
		/// </summary>
		/// <param name="clientType"> Type of the class that contains the Init function and the target member. </param>
		/// <param name="argumentType"> Type of the Init argument; the target member must be assignable from this type. </param>
		/// <param name="parameterIndex"> Zero-based index of the argument among the Init function's arguments. </param>
		/// <param name="result"> The member of <paramref name="clientType"/> into which the Init argument is probably going to be assigned to. </param>
		/// <returns> <see langword="true"/> if a match was found; otherwise, <see langword="false"/>. </returns>
		[return: MaybeNull]
		internal static bool TryGetInitArgumentTargetMember([DisallowNull] Type clientType, [DisallowNull] Type argumentType, int parameterIndex, bool requirePublicSetter, out MemberInfo result)
		{
			// Could add argument like prioritizeSerializable / prioritizeField, when searching for attributes?

			// Prio 1: assignable member at index
			// Prio 2: first member of exact same type
			// Prio 3: first member with assignable type

			// Could be a private field
			// Could be a private property
			// Could be a public property with a private setter
			// Could be a public property backed by custom private field
			// NOTE: Does not need to be serializable
			// NOTE: Does not need to have a setter (but must have a getter!?)
			// NOTE: Skip indexers
			List<FieldInfo> assignableFields = new List<FieldInfo>();
			List<PropertyInfo> assignableProperties = new List<PropertyInfo>();

			for(var type = clientType; !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
			{
				int index = 0;
				foreach(var member in clientType.GetMembers(requirePublicSetter ? PublicDeclaredInstance : AnyDeclaredInstance))
				{
					if(member is FieldInfo field)
					{
						if(IsAssignableFrom(field.FieldType, argumentType))
						{
							if(index == parameterIndex)
							{
								result = field;
								return true;
							}

							assignableFields.Add(field);
						}

						index++;
						continue;
					}
					
					if(member is PropertyInfo property)
					{
						if(property.GetIndexParameters().Length > 0 || !property.CanRead ||
							(requirePublicSetter && (!property.CanWrite || !property.GetGetMethod().IsPublic)))
						{
							continue;
						}

						if(IsAssignableFrom(property.PropertyType, argumentType))
						{
							if(index == parameterIndex)
							{
								result = property;
								return true;
							}

							assignableProperties.Add(property);
						}

						index++;
					}
				}
			}

			if(assignableFields.Count == 1)
			{
				result = assignableFields[0];
				return true;
			}

			if(assignableProperties.Count == 1)
			{
				result = assignableProperties[0];
				return true;
			}

			string argumentTypeName = argumentType.Name;
			string camelCase;
			if(char.IsLower(argumentTypeName[0]))
			{
				camelCase = argumentTypeName;
				if(TryGetMemberByName(camelCase, out result))
				{
					return true;
				}

				// guiContent -> GuiContent
				// guiContent -> GUIContent
				string PascalCase = char.ToUpperInvariant(argumentTypeName[0]) + argumentTypeName.Substring(1);
				for(int current = 1, next = 2, count = PascalCase.Length - 1; current < count; current++, next++)
				{
					if(char.IsLower(PascalCase[current]))
					{
						PascalCase = PascalCase.Substring(0, current) + char.ToUpperInvariant(PascalCase[current]) + PascalCase.Substring(next);
						if(TryGetMemberByName(PascalCase, out result))
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

				if(TryGetMemberByName(camelCase, out result))
				{
					return true;
				}
			}

			string _camelCase = "_" + camelCase;
			result = assignableFields.FirstOrDefault(member => string.Equals(member.Name, _camelCase));
			if(result != null)
			{
				return true;
			}

			string m_camelCase = "m_" + camelCase;
			if(TryGetMemberByName(m_camelCase, out result))
			{
				return true;
			}

			result = assignableFields.FirstOrDefault(field => field != null && field.FieldType == argumentType);
			if(result != null)
			{
				return true;
			}

			result = assignableProperties.FirstOrDefault(property => property != null && property.PropertyType == argumentType);
			if(result != null)
			{
				return true;
			}

			result = (MemberInfo)assignableFields.FirstOrDefault() ?? assignableProperties.FirstOrDefault();
			return result != null;

			bool TryGetMemberByName(string name, out MemberInfo member)
			{
				member = assignableFields.FirstOrDefault(member => string.Equals(member.Name, name));
				if(member != null)
				{
					return true;
				}

				member = assignableProperties.FirstOrDefault(member => string.Equals(member.Name, name));
				if(member != null)
				{
					return true;
				}

				return false;
			}
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
		public static bool IsClassMemberSerialized([DisallowNull] Type classType, [DisallowNull] string memberName)
		{
			for(var type = classType; !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
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

		private static bool AssignableClassMemberExists([DisallowNull] Type classType, [DisallowNull] string memberName, [DisallowNull] Type assignedType)
		{
			for(var type = classType; !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
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

		private static void InjectInternal([DisallowNull] object client, [DisallowNull] string memberName, [AllowNull] object value)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
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

		[return: NotNull]
		private static MemberInfo GetClassMemberForInjection([DisallowNull] object client, [DisallowNull] string memberName)
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

			for(var type = client.GetType(); !TypeUtility.IsNullOrBaseType(type); type = type.BaseType)
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

				var property = type.GetProperty(memberName, AnyDeclaredInstance);
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
			return owningClassType.GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
		}

		private static string GetBackingFieldName(string propertyName)
		{
			return string.Concat("<", propertyName, ">k__BackingField");
		}

		private static bool IsAssignableFrom(Type fieldOrPropertyType, Type assignedType)
		{
			if(fieldOrPropertyType.IsAssignableFrom(assignedType))
			{
				return true;
			}

			if(typeof(IAny).IsAssignableFrom(fieldOrPropertyType) && fieldOrPropertyType.IsGenericType)
			{
				return fieldOrPropertyType.GetGenericArguments()[0].IsAssignableFrom(assignedType);
			}

			return false;
		}
	}
}