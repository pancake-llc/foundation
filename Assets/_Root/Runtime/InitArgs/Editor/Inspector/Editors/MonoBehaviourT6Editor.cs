using System;
using UnityEditor;

namespace Pancake.Init.EditorOnly
{
    [CustomEditor(typeof(MonoBehaviour<,,,,,>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class MonoBehaviourT6Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,>);
    }
}