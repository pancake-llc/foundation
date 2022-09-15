#if UNITY_EDITOR
using Pancake.Init;

namespace Pancake.Editor.Init
{
    internal interface IInitializerEditorOnly : IInitializer
    {
        NullArgumentGuard NullArgumentGuard { get; set; }
        string NullGuardFailedMessage { get; set; }
        bool HasNullArguments { get; }
        bool MultipleInitializersPerTargetAllowed { get; }
    }
}
#endif