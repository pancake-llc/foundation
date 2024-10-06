using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT2EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,>);
		public MonoBehaviourT2EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}