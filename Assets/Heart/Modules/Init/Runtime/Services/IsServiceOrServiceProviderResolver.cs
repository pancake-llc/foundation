using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	internal sealed class IsServiceOrServiceProviderResolver
	{
		public delegate bool Delegate<TService>(TService test);

		private static readonly MethodInfo methodDefinition;
		private static readonly object[] arguments = new object[1];
		private readonly Delegate @delegate;

		static IsServiceOrServiceProviderResolver()
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			methodDefinition = typeof(Service).GetMethod(nameof(Service.IsServiceOrServiceProvider), flags);

			if(methodDefinition is null)
			{
				Debug.LogWarning($"MethodInfo {nameof(Service)}.{nameof(Service.IsServiceOrServiceProvider)}<TService>(TService test, Component client) not found.");
				methodDefinition = typeof(IsServiceOrServiceProviderResolver).GetMethod(nameof(AlwaysFalse), BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		public IsServiceOrServiceProviderResolver([DisallowNull] Type definingType)
		{
			var method = methodDefinition.MakeGenericMethod(definingType);
			var delegateType = typeof(Delegate<>).MakeGenericType(definingType);
			@delegate = Delegate.CreateDelegate(delegateType, method);
		}

		public bool IsServiceOrServiceProvider(object test)
		{
			arguments[0] = test;

			try
			{
				bool result = (bool)@delegate.DynamicInvoke(arguments);
				return result;
			}
			catch(TargetInvocationException ex)
			{
				Debug.LogWarning($"{nameof(Service)}.{nameof(Service.IsServiceOrServiceProvider)}<TService>(TService test)\n" + ex, test as Object);
				return false;
			}
			catch(ArgumentException ex)
			{
				Debug.LogWarning($"{nameof(Service)}.{nameof(Service.IsServiceOrServiceProvider)}<TService>(TService test)\n" + ex, test as Object);
				return false;
			}
		}

		private static bool AlwaysFalse<TService>(TService test)
		{
			Debug.LogWarning($"Unable to determine if {test} is a service.", test as Object);
			return false;
		}
	}
}