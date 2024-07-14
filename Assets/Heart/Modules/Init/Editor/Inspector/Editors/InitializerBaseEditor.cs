using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	[CanEditMultipleObjects]
    internal sealed class InitializerBaseEditor : InitializerEditor
    {
		protected override bool HasUserDefinedInitArgumentFields => true;
	}
}