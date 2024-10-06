using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT10EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,,,,>);
		public MonoBehaviourT10EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}