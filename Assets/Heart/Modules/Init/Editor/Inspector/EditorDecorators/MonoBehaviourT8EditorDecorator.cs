using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT8EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,,>);
		public MonoBehaviourT8EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}