using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that is responsible for providing an initialization argument, and can be
	/// validated by an initializer to verify that it will be able to fulfill that responsibility at runtime.
	/// </summary>
	public interface INullGuardByType
	{
		/// <summary>
		/// Gets a value indicating whether null guard passes for this object or not, and if not,
		/// what was the cause of the failure.
		/// </summary>
		/// <typeparam name="TValue"> Type of the value whose availability should be checked. </typeparam>
		/// <param name="client">
		/// The component performing the evaluation, if being performed by a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// Value representing the result of the null guard.
		/// </returns>
		NullGuardResult EvaluateNullGuard<TValue>([AllowNull] Component client);

		NullGuardResult EvaluateNullGuard([DisallowNull] Type serviceType, [AllowNull] Component client)
		{
			if(serviceType.ContainsGenericParameters)
			{
				var genericArguments = serviceType.GetGenericArguments();
				if(genericArguments.Length != 1 || !client || genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					return NullGuardResult.ValueProviderValueMissing;
				}

				if(genericArguments[0].GetGenericParameterConstraints().Length > 0)
				{
					#if DEV_MODE
					Debug.Log($"{GetType().Name}.EvaluateNullGuard({Internal.TypeUtility.ToString(serviceType)}) -> returning false because type contains generic parameters. Could try to construct generic type using client's type as the generic argument type, but that's unsafe due to the generic argument type having constraints, which could result in an exception.");
					#endif

					return NullGuardResult.ValueProviderValueMissing;
				}

				return (NullGuardResult)canProvideValueGeneric.MakeGenericMethod(serviceType.MakeGenericType(client.GetType())).Invoke(this, WithArgument(client));
			}

			return (NullGuardResult)canProvideValueGeneric.MakeGenericMethod(serviceType).Invoke(this, WithArgument(client));
		}

		private static object[] WithArgument(object argument)
		{
			singleArgument[0] = argument;
			return singleArgument;
		}

		private static readonly object[] singleArgument = new object[1];
		private static readonly MethodInfo canProvideValueGeneric =   typeof(INullGuardByType).GetMethod("EvaluateNullGuard", new[] { typeof(Component) });
	}
}