using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Sisus.Init.Internal;

namespace Sisus.Init.Reflection.Internal
{
	internal static class ReflectionUtility
	{
		private const BindingFlags AllDeclaredOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		/// <summary>
		/// Sets the value of a field or property with a certain name found on the target.
		/// </summary>
		/// <typeparam name="TClient"> Type of the object that contains the field or property. </typeparam>
		/// <param name="client"> Object that contains the field or property whose value is set. </param>
		/// <param name="name"> Name of the field or property. </param>
		/// <param name="value"> Value to set for field or property. </param>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property with the provided <paramref name="name"/> was found on the <paramref name="client"/> class <typeparamref name="TClient"/>,
		/// or if a field or property was found but a <paramref name="value"/> of type <typeparamref name="TValue"/> is not assignable to it.
		/// </exception>
		public static void SetPropertyOrFieldValue<TClient>(TClient client, [DisallowNull] string name, object value)
		{
			switch(TryFindFieldOrProperty(typeof(TClient), name, out FieldInfo field, out PropertyInfo property))
			{
				case FoundMember.Field:
					if(value is null)
					{
						if(field.FieldType.IsValueType)
						{
							throw new MissingMemberException($"Field by name \"{name}\" was found on class {typeof(TClient).Name} but its type {field.FieldType} was not assignable from provided value of null.");
						}
					}
					else if(!field.FieldType.IsInstanceOfType(value))
					{
						throw new MissingMemberException($"Field by name \"{name}\" was found on class {typeof(TClient).Name} but its type {field.FieldType} was not assignable from provided value of {value}.");
					}

					field.SetValue(client, value);
					return;
				case FoundMember.Property:
					if(value is null)
					{
						if(property.PropertyType.IsValueType)
						{
							throw new MissingMemberException($"Property by name \"{name}\" was found on class {typeof(TClient).Name} but its type {property.PropertyType} was not assignable from provided value of null.");
						}
					}
					else if(!property.PropertyType.IsInstanceOfType(value))
					{
						throw new MissingMemberException($"Property by name \"{name}\" was found on class {typeof(TClient).Name} but its type {property.PropertyType} was not assignable from provided value of {value}.");
					}
					property.SetValue(client, value, null);
					return;
				default:
					throw new MissingMemberException($"No field or property by name \"{name}\" was found on class {typeof(TClient).Name}.");
			}
		}

		/// <summary>
		/// Sets the value of a field or property with a certain name found on the target.
		/// </summary>
		/// <typeparam name="TClient"> Type of the object that contains the field or property. </typeparam>
		/// <param name="client"> Object that contains the field or property whose value is set. </param>
		/// <param name="name"> Name of the field or property. </param>
		/// <param name="value"> Value to set for field or property. </param>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property with the provided <paramref name="name"/> was found on the <paramref name="client"/> class <typeparamref name="TClient"/>,
		/// or if a field or property was found but a <paramref name="value"/> of type <typeparamref name="TValue"/> is not assignable to it.
		/// </exception>
		public static void SetPropertyOrFieldValue<TClient>(TClient client, [DisallowNull] object value)
		{
			switch(TryFindFieldOrProperty(typeof(TClient), value.GetType(), out FieldInfo field, out PropertyInfo property))
			{
				case FoundMember.Field:
					field.SetValue(client, value);
					return;
				case FoundMember.Property:
					property.SetValue(client, value, null);
					return;
				default:
					throw new MissingMemberException($"No field or property assignable from type {value.GetType().Name} was found on class {typeof(TClient).Name}.");
			}
		}

		private static FoundMember TryFindFieldOrProperty(Type componentType, [DisallowNull] string name, out FieldInfo field, out PropertyInfo property)
		{
			for(var type = componentType; type != null; type = type.BaseType)
			{
				field = type.GetField(name, AllDeclaredOnly);
				if(field != null)
				{
					property = null;
					return FoundMember.Field;
				}

				property = type.GetProperty(name, AllDeclaredOnly);
				if(property != null && property.CanWrite)
				{
					return FoundMember.Property;
				}
			}

			field = null;
			property = null;
			return FoundMember.None;
		}

		private static FoundMember TryFindFieldOrProperty(Type componentType, Type assignableFromType, out FieldInfo field, out PropertyInfo property)
		{
			for(var type = componentType; type != null; type = type.BaseType)
			{
				foreach(var fieldToTest in type.GetFields(AllDeclaredOnly))
				{
					if(fieldToTest.FieldType == assignableFromType)
					{
						field = fieldToTest;
						property = null;
						return FoundMember.Field;
					}
				}

				foreach(var propertyToTest in type.GetProperties(AllDeclaredOnly))
				{
					if(propertyToTest.PropertyType == assignableFromType && propertyToTest.CanWrite)
					{
						property = propertyToTest;
						field = null;
						return FoundMember.Field;
					}
				}
			}

			for(var t = componentType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				foreach(var fieldToTest in t.GetFields(AllDeclaredOnly))
				{
					if(fieldToTest.FieldType.IsAssignableFrom(assignableFromType))
					{
						field = fieldToTest;
						property = null;
						return FoundMember.Field;
					}
				}

				foreach(var propertyToTest in t.GetProperties(AllDeclaredOnly))
				{
					if(propertyToTest.PropertyType.IsAssignableFrom(assignableFromType))
					{
						property = propertyToTest;
						field = null;
						return FoundMember.Field;
					}
				}
			}

			field = null;
			property = null;
			return FoundMember.None;
		}

		private enum FoundMember { None, Field, Property }
	}
}
