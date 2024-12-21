using System;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init.EditorOnly.Internal
{
    [Flags]
    internal enum ExecutionOptions
    {
        Default = _0,

        /// <summary>
        /// If same delegate already exists in the queue, should it be added again?
        /// </summary>
        AllowDuplicates = _1,

        /// <summary>
        /// If this flag is set to true, and the current event matches the desired event (either Repaint or Layout),
        /// then the action will be executed immediately; otherwise it will be queued and only executed on the next
        /// event of the desired type.
        /// </summary>
        CanBeExecutedImmediately = _2,

        /// <summary>
        /// Should GUIUtility.ExitGUI() be called after executing the action?
        /// </summary>
        ExitGUI = _3,

        /// <summary>
        /// Should GUIUtility.ExitGUI() be called immediately if the current event is not of the desired type?
        /// </summary>
        ExitGUIIfWrongEvent = _4
    }
}