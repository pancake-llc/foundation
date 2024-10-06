using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT4EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,>);
		public MonoBehaviourT4EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}