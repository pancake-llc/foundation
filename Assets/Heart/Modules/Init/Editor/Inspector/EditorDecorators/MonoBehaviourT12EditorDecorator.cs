using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT12EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,,,,,,>);
		public MonoBehaviourT12EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}