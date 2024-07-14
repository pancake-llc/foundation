using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
    [CustomEditor(typeof(MonoBehaviour<,,>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class MonoBehaviourT3Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,>);
    }
}