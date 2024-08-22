using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init.Reflection
{
	internal static class ReflectionExtensions
	{
		private delegate void InitHandler<TClient, TArgument>(TClient instance, TArgument value) where TClient : class;
		private delegate void InitHandler<TClient, TFirstArgument, TSecondArgument>(TClient instance, TFirstArgument firstArgument, TSecondArgument secondArgument) where TClient : class;
		private delegate void InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument>(TClient instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TClient : class;
		private delegate void InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(TClient instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TClient : class;
		private delegate void InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(TClient instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TClient : class;

		private const BindingFlags DeclaredOnlyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided <paramref name="argument"/>.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TArgument"> Type of the initialization argument. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="argument">
		/// The argument passed to the component's Init(<typeparamref name="TArgument"/>) function,
		/// of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TArgument"/> was found on the class <typeparamref name="TComponent"/>.
		/// </exception>
		public static TComponent AddComponent<TComponent, TArgument>
			(this GameObject gameObject, TArgument argument)
				where TComponent : Component
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();
			result.Init(argument);
			return result;
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first initialization argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second initialization argument. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument">
		/// The first argument passed to the component's Init(<typeparamref name="TFirstArgument"/>, <typeparamref name="TSecondArgument"/>)
		/// function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="secondArgument">
		/// The second argument passed to the component's Init(<typeparamref name="TFirstArgument"/>, <typeparamref name="TSecondArgument"/>)
		/// function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TArgument"/> was found on the class <typeparamref name="TComponent"/>.
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument>
			(this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : Component
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();
			result.Init(firstArgument, secondArgument);
			return result;
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first initialization argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second initialization argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third initialization argument. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument">
		/// The first argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="secondArgument">
		/// The second argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="thirdArgument">
		/// The third argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TArgument"/> was found on the class <typeparamref name="TComponent"/>.
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>
			(this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : Component
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();
			result.Init(firstArgument, secondArgument, thirdArgument);
			return result;
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first initialization argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second initialization argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third initialization argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth initialization argument. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument">
		/// The first argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="secondArgument">
		/// The second argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="thirdArgument">
		/// The third argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="fourthArgument">
		/// The fourth argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TArgument"/> was found on the class <typeparamref name="TComponent"/>.
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : Component
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();
			result.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
			return result;
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and initializes the component using the provided arguments.
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first initialization argument. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second initialization argument. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of the third initialization argument. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of the fourth initialization argument. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of the fifth initialization argument. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument">
		/// The first argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="secondArgument">
		/// The second argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="thirdArgument">
		/// The third argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="fourthArgument">
		/// The fourth argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <param name="fifthArgument">
		/// The fifth argument passed to the component's Init function, of if the component doesn't have one, injected into a field with a matching type.
		/// </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TArgument"/> was found on the class <typeparamref name="TComponent"/>.
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : Component
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			TComponent result = typeof(TComponent) == typeof(Transform) ? gameObject.GetComponent<TComponent>() : gameObject.AddComponent<TComponent>();
			result.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
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
		internal static void Init<TClient, TArgument>([DisallowNull] this TClient client, TArgument argument) where TClient : class
		{
			var init = Cached<TClient, TArgument>.Init;
			if(init != null)
			{
				init(client, argument);
				return;
			}

			for(var type = client.GetType(); type != null; type = type.BaseType)
			{
				var methodInfo = typeof(TClient).GetMethod(nameof(Init), DeclaredOnlyInstance);
				init = (InitHandler<TClient, TArgument>)Delegate.CreateDelegate(typeof(InitHandler<TClient, TArgument>), null, methodInfo, false);
				if(init == null)
				{
					continue;
				}

				Cached<TClient, TArgument>.Init = init;
				init(client, argument);
				return;
			}

			SetPropertyOrFieldValue(client, argument);
		}

		internal static void Init<TClient, TFirstArgument, TSecondArgument>([DisallowNull] this TClient client, TFirstArgument firstArgument, TSecondArgument secondArgument) where TClient : class
		{
			var init = Cached<TClient, TFirstArgument, TSecondArgument>.Init;
			if(init != null)
			{
				init(client, firstArgument, secondArgument);
				return;
			}

			for(var type = client.GetType(); type != null; type = type.BaseType)
			{
				var methodInfo = typeof(TClient).GetMethod(nameof(Init), DeclaredOnlyInstance);
				init = (InitHandler<TClient, TFirstArgument, TSecondArgument>)Delegate.CreateDelegate(typeof(InitHandler<TClient, TFirstArgument, TSecondArgument>), null, methodInfo, false);
				if(init == null)
				{
					continue;
				}

				Cached<TClient, TFirstArgument, TSecondArgument>.Init = init;
				init(client, firstArgument, secondArgument);
				return;
			}

			SetPropertyOrFieldValue(client, firstArgument);
			SetPropertyOrFieldValue(client, secondArgument);
		}

		internal static void Init<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] this TClient client, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TClient : class
		{
			var init = Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument>.Init;
			if(init != null)
			{
				init(client, firstArgument, secondArgument, thirdArgument);
				return;
			}

			for(var type = client.GetType(); type != null; type = type.BaseType)
			{
				var methodInfo = typeof(TClient).GetMethod(nameof(Init), DeclaredOnlyInstance);
				init = (InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument>)Delegate.CreateDelegate(typeof(InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument>), null, methodInfo, false);
				if(init == null)
				{
					continue;
				}

				Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument>.Init = init;
				init(client, firstArgument, secondArgument, thirdArgument);
				return;
			}

			SetPropertyOrFieldValue(client, firstArgument);
			SetPropertyOrFieldValue(client, secondArgument);
			SetPropertyOrFieldValue(client, thirdArgument);
		}

		internal static void Init<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] this TClient client, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TClient : class
		{
			var init = Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.Init;
			if(init != null)
			{
				init(client, firstArgument, secondArgument, thirdArgument, fourthArgument);
				return;
			}

			for(var type = client.GetType(); type != null; type = type.BaseType)
			{
				var methodInfo = typeof(TClient).GetMethod(nameof(Init), DeclaredOnlyInstance);
				init = (InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>)Delegate.CreateDelegate(typeof(InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>), null, methodInfo, false);
				if(init == null)
				{
					continue;
				}

				Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.Init = init;
				init(client, firstArgument, secondArgument, thirdArgument, fourthArgument);
				return;
			}

			SetPropertyOrFieldValue(client, firstArgument);
			SetPropertyOrFieldValue(client, secondArgument);
			SetPropertyOrFieldValue(client, thirdArgument);
			SetPropertyOrFieldValue(client, fourthArgument);
		}

		internal static void Init<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] this TClient client, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TClient : class
		{
			var init = Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.Init;
			if(init != null)
			{
				init(client, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return;
			}

			for(var type = client.GetType(); type != null; type = type.BaseType)
			{
				var methodInfo = typeof(TClient).GetMethod(nameof(Init), DeclaredOnlyInstance);
				init = (InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>)Delegate.CreateDelegate(typeof(InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>), null, methodInfo, false);
				if(init == null)
				{
					continue;
				}

				Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.Init = init;
				init(client, firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return;
			}

			SetPropertyOrFieldValue(client, firstArgument);
			SetPropertyOrFieldValue(client, secondArgument);
			SetPropertyOrFieldValue(client, thirdArgument);
			SetPropertyOrFieldValue(client, fourthArgument);
			SetPropertyOrFieldValue(client, fifthArgument);
		}

		/// <summary>
		/// Sets the value of a field or property with a certain name found on the target.
		/// </summary>
		/// <typeparam name="TClient"> Type of the object that contains the field or property. </typeparam>
		/// <typeparam name="TValue"> Type of the field or property. </typeparam>
		/// <param name="client"> Object that contains the field or property whose value is set. </param>
		/// <param name="value"> Value to set for field or property. </param>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property of type <typeparamref name="TValue"/> was found on the <paramref name="client"/> class <typeparamref name="TClient"/>.
		/// </exception>
		private static void SetPropertyOrFieldValue<TClient, TValue>(TClient client, TValue value)
		{
			switch(TryFindFieldOrProperty<TValue>(typeof(TClient), out FieldInfo field, out PropertyInfo property))
			{
				case FoundMember.Field:
					field.SetValue(client, value);
					return;
				case FoundMember.Property:
					property.SetValue(client, value, null);
					return;
				default:
					throw new MissingMemberException($"No field or property of type {typeof(TValue).Name} was found on class {typeof(TClient).Name}.");
			}
		}

		private static FoundMember TryFindFieldOrProperty<TValue>(Type componentType, out FieldInfo field, out PropertyInfo property)
		{
			for(var t = componentType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				var fields = t.GetFields(DeclaredOnlyInstance);
				for(int i = 0, count = fields.Length; i < count; i++)
				{
					field = fields[i];
					if(field.FieldType == typeof(TValue))
					{
						property = null;
						return FoundMember.Field;
					}
				}

				var properties = t.GetProperties(DeclaredOnlyInstance);
				for(int i = 0, count = properties.Length; i < count; i++)
				{
					property = properties[i];
					if(property.PropertyType == typeof(TValue))
					{
						field = null;
						return FoundMember.Property;
					}
				}
			}

			for(var t = componentType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				var fields = t.GetFields(DeclaredOnlyInstance);
				for(int i = 0, count = fields.Length; i < count; i++)
				{
					field = fields[i];
					if(field.FieldType.IsAssignableFrom(typeof(TValue)))
					{
						property = null;
						return FoundMember.Field;
					}
				}

				var properties = t.GetProperties(DeclaredOnlyInstance);
				for(int i = 0, count = properties.Length; i < count; i++)
				{
					property = properties[i];
					if(property.PropertyType.IsAssignableFrom(typeof(TValue)))
					{
						field = null;
						return FoundMember.Property;
					}
				}
			}

			field = null;
			property = null;
			return FoundMember.None;
		}

		private enum FoundMember { None, Field, Property }

		private static class Cached<TClient, TArgument> where TClient : class
		{
			public static InitHandler<TClient, TArgument> Init;
		}

		private static class Cached<TClient, TFirstArgument, TSecondArgument> where TClient : class
		{
			public static InitHandler<TClient, TFirstArgument, TSecondArgument> Init;
		}

		private static class Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument> where TClient : class
		{
			public static InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument> Init;
		}

		private static class Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> where TClient : class
		{
			public static InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> Init;
		}

		private static class Cached<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> where TClient : class
		{
			public static InitHandler<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> Init;
		}
	}
}