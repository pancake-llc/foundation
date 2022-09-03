#if UNITY_EDITOR

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