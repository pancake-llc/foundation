using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT1EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<>);
		public StateMachineBehaviourT1EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}