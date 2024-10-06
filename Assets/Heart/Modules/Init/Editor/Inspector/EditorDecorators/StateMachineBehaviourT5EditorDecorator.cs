using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT5EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<,,,,>);
		public StateMachineBehaviourT5EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}