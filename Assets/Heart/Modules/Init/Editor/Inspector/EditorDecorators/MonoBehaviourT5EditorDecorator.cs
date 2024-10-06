using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT5EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,>);
		public MonoBehaviourT5EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}