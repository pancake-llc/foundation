using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	[CanEditMultipleObjects]
    public sealed class InitializableT7Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,>);
    }
}