using System;
using Pancake.Init;
using UnityEditor;

namespace Pancake.Editor.Init
{
    [CustomEditor(typeof(MonoBehaviour<,>), true, isFallback = true), CanEditMultipleObjects]
    public class MonoBehaviourT2Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,>);
    }
}