using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT2EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<,>);
		public StateMachineBehaviourT2EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}