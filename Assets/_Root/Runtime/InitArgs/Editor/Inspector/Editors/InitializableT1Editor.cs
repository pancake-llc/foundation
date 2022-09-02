using System;
using UnityEditor;

namespace Pancake.Editor.Init
{
	[CanEditMultipleObjects]
    public class InitializableT1Editor : InitializableEditor
    {
		protected override Type BaseTypeDefinition => typeof(IInitializable<>);
    }
}