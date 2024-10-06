using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT1EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<>);
		public MonoBehaviourT1EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}