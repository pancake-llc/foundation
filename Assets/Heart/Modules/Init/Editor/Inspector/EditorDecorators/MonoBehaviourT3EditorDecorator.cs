using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT3EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,>);
		public MonoBehaviourT3EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}