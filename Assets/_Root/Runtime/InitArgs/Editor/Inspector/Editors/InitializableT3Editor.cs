using System;
using UnityEditor;

namespace Pancake.Init.EditorOnly
{
	[CanEditMultipleObjects]
    public sealed class InitializableT3Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(IInitializable<,,>);
    }
}