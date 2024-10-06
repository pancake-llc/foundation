using System;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
    /// <summary>
    /// Base class for all custom exceptions that can be thrown by Init(args)'s API.
    /// </summary>
    public abstract class InitArgsException : Exception
    {
        public Object Context { get; }

        private protected InitArgsException(string message) : this(message, null, null) { }
        private protected InitArgsException(string message, Object context) : this(message, null, context) { }
        private protected InitArgsException(string message, Exception innerException, Object context = null) : base(message, innerException)
        {
            Context = context;
        }

        public void LogAsError() => UnityEngine.Debug.LogException(this, Context);
    }
}