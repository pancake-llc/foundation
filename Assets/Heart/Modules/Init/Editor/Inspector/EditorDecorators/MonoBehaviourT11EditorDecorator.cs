using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT11EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,,,,,>);
		public MonoBehaviourT11EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}