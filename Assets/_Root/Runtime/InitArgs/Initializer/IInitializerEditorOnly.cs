#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InitArgs.Editor")]
[assembly: InternalsVisibleTo("Tests.EditMode")]

namespace Pancake.Init.EditorOnly
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