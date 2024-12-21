using System;
using System.Text;
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

        internal static void AppendMoreHelpUrl(StringBuilder sb) => sb.Append("\n\nMore Help: docs.sisus.co/init-args/common-problems-solutions/");
    }
}