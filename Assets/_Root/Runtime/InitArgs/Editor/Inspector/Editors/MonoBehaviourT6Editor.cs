using System;
using Pancake.Init;
using UnityEditor;

namespace Pancake.Editor.Init
{
    [CustomEditor(typeof(MonoBehaviour<,,,,,>), true, isFallback = true), CanEditMultipleObjects]
    public sealed class MonoBehaviourT6Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,>);
    }
}