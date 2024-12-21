using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that can provide a value of a requested type to a <see cref="GameObject"/> client.
	/// <para>
	/// If a class derives from <see cref="Object"/> and implements <see cref="IValueByTypeProviderAsync"/> then
	/// <see cref="Any{T}"/> can wrap an instance of this class and return its value when <see cref="Any{T}.GetValue{TClient}"/> is called.
	/// </para>
	/// </summary>
	/// <seealso cref="IValueProvider{TValue}"/>
	/// <seealso cref="IValueByTypeProvider"/>
	/// <seealso cref="IValueByTypeProviderAsync"/>
	[RequireImplementors]
	public interface IValueByTypeProvider
	{
		/// <summary>
		/// Gets the value of type <typeparamref name="TValue"/> for the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TValue"> Type of the requested <paramref name="value"/>. </typeparam>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <param name="value">
		/// When this method returns, contains the value of type <typeparamref name="TValue"/>, if available; otherwise, the default value of <typeparamref name="TValue"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <see langword="true"/> if was able to retrieve the value; otherwise, <see langword="false"/>.
		bool TryGetFor<TValue>([AllowNull] Component client, [NotNullWhen(true), MaybeNullWhen(false)] out TValue value);

		/// <summary>
		/// Gets the value of the given type for the <paramref name="client"/>.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <param name="valueType"> Type of the requested value. </param>
		/// <param name="value">
		/// When this method returns, contains the value of type <see paramref="valueType"/>, if available;
		/// otherwise, <see langword="null"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <see langword="true"/> if was able to retrieve the value; otherwise, <see langword="false"/>.
		bool TryGetFor([AllowNull] Component client, [DisallowNull] Type valueType, out object value)
		{
			if(valueType.ContainsGenericParameters)
			{
				var genericArguments = valueType.GetGenericArguments();
				if(genericArguments.Length != 1 || !client || genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					value = null;
					return false;
				}

				if(genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					#if DEV_MODE
					Debug.Log($"{GetType().Name}.CanProvideValue({Internal.TypeUtility.ToString(valueType)}) -> returning false because type contains generic parameters. Could try to construct generic type using client's type as the generic argument type, but that's unsafe due to the generic argument type having constraints, which could result in an exception.");
					#endif

					value = null;
					return false;
				}

				valueType = valueType.MakeGenericType(client.GetType());
			}

			twoArguments[0] = client;
			if((bool)tryGetForGeneric.MakeGenericMethod(valueType).Invoke(this, WithArgument(client)))
			{
				value = twoArguments[1];
				twoArguments[0] = null;
				twoArguments[1] = null;
				return true;
			}

			value = null;
			twoArguments[0] = null;
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether this value provider can provide a value of type
		/// <typeparamref name="TValue"/> for the <paramref name="client"/> at this time.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if can provide a value for the client at this time; otherwise, <see langword="false"/>.
		/// </returns>
		bool HasValueFor<TValue>(Component client) => TryGetFor<TValue>(client, out _);

		/// <summary>
		/// Gets a value indicating whether this value provider can potentially provide
		/// a value of the given type to the client at runtime.
		/// <para>
		/// Used by the Inspector to determine if the value provider can be assigned to an Init argument field.
		/// </para>
		/// </summary>
		/// <typeparam name="TValue"> Type of the value that would be provided. </typeparam>
		/// <param name="client"> The client component that would receive the value. </param>
		/// <returns>
		/// <see langword="true"/> if can potentially provide a value of the given type to the client
		/// at runtime; otherwise, <see langword="false"/>.
		/// </returns>
		bool CanProvideValue<TValue>([AllowNull] Component client) => TryGetFor<TValue>(client, out _);

		bool CanProvideValue([AllowNull] Component client, [DisallowNull] Type valueType)
		{
			if(valueType.ContainsGenericParameters)
			{
				var genericArguments = valueType.GetGenericArguments();
				if(genericArguments.Length != 1 || !client || genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					return false;
				}

				if(genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					#if DEV_MODE
					Debug.Log($"{GetType().Name}.CanProvideValue({Internal.TypeUtility.ToString(valueType)}) -> returning false because type contains generic parameters. Could try to construct generic type using client's type as the generic argument type, but that's unsafe due to the generic argument type having constraints, which could result in an exception.");
					#endif

					return false;
				}

				valueType = valueType.MakeGenericType(client.GetType());
			}

			return (bool)canProvideValueGeneric.MakeGenericMethod(valueType).Invoke(this, WithArgument(client));
		}

		private static object[] WithArgument(object argument)
		{
			singleArgument[0] = argument;
			return singleArgument;
		}

		private static readonly object[] singleArgument = new object[1];
		private static readonly object[] twoArguments = new object[2];
		private static readonly MethodInfo canProvideValueGeneric =   typeof(IValueByTypeProvider).GetMethod(nameof(CanProvideValue), new[] { typeof(Component) });
		private static readonly MethodInfo tryGetForGeneric = (MethodInfo)typeof(IValueByTypeProvider).GetMember(nameof(TryGetFor))
			.First(member => member is MethodInfo method && method.GetParameters().Length == 2);
	}
}