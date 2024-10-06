using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT7EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,>);
		public InitializableT7EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}