using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class ConstructorBehaviourT2EditorDecorator : BaseConstructorBehaviourEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,>);
		public ConstructorBehaviourT2EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}