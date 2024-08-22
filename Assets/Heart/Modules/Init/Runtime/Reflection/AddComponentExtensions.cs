using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.Reflection.Internal.ReflectionUtility;

namespace Sisus.Init.Reflection
{
	/// <summary>
	/// Extensions methods for <see cref="UnityEngine.Object"/> type classes that can be used to
	/// <see cref="AddComponent">add components</see> or <see cref="Instantiate">clone objects</see>
	/// while injecting values into fields and properties of the created instance using reflection.
	/// <para>
	/// This can make it easier to create unit tests for <see cref="MonoBehaviour"/>-derived classes as it allows one to
	/// set the values of non-public fields which in normal use-cases might be meant to only be assigned only through the inspector.
	/// </para>
	/// </summary>
	public static class AddComponentExtensions
	{
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
			([DisallowNull] this GameObject gameObject, [DisallowNull] params (string name, object value)[] arguments)
				where TComponent : Component
		{
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.", default(Exception));
			}

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();

			if(arguments == null)
			{
				return result;
			}

			foreach(var (name, value) in arguments)
			{
				SetPropertyOrFieldValue(result, name, value);
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
			([DisallowNull] this GameObject gameObject, [DisallowNull] params object[] arguments)
				where TComponent : Component
		{
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.", default(Exception));
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
	}
}
