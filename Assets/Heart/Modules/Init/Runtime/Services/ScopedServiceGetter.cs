using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	internal sealed class ScopedServiceGetter
	{
		public delegate bool TryGetForHandler<TService>(object client, out TService service);

		private static readonly MethodInfo methodDefinition;
		private static readonly object[] arguments = new object[2];
		private readonly Delegate @delegate;

		static ScopedServiceGetter()
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
			methodDefinition = typeof(Service).GetMember(nameof(Service.TryGetFor), flags).First(m => ((MethodInfo)m).GetParameters()[0].ParameterType == typeof(object)) as MethodInfo;

			if(methodDefinition is null)
			{
				Debug.LogWarning("MethodInfo Service.TryGetFor<TService>(object client, out TService service) not found.");
				methodDefinition = typeof(ScopedServiceGetter).GetMethod(nameof(AlwaysFalse), BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		public ScopedServiceGetter([DisallowNull] Type definingType)
		{
			var method = methodDefinition.MakeGenericMethod(definingType);
			var delegateType = typeof(TryGetForHandler<>).MakeGenericType(definingType);
			@delegate = Delegate.CreateDelegate(delegateType, method);
		}

		public bool TryGetFor(object client, out object service)
		{
			arguments[0] = client;
			arguments[1] = null;

			try
			{
				bool result = (bool)@delegate.DynamicInvoke(arguments);
				service = arguments[1];
				return result;
			}
			catch(TargetInvocationException ex)
			{
				Debug.LogWarning(ex);
				service = null;
				return false;
			}
		}

		private static bool AlwaysFalse<TService>(object client, out TService service)
		{
			if(client is Object unityObject)
			{
				Debug.LogWarning($"Unable to get service {typeof(TService).Name} for client \"{unityObject.name}\".", unityObject);
			}
			else
			{
				Debug.LogWarning($"Unable to get service {typeof(TService).Name} for client {(client is null ? "null" : client.GetType().Name)}.");
			}

			service = default;
			return false;
		}
	}
}