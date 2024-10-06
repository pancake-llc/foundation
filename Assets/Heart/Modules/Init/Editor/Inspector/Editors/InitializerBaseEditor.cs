using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	[CustomEditor(typeof(InitializerBase<,>)), CanEditMultipleObjects]
	internal sealed class InitializerBaseEditor : InitializerEditor
	{
		protected override bool HasUserDefinedInitArgumentFields => true;
	}
}