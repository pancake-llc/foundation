using System;
using UnityEditor;

namespace Pancake.Editor.Init
{
	[CanEditMultipleObjects]
    public sealed class InitializableT4Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(IInitializable<,,,>);
    }
}