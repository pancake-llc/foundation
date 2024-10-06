using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class StateMachineBehaviourT6EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(StateMachineBehaviour<,,,,,>);
		public StateMachineBehaviourT6EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}