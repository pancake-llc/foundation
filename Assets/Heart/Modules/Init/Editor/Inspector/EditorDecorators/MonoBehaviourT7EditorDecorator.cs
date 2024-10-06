using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT7EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,>);
		public MonoBehaviourT7EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}