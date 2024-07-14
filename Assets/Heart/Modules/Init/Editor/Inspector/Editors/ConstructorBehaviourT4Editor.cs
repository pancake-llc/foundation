using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
    [CustomEditor(typeof(ConstructorBehaviour<,,,>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class ConstructorBehaviourT4Editor : BaseConstructorBehaviourEditor
    {
        protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,,,>);
    }
}