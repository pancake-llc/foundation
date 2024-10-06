using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT4EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<,,,>);
		public StateMachineBehaviourT4EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}