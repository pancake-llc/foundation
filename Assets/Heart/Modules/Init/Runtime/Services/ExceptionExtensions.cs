using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sisus.Init.Internal
{
    internal static class ExceptionExtensions
    {
#if DEV_MODE || DEBUG
        private static readonly HashSet<GlobalServiceInfo> exceptionsLogged = new();
#endif

        internal static void LogToConsole(this Exception exception)
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

        internal static void LogToConsole(this AggregateException aggregateException)
        {
            foreach(var innerException in aggregateException.InnerExceptions)
            {
                LogToConsole(innerException);
            }
        }
    }
}