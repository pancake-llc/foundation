using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal static class ExceptionExtensions
    {
#if DEV_MODE || DEBUG
        private static readonly HashSet<ServiceInfo> exceptionsLogged = new();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        internal static void LogToConsole([DisallowNull] this Exception exception)
        {
            if(exception is AggregateException aggregateException)
            {
                LogToConsole(aggregateException);
                return;
            }


#if DEV_MODE || DEBUG
            // Avoid same exception being logged twice for the same service.
            if(exception is ServiceInitFailedException serviceInitFailedException && exceptionsLogged.Add(serviceInitFailedException.ServiceInfo))
            {
                return;
            }
#endif

            Debug.LogException(exception, exception is InitArgsException initArgsException ? initArgsException.Context : null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if UNITY_6000_0_OR_NEWER
        [HideInCallstack]
#endif
        internal static void LogToConsole([DisallowNull] this AggregateException aggregateException)
        {
            foreach(var innerException in aggregateException.InnerExceptions)
            {
                LogToConsole(innerException);
            }
        }
    }
}