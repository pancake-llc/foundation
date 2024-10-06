using System;

namespace Sisus.Init.EditorOnly.Internal
{
    [Flags]
    internal enum ExecutionOptions
    {
        Default = 0,
        AllowDuplicates = 1,
        ExecuteImmediateIfLayoutEvent = 2,
        ExitGUI = 4,
        ExitGUIIfNotLayoutEvent = 8,
    }
}