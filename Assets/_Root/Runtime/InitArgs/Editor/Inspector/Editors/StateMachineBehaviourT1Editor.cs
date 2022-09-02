using System;
using UnityEditor;
using Pancake.Init;

namespace Pancake.Editor.Init
{
	[CustomEditor(typeof(StateMachineBehaviour<>), true, isFallback = true), CanEditMultipleObjects]
    public class StateMachineBehaviourT1Editor : InitializableEditor
    {
        protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<>);
    }
}