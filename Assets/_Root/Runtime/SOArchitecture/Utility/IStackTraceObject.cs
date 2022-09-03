using System.Collections.Generic;

namespace Pancake.SOA
{
    public interface IStackTraceObject
    {
        List<StackTraceEntry> StackTraces { get; }

        void AddStackTrace();
        void AddStackTrace(object value);
    } 
}