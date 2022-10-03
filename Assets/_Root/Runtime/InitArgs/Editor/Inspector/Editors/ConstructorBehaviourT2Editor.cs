using System;
using UnityEditor;

namespace Pancake.Init.EditorOnly
{
    [CustomEditor(typeof(ConstructorBehaviour<,>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class ConstructorBehaviourT2Editor : BaseConstructorBehaviourEditor
    {
        protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,>);
    }
}