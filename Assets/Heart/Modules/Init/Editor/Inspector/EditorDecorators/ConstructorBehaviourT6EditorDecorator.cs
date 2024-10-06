using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class ConstructorBehaviourT6EditorDecorator : BaseConstructorBehaviourEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,,,,,>);
		public ConstructorBehaviourT6EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}