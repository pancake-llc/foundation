using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT3EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<,,>);
		public StateMachineBehaviourT3EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}