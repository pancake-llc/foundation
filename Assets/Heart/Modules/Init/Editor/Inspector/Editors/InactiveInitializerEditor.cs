using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	[CanEditMultipleObjects]
    public class InactiveInitializerEditor : InitializerEditor
    {
		protected override bool HasUserDefinedInitArgumentFields => true;
	}
}