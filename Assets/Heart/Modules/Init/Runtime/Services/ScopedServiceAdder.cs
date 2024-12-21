using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Internal
{
	internal sealed class ScopedServiceAdder
	{
		public delegate void ServiceHandler<TService>(Clients clients, TService service, Component registerer);
		public delegate void ValueProviderTHandler<TService>(Clients clients, IValueProvider<TService> serviceProvider, Component registerer);
		public delegate void ValueProviderAsyncTHandler<TService>(Clients clients, IValueProviderAsync<TService> serviceProvider, Component registerer);
		public delegate void ValueByTypeProviderHandler<TService>(Clients clients, IValueByTypeProvider serviceProvider, Component registerer);
		public delegate void ValueByTypeProviderAsyncHandler<TService>(Clients clients, IValueByTypeProviderAsync serviceProvider, Component registerer);
		public delegate void ForValueProviderHandler<TService>(Clients clients, IValueProvider serviceProvider, Component registerer);
		public delegate void ForValueProviderAsyncHandler<TService>(Clients clients, IValueProviderAsync serviceProvider, Component registerer);

		private static readonly MethodInfo forServiceMethodDefinition;
		private static readonly MethodInfo forValueProviderTMethodDefinition;
		private static readonly MethodInfo forValueProviderTAsyncMethodDefinition;
		private static readonly MethodInfo forValueByTypeProviderMethodDefinition;
		private static readonly MethodInfo forValueByTypeProviderAsyncMethodDefinition;
		private static readonly MethodInfo forValueProviderMethodDefinition;
		private static readonly MethodInfo forValueProviderAsyncMethodDefinition;

		private static readonly object[] arguments = new object[3];
		private readonly Delegate @delegate;

		static ScopedServiceAdder()
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

			var methods = typeof(Service).GetMember(nameof(Service.AddFor), flags);

			#if DEV_MODE
			// AddFor<TService>(Clients clients, TService service, Component serviceProvider)
			// and 7 value provider specific ones.
			Debug.Assert(methods.Length == 8, $"Unexpected AddFor method count: {methods.Length}.");
			#endif

			foreach(MethodInfo method in methods)
			{
				var parameters = method.GetParameters();
				if(parameters.Length != 3)
				{
					continue;
				}

				Type parameterType = parameters[1].ParameterType;
				if(parameterType.IsGenericType)
				{
					Type genericTypeDefinition = parameterType.GetGenericTypeDefinition(); // needed?
					if(genericTypeDefinition == typeof(IValueProvider<>))
					{
						forValueProviderTMethodDefinition = method;
						continue;
					}

					if(genericTypeDefinition == typeof(IValueProviderAsync<>))
					{
						forValueProviderTAsyncMethodDefinition = method;
						continue;
					}

					#if DEV_MODE
					Debug.LogError($"AddFor method {method} with unrecognized generic type definition {genericTypeDefinition} detected.");
					#endif
				}
				else if(parameterType == typeof(IValueByTypeProvider))
				{
					forValueByTypeProviderMethodDefinition = method;
				}
				else if(parameterType == typeof(IValueByTypeProviderAsync))
				{
					forValueByTypeProviderAsyncMethodDefinition = method;
				}
				else if(parameterType == typeof(IValueProvider))
				{
					forValueProviderMethodDefinition = method;
				}
				else if(parameterType == typeof(IValueProviderAsync))
				{
					forValueProviderAsyncMethodDefinition = method;
				}
				else
				{
					forServiceMethodDefinition = method;
				}
			}

			#if DEV_MODE || INIT_ARGS_SAFE_MODE
			Debug.Assert(forServiceMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, TService service, Clients forClients) not found.");
			Debug.Assert(forValueProviderTMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueProvider<TService> serviceProvider, Clients forClients) not found.");
			Debug.Assert(forValueProviderTAsyncMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueProviderAsync<TService> serviceProvider, Clients forClients) not found.");
			Debug.Assert(forValueByTypeProviderMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueByTypeProvider serviceProvider, Clients forClients) not found.");
			Debug.Assert(forValueByTypeProviderAsyncMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueByTypeProviderAsync serviceProvider, Clients forClients) not found.");
			Debug.Assert(forValueProviderMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueProvider serviceProvider, Clients forClients) not found.");
			Debug.Assert(forValueProviderAsyncMethodDefinition != null, "MethodInfo Service.AddFor<TService>(object instance, IValueProviderAsync serviceProvider, Clients forClients) not found.");
			#endif
		}

		public ScopedServiceAdder([DisallowNull] Type definingType, ServiceProviderType serviceProviderType)
		{
			Type delegateType;
			MethodInfo methodDefinition;

			switch(serviceProviderType)
			{
				case ServiceProviderType.IValueProviderT:
					delegateType = typeof(ValueProviderTHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueProviderTMethodDefinition;
					break;
				case ServiceProviderType.IValueProviderAsyncT:
					delegateType = typeof(ValueProviderAsyncTHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueProviderTAsyncMethodDefinition;
					break;
				case ServiceProviderType.IValueByTypeProvider:
					delegateType = typeof(ValueByTypeProviderHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueByTypeProviderMethodDefinition;
					break;
				case ServiceProviderType.IValueByTypeProviderAsync:
					delegateType = typeof(ValueByTypeProviderAsyncHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueByTypeProviderAsyncMethodDefinition;
					break;
				case ServiceProviderType.IValueProvider:
					delegateType = typeof(ForValueProviderHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueProviderMethodDefinition;
					break;
				case ServiceProviderType.IValueProviderAsync:
					delegateType = typeof(ForValueProviderAsyncHandler<>).MakeGenericType(definingType);
					methodDefinition = forValueProviderAsyncMethodDefinition;
					break;
				default:
					delegateType = typeof(ServiceHandler<>).MakeGenericType(definingType);
					methodDefinition = forServiceMethodDefinition;

					#if DEV_MODE
					if(serviceProviderType != ServiceProviderType.None) throw new ArgumentOutOfRangeException(nameof(serviceProviderType), serviceProviderType, null);
					#endif

					break;
			}

			var method = methodDefinition.MakeGenericMethod(definingType);
			@delegate = Delegate.CreateDelegate(delegateType, method);
		}

		public void AddFor(object serviceOrProvider, Clients forClients, Component registerer)
		{
			arguments[0] = forClients;
			arguments[1] = serviceOrProvider;
			arguments[2] = registerer;
			@delegate.DynamicInvoke(arguments);
		}

		public record Key
		{
			public readonly Type definingType;
			public readonly ServiceProviderType providerType;

			public Key(Type definingType, ServiceProviderType providerType)
			{
				this.definingType = definingType;
				this.providerType = providerType;
			}
		}
	}
}