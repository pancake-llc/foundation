using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class ConstructorBehaviourT4EditorDecorator : BaseConstructorBehaviourEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,,,>);
		public ConstructorBehaviourT4EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}