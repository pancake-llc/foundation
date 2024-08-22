using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.Reflection.Internal.ReflectionUtility;
using Object = UnityEngine.Object;

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
	public static class InstantiateExtensions
	{
		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</see>
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
			([DisallowNull] this TObject original, [DisallowNull] params (string name, object value)[] arguments)
				where TObject : Object
		{
			Instantiate(original, out TObject clone, out GameObject gameObject, out bool setActive);

			if(arguments != null)
			{
				foreach(var (name, value) in arguments)
				{
					SetPropertyOrFieldValue(clone, name, value);
				}
			}

			if(setActive)
			{
				gameObject.SetActive(true);
			}

			return clone;
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</see>
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
			([DisallowNull] this TObject original, [DisallowNull] params object[] arguments)
				where TObject : Object
		{
			Instantiate(original, out TObject clone, out GameObject gameObject, out bool setActive);

			if(arguments != null)
			{
				foreach(var argument in arguments)
				{
					SetPropertyOrFieldValue(clone, argument);
				}
			}

			if(setActive)
			{
				gameObject.SetActive(true);
			}

			return clone;
		}

		private static void Instantiate<TObject>([DisallowNull] TObject original, out TObject clone, out GameObject gameObject, out bool setActive) where TObject : Object
		{
			if(!original)
			{
				throw new ArgumentNullException($"The object which you want to instantiate {nameof(TObject)} is null.", default(Exception));
			}

			if(original is Component component)
			{
				gameObject = component.gameObject;
				if(gameObject.activeSelf)
				{
					setActive = true;
					gameObject.SetActive(false);
					clone = Object.Instantiate(original);
					gameObject.SetActive(true);
				}
				else
				{
					setActive = false;
					clone = Object.Instantiate(original);
				}
			}
			else
			{
				gameObject = original as GameObject;
				if(gameObject != null && gameObject.activeSelf)
				{
					setActive = true;
					gameObject.SetActive(false);
					clone = Object.Instantiate(original);
					gameObject.SetActive(true);
				}
				else
				{
					setActive = false;
					clone = Object.Instantiate(original);
				}
			}
		}
	}
}