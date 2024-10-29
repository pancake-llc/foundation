using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal sealed class ScopedServiceRemover
    {
        public delegate void RemoveFromHandler<TService>(Clients clients, TService service, Component serviceProvider);

        private static readonly MethodInfo methodDefinition;
        private static readonly object[] arguments = new object[3];
        private readonly Delegate @delegate;

        static ScopedServiceRemover()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            methodDefinition = typeof(Service).GetMember(nameof(Service.RemoveFrom), flags).Select(x => (MethodInfo)x).FirstOrDefault(m => m.GetParameters().Length == 3);

            if(methodDefinition is null)
            {
                Debug.LogWarning("MethodInfo Service.RemoveFrom<TService>(Clients, TService, Component) not found.");
                methodDefinition = typeof(ScopedServiceRemover).GetMethod(nameof(DoNothing), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public ScopedServiceRemover([DisallowNull] Type definingType)
        {
            var method = methodDefinition.MakeGenericMethod(definingType);
            var delegateType = typeof(RemoveFromHandler<>).MakeGenericType(definingType);
            @delegate = Delegate.CreateDelegate(delegateType, method);
        }

        public void RemoveFrom([DisallowNull] object service, Clients clients, [DisallowNull] Component serviceProvider)
        {
            arguments[0] = clients;
            arguments[1] = service;
            arguments[2] = serviceProvider;
            @delegate.DynamicInvoke(arguments);
        }

        private void DoNothing<TService>(Clients clients, TService service, Component serviceProvider) { }
    }
}