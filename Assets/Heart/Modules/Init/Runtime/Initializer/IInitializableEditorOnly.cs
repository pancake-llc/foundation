#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;

namespace Sisus.Init.EditorOnly
{
	public interface IInitializableEditorOnly
	{
		[MaybeNull]
		IInitializer Initializer { get; set; }

		bool CanInitSelfWhenInactive { get; }
	}
}
#endif