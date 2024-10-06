using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT2EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,>);
		public InitializableT2EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}