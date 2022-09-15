using System;
using UnityEditor;
using Pancake.Init;

namespace Pancake.Editor.Init
{
    [CustomEditor(typeof(ConstructorBehaviour<>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class ConstructorBehaviourT1Editor : BaseConstructorBehaviourEditor
    {
        protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<>);
    }
}