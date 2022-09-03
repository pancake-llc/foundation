using System;
using System.Reflection;
using UnityEngine;
using JetBrains.Annotations;
using Object = UnityEngine.Object;
using Pancake.Init.Internal;

namespace Pancake.Init.Reflection
{
	/// <summary>
	/// Extensions methods for <see cref="UnityEngine.Object"/> type classes that can be used to
	/// <see cref="AddComponent">add components</see> or <see cref="Instantiate">clone objects</see>
	/// while injecting values into fields and properties of the created Instance using reflection.
	/// <para>
	/// This can make it easier to create unit tests for <see cref="MonoBehaviour"/>-derived classes as it allows one to
	/// set the values of non-public fields which in normal use-cases might be meant to only be assigned only through the inspector.
	/// </para>
	/// </summary>
	public static class ObjectExtensions
	{
		private const BindingFlags AllDeclaredOnly = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided <paramref name="arguments"/>.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="arguments"> Zero or more values to set. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property with one of the provided <paramref name="name">names</paramref> was found on the <paramref name="client"/> class <typeparamref name="TComponent"/>,
		/// or if a field or property was found but a <paramref name="value"/> of type <typeparamref name="TValue"/> is not assignable to it.
		/// </exception>
		public static TComponent AddComponent<TComponent>
			(this GameObject gameObject, params (string name, object value)[] arguments)
				where TComponent : Component
		{
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();

			if(arguments == null)
            {
				return result;
            }

			foreach(var argument in arguments)
			{
				SetPropertyOrFieldValue(result, argument.name, argument.value);
			}

			return result;
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <param name="arguments"> Zero or more values to set. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property with one of the provided <paramref name="name">names</paramref> was found on the <paramref name="client"/> class <typeparamref name="TObject"/>,
		/// or if a field or property was found but a <paramref name="value"/> of type <typeparamref name="TValue"/> is not assignable to it.
		/// </exception>
		public static TObject Instantiate<TObject>
			(this TObject original, params (string name, object value)[] arguments)
				where TObject : Object
		{
			if(original == null)
			{
				throw new ArgumentNullException($"The object which you want to instantiate {nameof(TObject)} is null.");
			}

			TObject result;
			GameObject gameObject;
			bool setActive;
			if(original is Component component)
			{
				gameObject = component.gameObject;
				if(gameObject.activeSelf)
				{
					gameObject.SetActive(false);
					result = Object.Instantiate(original);
					gameObject.SetActive(true);
					setActive = true;
				}
				else
                {
					setActive = false;
					result = Object.Instantiate(original);
				}
			}
			else
			{
				gameObject = original as GameObject;
				if(gameObject != null && gameObject.activeSelf)
				{
					setActive = true;
					gameObject.SetActive(false);
					result = Object.Instantiate(original);
					gameObject.SetActive(true);
				}
				else
				{
					setActive = false;
					result = Object.Instantiate(original);
				}
			}

			if(arguments != null)
			{
				foreach(var argument in arguments)
				{
					SetPropertyOrFieldValue(result, argument.name, argument.value);
				}
			}

			if(setActive)
            {
				gameObject.SetActive(true);
            }

			return result;
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided <paramref name="arguments"/>.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="arguments"> Zero or more values to set. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property with one of the provided <paramref name="name">names</paramref> was found on the <paramref name="client"/> class <typeparamref name="TComponent"/>,
		/// or if a field or property was found but a <paramref name="value"/> of type <typeparamref name="TValue"/> is not assignable to it.
		/// </exception>
		public static TComponent AddComponent<TComponent>
			(this GameObject gameObject, params object[] arguments)
				where TComponent : Component
		{
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();

			if(arguments == null)
            {
				return result;
            }

			foreach(var argument in arguments)
			{
				SetPropertyOrFieldValue(result, argument);
			}

			return result;
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
		private static void SetPropertyOrFieldValue<TClient>(TClient client, [NotNull] string name, object value)
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
					else if(!field.FieldType.IsAssignableFrom(value.GetType()))
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
					else if(!property.PropertyType.IsAssignableFrom(value.GetType()))
					{
						throw new MissingMemberException($"Property by name \"{name}\" was found on class {typeof(TClient).Name} but its type {property.PropertyType} was not assignable from provided value of {value}.");
					}
					property.SetValue(client, value, null);
					return;
				default:
					throw new MissingMemberException($"No field or property by name \"{name}\" was found on class {typeof(TClient).Name}.");
			}
        }

        private static FoundMember TryFindFieldOrProperty(Type componentType, [NotNull] string name, out FieldInfo field, out PropertyInfo property)
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
		private static void SetPropertyOrFieldValue<TClient>(TClient client, [NotNull] object value)
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