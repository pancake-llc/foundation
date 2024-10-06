using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class ConstructorBehaviourT5EditorDecorator : BaseConstructorBehaviourEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,,,,>);
		public ConstructorBehaviourT5EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}