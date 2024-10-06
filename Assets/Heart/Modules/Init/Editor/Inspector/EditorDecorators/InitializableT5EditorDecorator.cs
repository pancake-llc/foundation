using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT5EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,>);
		public InitializableT5EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}