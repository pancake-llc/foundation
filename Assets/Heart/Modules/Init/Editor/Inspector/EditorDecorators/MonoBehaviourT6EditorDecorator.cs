using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT6EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,>);
		public MonoBehaviourT6EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}