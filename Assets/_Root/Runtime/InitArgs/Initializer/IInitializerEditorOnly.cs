#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("pancake@initargs.editor")]
[assembly: InternalsVisibleTo("Tests.EditMode")]

namespace Pancake.Init
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