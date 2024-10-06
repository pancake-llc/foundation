using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class MonoBehaviourT9EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(MonoBehaviour<,,,,,,,,>);
		public MonoBehaviourT9EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}