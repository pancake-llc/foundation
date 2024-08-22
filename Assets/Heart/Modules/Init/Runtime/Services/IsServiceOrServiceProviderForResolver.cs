using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Internal
{
	internal sealed class IsServiceOrServiceProviderForResolver
	{
		public delegate bool Delegate<TService>(Component client, TService test);

		private static readonly MethodInfo methodDefinition;
		private static readonly object[] arguments = new object[2];
		private readonly Delegate @delegate;

		static IsServiceOrServiceProviderForResolver()
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			methodDefinition = typeof(Service).GetMethod(nameof(Service.IsServiceOrServiceProviderFor), flags);

			if(methodDefinition is null)
			{
				Debug.LogWarning($"MethodInfo {nameof(Service)}.{nameof(Service.IsServiceOrServiceProviderFor)}<TService>(TService test, Component client) not found.");
				methodDefinition = typeof(IsServiceOrServiceProviderForResolver).GetMethod(nameof(AlwaysFalse), BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		public IsServiceOrServiceProviderForResolver([DisallowNull] Type definingType)
		{
			var method = methodDefinition.MakeGenericMethod(definingType);
			var delegateType = typeof(Delegate<>).MakeGenericType(definingType);
			@delegate = Delegate.CreateDelegate(delegateType, method);
		}

		public bool IsServiceOrServiceProviderFor(Component client, object test)
		{
			arguments[0] = client;
			arguments[1] = test;

			try
			{
				bool result = (bool)@delegate.DynamicInvoke(arguments);
				return result;
			}
			catch(TargetInvocationException ex)
			{
				Debug.LogWarning($"{nameof(Service)}.{nameof(Service.IsServiceOrServiceProviderFor)}<TService>(TService test, Component client)\n" + ex, client);
				return false;
			}
			catch(ArgumentException ex)
			{
				Debug.LogWarning($"{nameof(Service)}.{nameof(Service.IsServiceOrServiceProviderFor)}<TService>(TService test, Component client)\n" + ex, client);
				return false;
			}
		}

		private static bool AlwaysFalse<TService>(TService test, Component client)
		{
			Debug.LogWarning($"Unable to determine if {test} is a service for client {client.GetType().Name} on GameObject \"{client.name}\".", client);
			return false;
		}
	}
}