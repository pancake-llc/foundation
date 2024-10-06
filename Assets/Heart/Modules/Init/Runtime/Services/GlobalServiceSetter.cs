using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Internal
{
    public delegate void SetInstanceHandler<in TService>(TService instance);

    internal sealed class GlobalServiceSetter
    {
        private static readonly MethodInfo methodDefinition;
        private static readonly object[] arguments = new object[1];

        private readonly Delegate @delegate;

        static GlobalServiceSetter()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            methodDefinition = typeof(Service).GetMethod(nameof(Service.Set), flags);

            if(methodDefinition is null)
            {
                Debug.LogWarning("MethodInfo Service.SetInstance<> not found.");
                methodDefinition = typeof(GlobalServiceSetter).GetMethod(nameof(DoNothing), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public GlobalServiceSetter([DisallowNull] Type definingType)
        {
            var method = methodDefinition.MakeGenericMethod(definingType);
            var delegateType = typeof(SetInstanceHandler<>).MakeGenericType(definingType);
            @delegate = Delegate.CreateDelegate(delegateType, method);
        }

        public void SetInstance(object instance)
        {
            try
            {
                arguments[0] = instance;
                @delegate.DynamicInvoke(arguments);
            }
            catch(TargetInvocationException ex)
            {
                Debug.LogWarning(ex);
            }
        }

        private void DoNothing<TService>(TService instance) { }
    }
}