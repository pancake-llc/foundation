using System.Runtime.CompilerServices;

namespace Pancake
{
    /// <summary>
    /// Hold some const and static member
    /// </summary>
    internal static class Global
    {
        internal const string DEFAULT_SCRIPT_GEN_PATH = "Assets/_Root/Scripts";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ValidateScriptGenPath()
        {
            if (!DEFAULT_SCRIPT_GEN_PATH.DirectoryExists()) DEFAULT_SCRIPT_GEN_PATH.CreateDirectory();
        }
    }
}