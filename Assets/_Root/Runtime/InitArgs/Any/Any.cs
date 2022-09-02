using System;
using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using Pancake.Init.Serialization;
using UnityEngine;
using static Pancake.NullExtensions;
using Component = UnityEngine.Component;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
    /// <summary>
    /// Represents a value of type <typeparamref name="T"/> that can be assigned
    /// through Unity's inspector as well as serialized by its serializer.
    /// <para>
    /// If <typeparamref name="T"/> is a <see cref="ServiceAttribute">service</see> type
    /// then this object represents the <see cref="Service{T}.Instance">shared instance</see> of that service.
    /// </para>
    /// <para>
    /// If a class derives from <see cref="Object"/> and implements <see cref="IValueProvider{T}"/> then
    /// <see cref="Any{T}"/> can wrap an instance of this class and return its <see cref="IValueProvider{T}.Value"/>
    /// when <see cref="Value"/> is called.
    /// </para>
    /// <para>
    /// If a class derives from <see cref="Object"/> and has a <see cref="TypeConverter"/>
    /// which returns <see langword="true"/> for <see cref="TypeConverter.CanConvertTo(T)"/> then
    /// <see cref="Any{T}"/> can wrap an instance of this class and convert its <see cref="Value"/> to <typeparamref name="T"/>
    /// on-the-fly using the type converter.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// Type of the value that this object wraps.
    /// <para>
    /// Can be an interface type, a class type or a value type.
    /// </para>
    /// </typeparam>
    [Serializable]
	public struct Any<T> : IValueProvider<T>
		#if DEBUG
		, ISerializationCallbackReceiver
		#endif
	{
		private static readonly AnyTypeDescriptorContext converterContext = new AnyTypeDescriptorContext(typeof(T));

		[SerializeField]
		private Object reference;

		[SerializeReference]
		private T value;

		/// <summary>
		/// Gets a value indicating whether the current <see cref="Any{T}"/> object
		/// has a non-null underlying type.
		/// <para>
		/// <see langword="true"/> if the current <see cref="Any{T}"/> object has a value;
		/// <see langword="false"/> if the current <see cref="Any{T}"/> object has no value.
		/// </para>
		/// </summary>
		public bool HasValue
        {
			get
            {
				#if UNITY_EDITOR
				if(!ServiceUtility.ServicesAreReady && ServiceUtility.IsServiceDefiningType<T>())
				{
					return true;
				}
				#endif

				return ValueOrDefault != Null;
			}
        }

        /// <summary>
        /// Gets the value of the current <see cref="Any{T}"/> object
        /// if it has been assigned a valid underlying value.
        /// <para>
        /// If <see cref="T"/> is a <see cref="ServiceAttribute">service</see> type then returns
        /// the <see cref="Service{T}.Instance">shared instance</see> of that service.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown if the <see cref="HasValue"/> property is <see langword="false"/>. </exception>
        [NotNull]
        public T Value => ValueOrDefault ?? throw new InvalidOperationException($"{nameof(Any<object>)}<{typeof(T).Name}>.Value must return a non-null value. Use {nameof(ValueOrDefault)} if a null return value is acceptable.");

		/// <inheritdoc/>
		object IValueProvider.Value => Value;

		/// <summary>
		/// Gets the value of the current <see cref="Any{T}"/> object
		/// if it has been assigned a valid underlying value;
		/// otherwise, the default value of <see cref="T"/>.
		/// <para>
		/// If <see cref="T"/> is a <see cref="ServiceAttribute">service</see> type then returns
		/// the <see cref="Service{T}.Instance">shared instance</see> of that service.
		/// </para>
		/// </summary>
		[CanBeNull]
		public T ValueOrDefault
		{
			get
			{
				// Using GetHashCode() != 0 instead of Object != null because the latter is not thread safe and can result in an InvalidOperationException.
				if(!(reference is null) && reference.GetHashCode() != 0)
				{
					if(reference is IValueProvider<T> referenceValueProvider)
					{
						#if DEV_MODE
						Debug.Assert(reference != null, $"Any<{typeof(T).Name}>"); // NOTE: This is a risky check to do because it's not a thread safe operation.
						#endif

						return referenceValueProvider.Value;
					}

					if(reference is T referenceValue)
					{
						#if DEV_MODE && DEBUG_GET_VALUE
						if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
						{
							Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
						}
						#endif

						return referenceValue;
					}

					var converter = TypeDescriptor.GetConverter(reference.GetType());
					converterContext.Instance = reference;
					if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
					{
						#if DEV_MODE && DEBUG_GET_VALUE
						if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
						{
							Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
						}
						#endif

						return (T)converter.ConvertTo(reference, typeof(T));
					}

					#if DEV_MODE && DEBUG_GET_VALUE
					if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
					}
					else
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.name} could not be converted to {typeof(T).Name}.", reference);
					}
					#endif
				}

				if(Services.TryGetForAnyClient(out T service))
                {
					return service;
                }
				
                return value is IValueProvider<T> valueProvider ? valueProvider.Value : value;
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="Any"/> struct with the given underlying value.
		/// </summary>
		/// <param name="value"> The underlying value of the <see cref="Any{T}"/> object. </param>
		public Any(T value)
		{
			if(value is Object reference)
            {
				this.value = default;
				this.reference = reference;
				return;
            }

			this.value = value;
			this.reference = default;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Any"/> struct with the given underlying value.
		/// </summary>
		/// <param name="value">
		/// The underlying value of the <see cref="Any{T}"/> object.
		/// </param>
		/// <exception cref="InvalidCastException">
		/// Thrown if <paramref name="value"/> could not be cast to <see cref="T"/> nor
		/// converted to it via a <see cref="TypeConverter"/>
		/// or an <see cref="IValueProvider{T}"/> implementation.
		/// </exception>
		public Any(Object value)
		{
			AssertConstructorArgumentIsValid(value);

			this.value = default;
			reference = value;
		}

        /// <summary>
        /// Creates a new instance of the <see cref="Any"/> struct with the given underlying value.
        /// </summary>
        /// <param name="value"> The underlying value of the <see cref="Any{T}"/> object. </param>
        /// <returns> A new instance of the <see cref="Any{T}"/> struct. </returns>
        public static Any<T> FromValue(T value) => new Any<T>(value);

        /// <summary>
        /// Creates a new instance of the <see cref="Any"/> struct with the given underlying value.
        /// </summary>
        /// <param name="value">
        /// The underlying value of the <see cref="Any{T}"/> object.
        /// </param>
        /// <returns> A new instance of the <see cref="Any{T}"/> struct. </returns>
        public static Any<T> FromObject([CanBeNull] Object value) => new Any<T>(value);

        /// <summary>
        /// Returns a value indicating whether or not it is possible to create an instance of the
        /// <see cref="Any"/> struct with the underlying value of <paramref name="reference"/>.
        /// </summary>
        /// <param name="reference"> The underlying value of the <see cref="Any{T}"/> object. </param>
        /// <returns> A new instance of the <see cref="Any{T}"/> struct. </returns>
        public static bool IsCreatableFrom([CanBeNull] Object reference)
        {
			if(reference == null)
            {
				return typeof(T).IsValueType;
            }

			if(reference is T)
			{
				return true;
			}

			if(reference is IValueProvider<T>)
            {
				return true;
            }

			var converter = TypeDescriptor.GetConverter(reference.GetType());
			converterContext.Instance = reference;
			if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
			{
				return true;
			}

			return false;
        }

		/// <summary>
		/// Defines an implicit conversion of a <see cref="Any{T}"/> to the <see cref="T"/> object that it wraps.
		/// </summary>
		/// <param name="instance"> The <see cref="Any{T}"/> instance to convert. </param>
		public static implicit operator T(Any<T> instance) => instance.ValueOrDefault;

		/// <summary>
		/// Defines an explicit conversion of a <see cref="T"/> to an <see cref="Any{T}"/> object.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		public static implicit operator Any<T>(T value) => new Any<T>(value);

		/// <summary>
		/// Gets the value of the current <see cref="Any{T}"/> object if it has one.
		/// <para>
		/// If the underlying value is <see langword="null"/> and <see cref="T"/> is a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> which is accessible to the <paramref name="client"/> then gets the service.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> Type of the client requesting the value. </typeparam>
		/// <param name="client"> The client requesting the value. </param>
		/// <param name="context"> The context from which the request is being made. </param>
		/// <returns>
		/// The value associated with the current <see cref="Any{T}"/> object, if it has a value;
		/// otherwise, the default value of <see cref="T"/>.
		/// </returns>
		public T GetValue<TClient>([NotNull] TClient client, Context context = Context.MainThread)
        {
			T result = GetValueOrDefault(client, context);

			if(context == Context.MainThread)
			{
				if(result != Null)
				{
					return result;
				}
			}
			else if(result != null)
			{
				return result;
			}

			throw new InvalidOperationException($"{nameof(Any<object>)}<{typeof(T).Name}>.GetValue({(client is null ? "null" : client.GetType().Name)}, {context}) must return a non-null value. Use {nameof(ValueOrDefault)} if a null return value is acceptable.");
        }

		/// <summary>
		/// Gets the value of the current <see cref="Any{T}"/> object if it has on.
		/// <para>
		/// If the underlying value is <see langword="null"/> and <see cref="T"/> is a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> which is accessible to the <paramref name="client"/> then gets the service.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> Type of the client requesting the value. </typeparam>
		/// <param name="client"> The client requesting the value. </param>
		/// <param name="context"> The context from which the request is being made. </param>
		/// <returns>
		/// The value associated with the current <see cref="Any{T}"/> object, if it has a value;
		/// otherwise, the default value of <see cref="T"/>.
		/// </returns>
		public T GetValueOrDefault<TClient>([NotNull] TClient client, Context context = Context.MainThread)
        {
			if(context == Context.Threaded)
			{
				return GetValueOrDefaultThreadSafe();
			}

			if(reference != null)
			{
				if(reference is IValueProvider<T> referenceValueProvider)
				{
					return referenceValueProvider.Value;
				}

				if(reference is T referenceValue)
				{
					#if DEV_MODE && DEBUG_GET_VALUE
					if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
					}
					#endif

					return referenceValue;
				}

				var converter = TypeDescriptor.GetConverter(reference.GetType());
				converterContext.Instance = reference;
				if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
				{
					#if DEV_MODE && DEBUG_GET_VALUE
					if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
					}
					#endif

					return (T)converter.ConvertTo(reference, typeof(T));
				}

				#if DEV_MODE && DEBUG_GET_VALUE
				if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
				{
					Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
				}
				else
				{
					Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.name} could not be converted to {typeof(T).Name}.", reference);
				}
				#endif
			}

			if(Service.TryGet(client, out T service))
			{
				return service;
			}
				
			return value is IValueProvider<T> valueProvider ? valueProvider.Value : value;
		}

		private T GetValueOrDefaultThreadSafe()
		{
			// Using is null and GetHashCode() instead of != null because != null is not thread safe and can actually result in an InvalidOperationException!
			if(!(reference is null) && reference.GetHashCode() != 0)
			{
				if(reference is IValueProvider<T> referenceValueProvider)
				{
					#if DEV_MODE
					Debug.Assert(reference != null, $"Any<{typeof(T).Name}>"); // NOTE: This is a risky check to do because it's not a thread safe operation.
					#endif

					return referenceValueProvider.Value;
				}

				if(reference is T referenceValue)
				{
					#if DEV_MODE && DEBUG_GET_VALUE
					if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
					}
					#endif

					return referenceValue;
				}

				var converter = TypeDescriptor.GetConverter(reference.GetType());
				converterContext.Instance = reference;
				if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
				{
					#if DEV_MODE && DEBUG_GET_VALUE
					if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
					{
						Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
					}
					#endif

					return (T)converter.ConvertTo(reference, typeof(T));
				}

				#if DEV_MODE && DEBUG_GET_VALUE
				if(reference == null) // NOTE: This is a risky check to do because it's not a thread safe operation.
				{
					Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.GetHashCode()} was actually null. This could be a 'Missing' reference pointing to a Object that has been destroyed.");
				}
				else
				{
					Debug.LogWarning($"Any<{typeof(T).Name}>.ValueOrDefault Object reference with hash {reference.name} could not be converted to {typeof(T).Name}.", reference);
				}
				#endif
			}

			if(Services.TryGetForAnyClient(out T service))
			{
				return service;
			}
				
			return value is IValueProvider<T> valueProvider ? valueProvider.Value : value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="Any{T}"/> object has a non-null value
		/// when retrieved by the <paramref name="client"/> in question in the given <paramref name="context"/>.
		/// </summary>
		/// <typeparam name="TClient"> Type of the client checking the value. </typeparam>
		/// <param name="client"> The client checking the value. </param>
		/// <param name="context">
		/// The context from which the checking is being made.
		/// <para>
		/// Some services might not be accessible in a <see cref="Context.Threaded"/> context.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the <see cref="Any{T}"/> object has a non-null value.
		/// <para>
		/// <see langword="true"/> if <see cref="T"/> is the defining type of a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> that is accessible to the <paramref name="client"/>
		/// in the current <paramref name="context"/>.
		/// </para>
		/// <para>
		/// Otherwise, <see langword="false"/>.
		/// </para>
		/// </returns>
		public bool GetHasValue<TClient>(TClient client, Context context = Context.MainThread)
        {
			#if UNITY_EDITOR
			if(reference is CrossSceneReference)
			{
				return true;
			}

			bool isPlaying = context  == Context.MainThread ? Application.isPlaying : EditorOnly.ThreadSafe.Application.IsPlaying;
			if(!isPlaying && ServiceUtility.IsServiceDefiningType<T>())
            {
				return true;
            }
			#endif

			var valueOrDefault = GetValueOrDefault(client, context);

			if(context == Context.MainThread)
			{
				return valueOrDefault != Null;
			}

			// Use GetHashCode() != 0 instead of Object != null because the latter is not thread safe and can result in an InvalidOperationException.
			return valueOrDefault != null && (!(valueOrDefault is Object obj) || obj.GetHashCode() != 0);
        }

		/// <summary>
		/// Gets the current value of the <see cref="Any{T}"/> object if it has one.
		/// </summary>
		/// <param name="value">
		/// When this method returns, contains the current value of the <see cref="Any{T}"/> object, if it has been assigned one.
		/// <para>
		/// If <see cref="T"/> is the defining type of a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> that is accessible to all clients, then contains
		/// the shared instance of that service.
		/// </para>
		/// Otherwise, contains the default value of <see cref="T"/>.
		/// <para>
		/// This parameter is passed uninitialized.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the <see cref="Any{T}"/> object has a non-null value.
		/// <para>
		/// <see langword="true"/> if <see cref="T"/> is the defining type of a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> that is accessible to all clients.
		/// </para>
		/// <para>
		/// Otherwise, <see langword="false"/>.
		/// </para>
		public bool TryGetValue(out T value)
        {
			value = ValueOrDefault;
			return !(value is null);
        }

		/// <summary>
		/// Gets the value of the current <see cref="Any{T}"/> object if it has one.
		/// <para>
		/// If the underlying value is <see langword="null"/> and <see cref="T"/> is a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> which is accessible to the <paramref name="client"/> then gets the service.
		/// </para>
		/// </summary>
		/// <param name="value">
		/// When this method returns, contains the value associated with the current <see cref="Any{T}"/> object, if it has a value;
		/// otherwise, the default value of <see cref="T"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if a valid underlying value has been assigned; otherwise, <see langword="false"/>. </returns>
		public bool TryGetValue<TClient>(TClient client, Context context, out T value)
        {
			value = GetValueOrDefault(client, context);

			if(context == Context.MainThread)
			{
				return value != Null;
			}

			if(value is null)
			{
				return false;
			}

			// GetHashCode() instead of != null because != null is not thread safe and can actually result in an InvalidOperationException!
			return value is Object reference && reference.GetHashCode() != 0;
        }

		/// <summary>
		/// Gets the value of the current <see cref="Any{T}"/> object if it has one.
		/// <para>
		/// If the underlying value is <see langword="null"/> and <see cref="T"/> is a <see cref="ServiceAttribute">global service</see>
		/// or a <see cref="Services">scene service</see> which is accessible to the <paramref name="client"/> then gets the service.
		/// </para>
		/// <para>
		/// This method can only be called from the main thread.
		/// </para>
		/// </summary>
		/// <param name="value">
		/// When this method returns, contains the value associated with the current <see cref="Any{T}"/> object, if it has a value;
		/// otherwise, the default value of <see cref="T"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if a valid underlying value has been assigned; otherwise, <see langword="false"/>. </returns>
		public bool TryGetValue<TClient>(TClient client, out T value)
        {
			value = GetValueOrDefault(client, Context.MainThread);
			return value != Null;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object. </param>
        /// <returns> <see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>. </returns>
        public override bool Equals(object other)
		{
			if(other is null)
			{
				return value is null && reference == null && !ServiceUtility.IsServiceDefiningType<T>();
			}

			if(other is Any<T> any)
            {
				return Equals(any.ValueOrDefault);
            }

            return other.Equals(ValueOrDefault);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the value of the current object.
		/// </summary>
		/// <param name="other"> The object to compare with the current object's value. </param>
		/// <returns> <see langword="true"/> if the specified object is equal to the current object's value; otherwise, <see langword="false"/>. </returns>
		public bool Equals(T other)
		{
			if(other is null)
            {
				return value is null && reference == null && !ServiceUtility.IsServiceDefiningType<T>();
            }

			return other.Equals(ValueOrDefault);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the value of the current object.
		/// </summary>
		/// <param name="other"> The object to compare with the current object's value. </param>
		/// <returns> <see langword="true"/> if the specified object is equal to the current object's value; otherwise, <see langword="false"/>. </returns>
		public bool Equals(Object other, Context context = Context.MainThread)
		{
			if(other == null)
            {
				return value is null && reference == null && !ServiceUtility.IsServiceDefiningType<T>();
			}

			if(reference == other)
			{
				return true;
			}

			if(reference != null)
			{
				return false;
			}

			return Service.TryGet(context, out T service) && service as Object == other;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other"> The object to compare with the current object. </param>
		/// <returns> <see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>. </returns>
		public bool Equals(Any<T> other) => Equals(other.value);

		/// <summary>
		/// Returns a string that represents the underlying object.
		/// </summary>
		/// <returns> A string that represents the underlying object, if it has been assigned; otherwise, and empty string. </returns>
		public override string ToString() => ValueOrDefault is T value ? value.ToString() : "";

        /// <summary>
        /// Returns the hash code for this <see cref="Any{T}"/> object.
        /// </summary>
        /// <returns> Hash code of the underlying object, if it has been assigned; otherwise, 0. </returns>
        public override int GetHashCode() => ValueOrDefault is T value ? value.GetHashCode() : 0;

		[Conditional("DEBUG")]
		private static void AssertConstructorArgumentIsValid(Object reference)
        {
			#if DEBUG
			if(reference == null)
            {
				if(typeof(T).IsValueType)
                {
					throw new InvalidCastException($"Any<{typeof(T).Name}> can not have a null value because {typeof(T).Name} is a value type.");
                }
				return;
            }

			if(reference is T)
			{
				return;
			}

			if(reference is IValueProvider<T>)
            {
				return;
            }

			var converter = TypeDescriptor.GetConverter(reference.GetType());
			converterContext.Instance = reference;
			if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
			{
				return;
			}

			throw new InvalidCastException($"Any<{typeof(T).Name}> can not have a value of type {reference.GetType().Name}.");
			#endif
		}

		[Conditional("DEBUG")]
		private static void AssertSerializedReferenceIsValid(Object reference)
        {
			#if DEBUG
			if(reference == null || reference is T || reference is IValueProvider<T>)
            {
				return;
            }

			var converter = TypeDescriptor.GetConverter(reference.GetType());
			converterContext.Instance = reference;
			if(converter != null && converter.CanConvertTo(converterContext, typeof(T)))
			{
				return;
			}

			throw new InvalidCastException($"Any<{typeof(T).Name}> can not have a value of type {reference.GetType().Name}.");
			#endif
		}

		#if DEBUG
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if(typeof(T) == typeof(object) && !(value is null))
            {
				if(value is int intValue)
                {
					value = (T)(object)new _Integer() { value = intValue };
				}
				else if(value is bool boolValue)
				{
					value = (T)(object)new _Boolean() { value = boolValue };
				}
				else if(value is float floatValue)
				{
					value = (T)(object)new _Float() { value = floatValue };
				}
				else if(value is string stringValue)
				{
					value = (T)(object)new _String() { value = stringValue };
				}
				else if(value is double doubleValue)
				{
					value = (T)(object)new _Double() { value = doubleValue };
				}
				else if(value is Type typeValue)
				{
					value = (T)(object)new _Type(typeValue, null);
				}
			}

			if(value is Object)
			{
				value = default;
			}

			if(reference is CrossSceneReference crossSceneReference && !crossSceneReference.isCrossScene)
			{
				var crossSceneReferenceValue = crossSceneReference.Value;
				if(crossSceneReferenceValue != null)
				{
					#if DEV_MODE
					Debug.Log($"CrossSceneReference {crossSceneReferenceValue.name} ({crossSceneReferenceValue.GetType().Name}) isCrossScene was false. Changing into a direct reference.");
					#endif

					if(crossSceneReference.Value is T directReference)
					{
						value = directReference;
					}
					#if DEV_MODE
					else
					{
						Debug.LogWarning($"CrossSceneReference {crossSceneReferenceValue.name} of type {crossSceneReferenceValue.GetType().Name} not convertible to {typeof(T).Name}.");
					}
					#endif
				}
			}

			try
			{
				AssertSerializedReferenceIsValid(reference);
			}
			catch(InvalidCastException e)
            {
				if(reference is GameObject gameObject)
                {
					foreach(var component in gameObject.GetComponents<Component>())
                    {
						if(IsCreatableFrom(component))
                        {
							reference = component;
							return;
                        }
                    }
                }

				Debug.LogWarning(e.Message, reference);
				reference = null;
            }
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize() { }
		#endif

		#if UNITY_EDITOR
		internal bool HasSerializedValue() => reference != null || value != null;
		#endif
	}
}