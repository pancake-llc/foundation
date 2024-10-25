using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal sealed class ScopedServiceAdder
    {
        public delegate void AddForHandler<TService>(Clients clients, TService service, Component container);

        private static readonly MethodInfo methodDefinition;
        private static readonly object[] arguments = new object[3];
        private readonly Delegate @delegate;

        static ScopedServiceAdder()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            methodDefinition = typeof(Service).GetMember(nameof(Service.AddFor), flags).Select(x => (MethodInfo)x).FirstOrDefault(m => m.GetParameters().Length == 3);

            if(methodDefinition is null)
            {
                Debug.LogWarning("MethodInfo Service.AddFor<>(object instance, Clients forClients) not found.");
                methodDefinition = typeof(ScopedServiceAdder).GetMethod(nameof(DoNothing), BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Clients), null, typeof(Component) }, null);
            }
        }

        public ScopedServiceAdder([DisallowNull] Type definingType)
        {
            var method = methodDefinition.MakeGenericMethod(definingType);
            var delegateType = typeof(AddForHandler<>).MakeGenericType(definingType);
            @delegate = Delegate.CreateDelegate(delegateType, method);
        }

        public void AddFor(object instance, Clients forClients, Component container)
        {
            arguments[0] = forClients;
            arguments[1] = instance;
            arguments[2] = container;
            @delegate.DynamicInvoke(arguments);
        }

        private static void DoNothing<TService>(Clients clients, TService service, Component container)
            => Debug.LogWarning($"Unable to register service {service.GetType().Name}.", container);
    }
}