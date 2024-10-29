using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal sealed class ScopedServicesFromProviderRemover
    {
        public delegate void RemoveFromHandler<TService>(Clients clients, Component container);

        private static readonly MethodInfo methodDefinition;
        private static readonly object[] arguments = new object[2];
        private readonly Delegate @delegate;

        static ScopedServicesFromProviderRemover()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            methodDefinition = typeof(Service).GetMember(nameof(Service.RemoveFrom), flags).Select(x => (MethodInfo)x).FirstOrDefault(m => m.GetParameters()[1].ParameterType == typeof(Component));

            if(methodDefinition is null)
            {
                Debug.LogWarning("MethodInfo Service.RemoveFrom<TService>(Clients, Component) not found.");
                methodDefinition = typeof(ScopedServicesFromProviderRemover).GetMethod(nameof(DoNothing), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public ScopedServicesFromProviderRemover([DisallowNull] Type definingType)
        {
            var method = methodDefinition.MakeGenericMethod(definingType);
            var delegateType = typeof(RemoveFromHandler<>).MakeGenericType(definingType);
            @delegate = Delegate.CreateDelegate(delegateType, method);
        }

        public void RemoveFrom(Clients clients, [DisallowNull] Component container)
        {
            arguments[0] = clients;
            arguments[1] = container;
            @delegate.DynamicInvoke(arguments);
        }

        private void DoNothing<TService>(Clients clients, Component serviceProvider) { }
    }
}