using Sisus.Init.Internal;
using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	[CustomEditor(typeof(InactiveInitializer), editorForChildClasses:true, isFallback = false), CanEditMultipleObjects]
	public class InactiveInitializerEditor : InitializerEditor
	{
		protected override bool HasUserDefinedInitArgumentFields => true;
	}
}