using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal sealed class GlobalServiceSilentSetter
    {
        private static readonly MethodInfo methodDefinition;
        private static readonly object[] arguments = new object[1];

        private readonly Delegate @delegate;

        static GlobalServiceSilentSetter()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            GlobalServiceSilentSetter.methodDefinition = typeof(Service).GetMethod(nameof(Service.SetSilently), flags);

            if(GlobalServiceSilentSetter.methodDefinition is null)
            {
                Debug.LogWarning("MethodInfo Service.SetInstanceSilently<> not found.");
                GlobalServiceSilentSetter.methodDefinition = typeof(GlobalServiceSilentSetter).GetMethod(nameof(GlobalServiceSilentSetter.DoNothing), BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public GlobalServiceSilentSetter([DisallowNull] Type definingType)
        {
            var method = GlobalServiceSilentSetter.methodDefinition.MakeGenericMethod(definingType);
            var delegateType = typeof(SetInstanceHandler<>).MakeGenericType(definingType);
            @delegate = Delegate.CreateDelegate(delegateType, method);
        }

        public void SetInstanceSilently(object instance)
        {
            try
            {
                GlobalServiceSilentSetter.arguments[0] = instance;
                @delegate.DynamicInvoke(GlobalServiceSilentSetter.arguments);
            }
            catch(TargetInvocationException ex)
            {
                Debug.LogWarning(ex);
            }
        }

        private void DoNothing<TService>(TService instance) { }
    }
}